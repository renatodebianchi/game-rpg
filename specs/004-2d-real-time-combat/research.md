# Phase 0 Research: Combate em Tempo Real 2D (estilo Tales of Phantasia)

**Feature**: [spec.md](./spec.md) | **Date**: 2026-09-05

Este documento resolve o Technical Context de [plan.md](./plan.md). A stack técnica do projeto
(Unity 6000.5.9f1, C#, URP, UGUI) já foi decidida nas features 001/002/003; esta feature apenas
troca o renderer 3D pelo 2D e substitui a arquitetura de combate por turnos.

## Decision: URP 2D Renderer substitui o Universal Renderer 3D

- **Decision**: Trocar o `UniversalRendererData` (3D) criado por `ProjectBootstrap.CreateUrpAsset`
  por um `Renderer2DData` (URP 2D Renderer), e usar câmeras ortográficas sem rotação isométrica
  (`Quaternion.identity`, olhando ao longo de -Z, convenção padrão 2D do Unity) em vez do
  `Euler(35.264, 45, 0)` usado desde a feature 001.
- **Rationale**: É a forma nativa e já suportada pelo URP (já instalado, sem pacote novo) de
  renderizar um jogo 2D "de verdade" (sprites, iluminação 2D se necessário no futuro), em vez de
  continuar aproximando 2D com sprites *billboard* dentro de uma cena 3D isométrica (abordagem
  usada nas features 001/003). Atende FR-001/FR-011 diretamente.
- **Alternatives considered**: Manter o Universal Renderer 3D e continuar usando sprites
  *billboard* (abordagem da feature 003) — rejeitado porque o pedido desta feature é
  explicitamente mudar "a abordagem visual do jogo de 3D para 2D", não apenas achatar a
  aparência dentro de uma cena ainda 3D.

## Decision: Arena de combate como espaço contínuo 1D (`BattleArena`), não mais um grid

- **Decision**: Substituir `GridMap`/`GridCoordinate` por `BattleArena`, que define apenas
  limites horizontais mínimo/máximo (um intervalo `float`); a posição de um combatente passa a
  ser um único `float` (`PositionX`), sem coordenada de grid nem ocupação de célula.
- **Rationale**: FR-002 exige "movimento livre (não restrito a um grid) ao longo do eixo
  horizontal" — um grid discreto contradiz diretamente esse requisito. Um intervalo contínuo é o
  modelo mais simples que atende exatamente ao pedido (Princípio V).
- **Alternatives considered**: Manter um grid fino (muitas células pequenas) para simular
  continuidade — rejeitado por ser complexidade acidental; um `float` já é "contínuo" de
  verdade, sem precisar simular.

## Decision: Ações em tempo real (`RealTimeActionDefinition` + `RealTimeActionExecutor`) substituem `ActionResolver`/`TurnResources`

- **Decision**: Introduzir `RealTimeActionDefinition` como dado (`ScriptableObject`, mesmo
  padrão de `SkillNodeDefinition`) com `Range`, `ExecutionTime` (tempo de conjuração/execução) e
  `ResourceCost` (gasto de um novo recurso de combate, ver próxima decisão). Um único
  `RealTimeActionExecutor` (por combatente ou compartilhado, com estado por combatente em
  `CombatantActionState`) inicia a ação, aguarda `ExecutionTime` (interrompível, FR-009) e só
  então aplica dano/efeito, reaproveitando o pipeline de `IDamageModifier` já existente em
  `ActionResolver` (renomeado para uma interface `IDamageModifierRegistry` que ambos podem
  implementar/consumir).
- **Rationale**: Preserva o investimento já feito em `IDamageModifier`/`HungerSystem`/
  `SanitySystem`/`CapabilityResolver` (FR-012, Princípio II) — só a decisão de "quando" uma ação
  se resolve muda (tempo real com tempo de execução, em vez de instantâneo por turno). Ser
  orientado a dados atende FR-005 (tempo de conjuração configurável) sem precisar de um novo
  sistema de habilidades.
- **Alternatives considered**: Reaproveitar `ActionResolver` como está, só chamando-o
  repetidamente — rejeitado porque `ActionResolver.ResolveBasicAttack` é uma operação
  instantânea (sem noção de tempo de execução/interrupção), incompatível com FR-005/FR-009.

## Decision: Um novo recurso de combate em tempo real ("Pontos de Técnica") substitui a checagem de `ActionAvailable` por turno

- **Decision**: Cada combatente ganha um `CombatantActionState` com um recurso limitado
  (pontos de técnica) que regenera lentamente com o tempo (não instantaneamente a cada turno) e
  é gasto por habilidades/magias (FR-008); ataques básicos (corpo a corpo/à distância) não
  custam esse recurso, mas têm um tempo de recarga próprio (`Cooldown` em
  `RealTimeActionDefinition`) para não serem espamáveis sem limite.
- **Rationale**: FR-008 exige que habilidades/magias continuem tendo um custo real em combate;
  em um modelo sem turnos, "uma ação por turno" deixa de fazer sentido — um recurso que
  regenera com o tempo é o equivalente direto mais simples (e é literalmente o mecanismo usado
  pelo próprio Tales of Phantasia, TP).
- **Alternatives considered**: Reaproveitar `AvailableSkillPoints` (pontos de habilidade da
  árvore, feature 001 US2) como o recurso gasto em combate — rejeitado por misturar dois
  conceitos diferentes (pontos de progressão permanente vs. recurso de combate que se recupera);
  manteria `AvailableSkillPoints` intacto, só para desbloquear nós da árvore, como já é hoje.

## Decision: IA de inimigo contínua (`EnemyCombatAI`) substitui `EnemyAI.TakeTurn`

- **Decision**: `EnemyCombatAI.Tick(TimeSpan delta)` decide continuamente mover em direção ao
  alvo mais próximo ou atacar quando ao alcance, chamado a cada frame (ou a um intervalo curto
  de decisão) em vez de uma vez por turno. Sem pathfinding (`GridPathfinding` removido): a arena
  não tem obstáculos (Assumptions da spec), então mover em direção ao alvo é uma linha reta no
  eixo horizontal.
- **Rationale**: FR-007 exige que inimigos ajam "a qualquer momento... de forma independente e
  simultânea"; a essência da IA (perseguir e atacar ao alcance) é preservada da feature 001, só
  a cadência (contínua vs. por turno) muda. Remover pathfinding é uma simplificação justificada
  pela ausência de obstáculos na arena (Princípio V).
- **Alternatives considered**: Manter `GridPathfinding` "por precaução" para uma arena que possa
  ganhar obstáculos no futuro — rejeitado por não haver necessidade concreta agora (YAGNI,
  Princípio V); pode ser reintroduzido quando/se uma feature futura pedir obstáculos.

## Decision: Fuga por canal contínuo (`RealTimeFleeAction`) substitui a fuga como ação de turno

- **Decision**: Fugir exige segurar o comando de movimento em direção a uma borda da arena por
  um tempo contínuo mínimo (um "canal", que reseta se interrompido); ao completar o canal, a
  chance de sucesso é calculada com a mesma fórmula já validada em `FleeAction`
  (distância até o hostil mais próximo + Destreza), adaptada para usar distância `float` em vez
  de `GridCoordinate.ManhattanDistance`.
- **Rationale**: FR-013/Edge Cases exigem fuga sem depender de uma ação de menu de turno,
  mantendo a mesma fórmula de chance já testada (reuso direto, sem reinventar o balanceamento).
- **Alternatives considered**: Fuga instantânea ao encostar na borda — rejeitado por remover
  completamente o risco/custo de tentar fugir, mudando o balanceamento herdado da feature 001
  sem necessidade.

## Decision: Câmera com clamp nas bordas (`BoundedFollowCamera`) substitui `CombatCameraController`

- **Decision**: Um único componente `BoundedFollowCamera` (não-Cinemachine, script direto no
  mesmo espírito de `Demo.DemoCameraController`) segue um alvo (`Transform`) centralizando-o na
  tela, mas força a posição da câmera a ficar dentro de um retângulo de limites do
  mundo (calculado a partir dos limites do mapa/arena e do tamanho ortográfico da câmera),
  compartilhado pela Exploração e pela arena de combate (FR-015).
- **Rationale**: É a implementação mais direta e transparente do requisito (Princípio V);
  evita depender de `CinemachineConfiner2D` (que exigiria gerar/cachear uma forma de colisão 2D
  em tempo de execução) para um comportamento que um cálculo de `Mathf.Clamp` resolve
  diretamente, e mantém o padrão já estabelecido no projeto de câmeras construídas via
  script simples, não via prefabs/Cinemachine para as câmeras de demo.
- **Alternatives considered**: `Unity.Cinemachine.CinemachineConfiner2D` com um
  `PolygonCollider2D` delimitando a arena/mapa — avaliado e rejeitado para esta feature por
  adicionar uma peça de configuração (forma de colisão + cache) sem necessidade concreta além do
  que o clamp direto já resolve; pode ser reconsiderado se uma feature futura precisar de bordas
  não-retangulares.

## Decision: Ataque à distância continua condicionado a uma capacidade adquirida (reuso de `CapabilityResolver`)

- **Decision**: A capacidade de usar ataque à distância (FR-004) continua sendo verificada via
  `Character.AcquiredSkillNodeIds`/`CapabilityResolver`, com uma convenção de id de capacidade
  dedicada (ex.: `capability.ranged_attack`) atribuível a um nó da árvore de habilidades já
  existente — sem introduzir um novo sistema de resolução de capacidades.
- **Rationale**: Atende FR-004 e preserva FR-012 (a árvore de habilidades não muda de regra),
  reaproveitando a infraestrutura já testada da feature 001 US2.
- **Alternatives considered**: Toda habilidade concede ataque à distância por padrão (sem
  checagem de capacidade) — rejeitado por remover a progressão que a árvore de habilidades já
  modela.

## Resumo das resoluções de Technical Context

| Campo | Resolução |
|---|---|
| Language/Version | C# (Unity 6000.5.9f1 — mesma versão das features 001/002/003) |
| Primary Dependencies | URP com 2D Renderer (`Renderer2DData`); nenhum pacote novo do Package Manager |
| Storage | Inalterado (JSON local) |
| Testing | Unity Test Framework — EditMode (arena, execução/interrupção de ação, IA de inimigo, canal de fuga) e PlayMode (fluxo completo de encontro) |
| Target Platform | PC desktop (mesmo alvo das features 001/002/003) |
| Project Type | Substituição do combate e da câmera dentro do mesmo projeto Unity único |
| Performance Goals | 60 fps; <100ms de latência input-para-ação (mais crítico que no modelo por turnos) |
| Constraints | Regras de atributos/habilidades/sobrevivência/reputação-economia inalteradas (FR-012); código de grid/turnos removido, não deixado como código morto |
| Scale/Scope | ~10 arquivos de combate substituídos; troca do renderer URP; 1 componente de câmera compartilhado |
