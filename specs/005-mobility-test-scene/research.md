# Phase 0 Research: Cena de Teste de Mobilidade

**Feature**: [spec.md](./spec.md) | **Date**: 2026-09-05

Este documento resolve o Technical Context de [plan.md](./plan.md). A stack técnica do projeto
(Unity 6000.5.9f1, C#, URP 2D Renderer desde a feature 004) é reutilizada sem alterações; esta
feature resolve apenas as decisões específicas do controlador de movimentação e dos novos
assets visuais.

## Decision: Assets de personagem/cenário — Anokolisa "Legacy Fantasy – High Forest" (itch.io, gratuito)

- **Decision**: Usar o pacote [Legacy Fantasy – High Forest](https://anokolisa.itch.io/sidescroller-pixelart-sprites-asset-pack-forest-16x16)
  do criador Anokolisa no itch.io (600+ sprites, pixel art 16×16) como fonte única do sprite do
  personagem (guerreiro com animações de Run, Idle, Attack ×2, Start Jump, Air Jump, End Jump,
  Die) e do tileset de chão/plataformas/paredes (Forest/Ruins/Lake/Cave Tiles, árvores) e do
  fundo (background de duas camadas com céu, incluído no pacote).
- **Rationale**: Substitui a escolha anterior (Kenney "Pixel Platformer") a pedido explícito do
  usuário. Um único pacote continua cobrindo FR-001 (tileset), FR-011 (poses do personagem) e
  FR-012 (fundo) sem combinar múltiplas fontes. É gratuito para uso comercial (declarado pelo
  autor na página do pacote), mesmo não sendo CC0 formal — licença customizada do autor,
  disponibilizada junto ao pacote; atribuição não é estritamente exigida, mas é registrada em
  `CREDITS.md` por boa prática (mesma política de créditos já usada na feature 003).
- **Mapeamento de poses (melhor esforço, mesmo princípio da feature 003)**: este pacote não tem
  uma pose dedicada de agachar nem de deslizar na parede — `Idle` é reaproveitado como base
  visual para `Crouching` (ajustado por escala/posição no código, sem exigir um quadro novo), e
  `Air Jump` é reaproveitado para `WallSliding`/`Falling`; `Start Jump` cobre `Jumping`,
  `Air Jump` também cobre `DoubleJumping`. Nenhum desses casos é tratado como erro — é o mesmo
  princípio de fallback já documentado na feature 003.
- **Alternatives considered**: Manter o Kenney "Pixel Platformer" — descartado por pedido
  explícito do usuário de trocar a fonte para Anokolisa. Outro pacote de Anokolisa (ex. o
  "Pixel Crawler", de perspectiva top-down) — rejeitado por não ser lateral/side-view, incompatível
  com a perspectiva desta cena (FR-001).
- **Nota de licenciamento/download**: Diferente do CDN estático do Kenney.nl, o itch.io não
  expõe um link de download direto anônimo — o arquivo é obtido através da própria página do
  pacote. O download ainda requer confirmação explícita do usuário (nome do arquivo, fonte,
  tamanho) antes de ser realizado durante a implementação, e pode exigir usar o navegador em vez
  de um download direto por linha de comando.

## Decision: Controlador de movimentação via Rigidbody2D + raycasts (não um novo pacote)

- **Decision**: Implementar o movimento do personagem com `Rigidbody2D` (modo Dynamic,
  gravityScale configurável, rotação travada) manipulado por velocidade
  (`Rigidbody2D.linearVelocity`), com detecção de chão/parede via `Physics2D.Raycast` a partir
  das bordas do `BoxCollider2D` do personagem — a técnica padrão para controladores de
  plataforma 2D em Unity, usando apenas o módulo de Physics2D já embutido no motor (nenhum
  pacote novo do Package Manager).
- **Rationale**: Resolve corretamente colisão contra chão/plataformas/paredes "de graça" via o
  motor de física do Unity, o que seria arriscado e complexo de reimplementar manualmente
  (essencial para SC-003, "nunca atravessar uma parede/chão/plataforma"). É a abordagem mais
  simples que resolve esse problema real (Princípio V) sem introduzir uma nova dependência.
- **Alternatives considered**: Um `CharacterController` totalmente customizado por
  manipulação direta de `transform.position` (mesmo padrão simplificado usado por
  `ExplorationCharacterController`/`BattleArenaDemoController`) — rejeitado aqui porque essas
  duas cenas não têm paredes/plataformas para colidir; esta cena tem exatamente esse requisito
  como núcleo do teste (FR-001, SC-003), o que exige resolução de colisão real.

## Decision: Máquina de estados de movimentação como classe C# pura (`PlatformerMovementState`)

- **Decision**: A lógica de decisão (quantos pulos restam, se pode pular/deslizar na parede, o
  acúmulo e liberação de energia do salto carregado) vive em uma classe C# pura
  (`PlatformerMovementState`), avançada explicitamente por um `MonoBehaviour`
  (`PlatformerMovementController`) que só alimenta os resultados das checagens de física
  (grounded/tocando parede) e aplica a velocidade resultante ao `Rigidbody2D` — mesmo padrão já
  estabelecido em `Combat.CombatantActionState` (feature 004).
- **Rationale**: Atende ao Princípio III (testes automatizados para lógica central) sem
  precisar de uma cena Unity carregada para testar regras como "não há um terceiro pulo no ar"
  (FR-005) ou "energia acumulada define a altura do salto" (FR-010) — testável via EditMode
  puro, como as demais features.
- **Alternatives considered**: Colocar toda a lógica diretamente no `MonoBehaviour` (padrão
  comum em tutoriais de plataforma) — rejeitado por tornar as regras de negócio centrais
  não-testáveis sem uma cena carregada, violando o Princípio III.

## Decision: Animação por troca de sprite via código, sem Animator Controller

- **Decision**: Cada estado de movimentação mapeia para um conjunto de sprites do pacote Pixel
  Platformer, ciclados por um pequeno componente (`SpriteFlipbookAnimator`) que troca
  `SpriteRenderer.sprite` a um intervalo fixo — sem usar o sistema Animator/Mecanim do Unity
  (sem `.anim`/`.controller` autorados).
- **Rationale**: Consistente com o padrão já estabelecido no projeto de construir tudo via
  código em vez de grafos de asset autorados no Editor (ver `DemoUiKit`, `ProjectBootstrap`) —
  evita depender de uma sessão de Editor GUI para configurar transições de Animator, que não é
  confiável neste ambiente (mesma razão documentada em decisões anteriores do projeto).
- **Alternatives considered**: `Animator` + `AnimatorController` com transições — rejeitado por
  exigir autoria de asset gráfico (grafo de estados) tipicamente feita na GUI do Editor, fora do
  fluxo headless já estabelecido; um `SpriteFlipbookAnimator` simples resolve o mesmo problema
  via código puro.

## Decision: Câmera reaproveitada (`BoundedFollowCamera`, feature 004)

- **Decision**: A cena de teste de mobilidade usa o mesmo `Demo.BoundedFollowCamera`
  (introduzido na feature 004) para seguir o personagem com clamp nas bordas do cenário, sem
  nenhum componente de câmera novo.
- **Rationale**: O componente já resolve exatamente esse problema (seguir + não revelar espaço
  vazio nas bordas) de forma genérica, reutilizável por qualquer cena 2D — reaproveitá-lo evita
  duplicar lógica de câmera pela terceira vez no projeto (Princípio V).
- **Alternatives considered**: Câmera fixa sem seguir o personagem — rejeitado porque o cenário
  de teste é maior que a tela (precisa de plataformas/paredes suficientes para testar todas as
  habilidades), então a câmera precisa acompanhar o personagem.

## Resumo das resoluções de Technical Context

| Campo | Resolução |
|---|---|
| Language/Version | C# (Unity 6000.5.9f1 — mesma versão das features 001-004) |
| Primary Dependencies | `UnityEngine.Physics2D` (módulo já embutido, sem pacote novo); assets binários de terceiros (Anokolisa "Legacy Fantasy – High Forest", itch.io) |
| Storage | N/A (cena de teste sem persistência) |
| Testing | Unity Test Framework — EditMode (`PlatformerMovementState`: pulo duplo, deslizar/pular na parede, acúmulo/liberação de energia) |
| Target Platform | PC desktop (mesmo alvo das features 001-004) |
| Project Type | Extensão do mesmo projeto Unity único — nova cena de demo isolada |
| Performance Goals | 60 fps; resposta de input de movimentação em within um frame (mesmo orçamento já usado no combate em tempo real, feature 004) |
| Constraints | Assets de terceiros DEVEM ser de fontes abertas/gratuitas com créditos registrados (FR-013 — não necessariamente CC0 formal, a licença da Anokolisa é customizada); nenhum download sem confirmação explícita do usuário; a cena não pode alterar a movimentação já existente em Exploração/Combate (FR-014); regras de jogo (parede, energia) DEVEM viver em `PlatformerMovementState`, nunca só no `MonoBehaviour` (Princípio III) |
| Scale/Scope | 1 cena nova, 1 pacote de assets, 1 controlador de movimentação com ~9 estados |
