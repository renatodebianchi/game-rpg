# Créditos de Assets de Terceiros — Cena de Teste de Mobilidade

Registro exigido por FR-013 (spec `005-mobility-test-scene`).

## Legacy-Fantasy – High Forest (v2.3)

- **Autor/criador**: Anokolisa
- **Fonte**: https://anokolisa.itch.io/sidescroller-pixelart-sprites-asset-pack-forest-16x16
- **Licença**: Gratuito, inclusive para uso comercial (declarado pelo autor na página do
  pacote e em respostas a comentários). Não é CC0 formal — o autor disponibilizou uma licença
  customizada própria (documento: https://drive.google.com/file/d/17y1gjuwVirE8V79WcTL9tgTUtMu6R1je/view).
  Atribuição não é estritamente exigida pela licença, mas é registrada aqui por boa prática,
  como o próprio autor pede na página do pacote.
- **Uso neste projeto**: `Assets/Art/Platformer/Resources/Character/*` — quadros de animação
  recortados das spritesheets originais (Idle, Run, Jump-Start, Jump-All/Air Jump) do "Level 1
  warrior character", usados por `SpriteFlipbookAnimator` para cada estado de movimentação
  (`MovementStateKind`) em `PlatformerMovementController`.
  `Assets/Art/Platformer/Resources/Environment/*` — tiles do ambiente ("Forest Tiles") usados
  como chão/plataformas/paredes da cena `MobilityTest.unity`, e o fundo de duas camadas
  ("Background") usado como textura de fundo (FR-012).
