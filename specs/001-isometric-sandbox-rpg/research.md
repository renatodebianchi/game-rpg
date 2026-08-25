# Phase 0 Research: RPG Sandbox com Árvore de Habilidades, Combate Tático e Mundo Reativo

**Feature**: [spec.md](./spec.md) | **Date**: 2026-08-25

Este documento resolve todas as marcações `NEEDS CLARIFICATION` do Technical Context do
[plan.md](./plan.md), com base na entrada explícita do usuário ("usar a engine Unity, manter
perspectiva de jogabilidade fluída e dinâmica, boa ambientação, manter as premissas listadas na
spec") e nas premissas já registradas em `spec.md`.

## Decision: Engine e linguagem

- **Decision**: Unity (LTS mais recente disponível no início do projeto, ex.: Unity 6 LTS),
  usando C# como linguagem principal.
- **Rationale**: Requisito explícito do usuário. Unity oferece suporte maduro a câmeras
  isométricas, Tilemap 2D e renderização 3D com projeção isométrica, sistemas de animação e
  iluminação que atendem ao pedido de "jogabilidade fluída e dinâmica" e "boa ambientação".
  Também possui um ecossistema grande de ferramentas para RPGs táticos (pathfinding em grade,
  ScriptableObjects para dados, Timeline para cenas narrativas).
- **Alternatives considered**: Godot (mais leve, mas ecossistema de ferramentas de RPG tático
  isométrico menos maduro) e engine própria (rejeitada por violar o Princípio V — Simplicidade —
  ao introduzir complexidade de engine sem necessidade concreta).

## Decision: Renderização e câmera isométrica

- **Decision**: Universal Render Pipeline (URP) com câmera ortográfica em ângulo isométrico
  (ou pseudo-isométrico 2.5D), usando Cinemachine para controle de câmera fluido (transições
  suaves entre exploração e combate).
- **Rationale**: URP equilibra qualidade visual ("boa ambientação") com performance previsível
  em hardware de PC de médio porte, alinhado ao Princípio IV (orçamentos de performance).
  Cinemachine é a ferramenta padrão da Unity para câmeras dinâmicas sem lógica customizada
  excessiva.
- **Alternatives considered**: HDRP (rejeitado: exige hardware mais potente, desnecessário para
  o estilo de arte isométrico do MVP) e câmera scripted manualmente (rejeitado por violar
  Simplicidade frente a uma solução já madura no engine).

## Decision: Movimento e combate em grade

- **Decision**: Grid lógico customizado (estrutura de dados própria de célula/tile) desacoplado
  da representação visual (Tilemap/posições de mundo), com um serviço de pathfinding em grade
  (A* customizado ou pacote leve de pathfinding em grade) usado apenas durante o modo de combate
  e para movimentação assistida na exploração.
- **Rationale**: Mantém a lógica de combate (Princípio II — modular e testável) independente de
  Tilemap/render, permitindo testes automatizados de EditMode sobre o grid sem depender de cena
  carregada. Evita acoplar regras de turno à camada de apresentação.
- **Alternatives considered**: Usar NavMesh da Unity diretamente para tudo (rejeitado: NavMesh é
  otimizado para movimento livre/contínuo, não para grade tática com custo de ação por casa) e
  bibliotecas de terceiros pesadas de RPG tático completo (rejeitado: risco de complexidade
  desnecessária/licenciamento, contraria Simplicidade).

## Decision: Dados de conteúdo (habilidades, itens, NPCs, vilas)

- **Decision**: `ScriptableObject` para definir nós da árvore de habilidades, itens/recursos,
  arquétipos de NPC e configuração de vilas/facções, versionados como assets no projeto Unity.
- **Rationale**: Atende diretamente ao Princípio II (arquitetura modular e orientada a dados):
  designers podem criar/balancear conteúdo (novas habilidades, vilas, recursos) sem alterar
  código. Cada `ScriptableObject` funciona como um contrato de dados testável isoladamente.
- **Alternatives considered**: Arquivos JSON externos carregados em runtime (viável, mas perde
  integração com o editor da Unity — validação de referências, drag-and-drop — sem benefício
  claro para um jogo single-player local) e dados hard-coded em código (rejeitado: viola
  Princípio II).

## Decision: Persistência (save/load)

- **Decision**: Serialização local em JSON (ou binário, a definir na implementação) gravada em
  `Application.persistentDataPath`, cobrindo progresso do personagem, árvore de habilidades
  adquirida, indicadores de sobrevivência, reputação por comunidade e estado de recursos/
  população de cada vila.
- **Rationale**: Premissa da spec: "persistência de progresso assume salvamento local (não é
  necessário sincronização em nuvem nesta fase)". JSON favorece depuração e testes automatizados
  de save/load (exigidos como não-negociáveis pelo Princípio III).
- **Alternatives considered**: Banco de dados embutido (SQLite) — rejeitado por complexidade
  desnecessária frente ao volume de dados de um único personagem/mundo local (Simplicidade);
  serviço de save em nuvem — fora de escopo (premissa da spec).

## Decision: Testes

- **Decision**: Unity Test Framework (baseado em NUnit), com testes de **EditMode** para lógica
  pura (fórmulas de combate, regras da árvore de habilidades, simulação de fome/sanidade,
  cálculo de reputação e economia de vila) e testes de **PlayMode** para fluxos integrados
  (encontro de combate completo, ciclo de save/load, transição de estado de vila ao longo do
  tempo simulado).
- **Rationale**: Atende diretamente ao Princípio III (NON-NEGOTIABLE): combate, fórmulas,
  inventário/recursos, save/load e transições de estado de quests/reputação exigem testes
  automatizados. Unity Test Framework é a ferramenta nativa e evita dependências externas
  desnecessárias.
- **Alternatives considered**: Testes manuais via playtesting apenas (rejeitado: viola
  explicitamente o Princípio III) e framework de testes de terceiros (rejeitado: Unity Test
  Framework já cobre a necessidade sem custo adicional de integração).

## Decision: Plataforma-alvo e orçamento de performance

- **Decision**: PC desktop (Windows como plataforma primária de desenvolvimento/validação;
  build standalone), com meta de 60 fps em hardware de médio porte durante exploração e
  combate, e latência de resposta a input (movimento, ataque, navegação de menu/árvore de
  habilidades) abaixo de 100ms percebidos.
- **Rationale**: Alinhado ao pedido de "jogabilidade fluída e dinâmica" e ao Princípio IV, que
  exige um orçamento explícito de performance antes da construção de cada sistema. PC desktop é
  o alvo mais direto para um RPG tático isométrico single-player no estilo referenciado
  (Baldur's Gate 3), sem introduzir a complexidade adicional de portar para console/mobile no
  MVP.
- **Alternatives considered**: Suporte multiplataforma completo (console/mobile) desde o MVP —
  rejeitado por violar Simplicidade/Escopo Iterativo (Princípio V); pode ser revisitado em
  iterações futuras via nova spec.

## Decision: Escopo/Escala do MVP

- **Decision**: Uma região jogável com um pequeno número de vilas interligadas (ordem de
  grandeza: 2–4 vilas), população de NPCs simulados por vila na casa de dezenas (não milhares),
  árvore de habilidades com dezenas de nós distribuídos entre as trilhas combatente, arcanista
  e nós híbridos.
- **Rationale**: Mantém a premissa de escopo já registrada na spec (recorte de mundo suficiente
  para demonstrar a simulação de reputação/economia) e respeita Simplicidade/Escopo Iterativo,
  evitando simular uma economia de mundo aberto completa antes de validar o loop principal.
- **Alternatives considered**: Mundo aberto completo desde o MVP — rejeitado (risco de nunca
  finalizar o MVP, conflita com Princípio V).

## Resumo das resoluções de Technical Context

| Campo | Resolução |
|---|---|
| Language/Version | C# (Unity, LTS mais recente disponível) |
| Primary Dependencies | Unity URP, Cinemachine, Unity Input System, ScriptableObjects, Unity Test Framework |
| Storage | Arquivos locais (JSON) em `Application.persistentDataPath` |
| Testing | Unity Test Framework (EditMode + PlayMode) |
| Target Platform | PC desktop (Windows primário) |
| Project Type | Jogo desktop single-player (projeto único Unity) |
| Performance Goals | 60 fps em hardware de médio porte; input responsivo (<100ms percebido) |
| Constraints | Offline-capable; sem dependência de serviços de rede no MVP |
| Scale/Scope | 1 região, 2–4 vilas, dezenas de NPCs simulados, dezenas de nós de habilidade |

## Nota de governança

A engine/stack acima resolve o `TODO(TECH_STACK)` deixado em aberto em
`.specify/memory/constitution.md`. Recomenda-se rodar `/speckit-constitution` como ação
separada para registrar formalmente Unity/C# na seção "Technical Constraints" da constituição
(fora do escopo deste comando de planejamento).
