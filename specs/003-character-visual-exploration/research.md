# Phase 0 Research: Assets Visuais do Personagem e da Interface + Exploração com Personagem Criado

**Feature**: [spec.md](./spec.md) | **Date**: 2026-08-26

Este documento resolve o Technical Context de [plan.md](./plan.md). A stack técnica do projeto
(Unity 6000.5.9f1, C#, URP, UGUI) já foi decidida nas features 001/002 e é reutilizada sem
alterações; esta feature resolve apenas as decisões específicas de assets visuais e da nova
demo de Exploração jogável.

## Decision: Pacote de sprite do personagem — Kenney "Roguelike Characters" (CC0)

- **Decision**: Usar o pacote [Roguelike Characters](https://kenney.nl/assets/roguelike-characters)
  do Kenney.nl (450 variações de sprite, formato spritesheet, licença CC0) como fonte do
  sprite humanoide 2D do personagem.
- **Rationale**: É um pacote de sprites *top-down* pequenos (pixel art), compatível com a
  câmera isométrica/ortográfica já usada nas demos; licença CC0 confirmada (domínio público,
  sem exigência de atribuição), satisfazendo FR-006/FR-009. O grande número de variações de
  sprite permite escolher visuais suficientemente distintos para dar uma sensação de
  personalização, mesmo sem cobrir exatamente as combinações de `VisualCharacteristics` da
  feature 002.
- **Alternatives considered**: Um modelo 3D humanoide rigado (ex. de fontes como Mixamo/
  Quaternius) — rejeitado na fase de clarificação da spec por introduzir rig/animação 3D fora
  do escopo definido (Princípio V). Pacotes pagos ou com exigência de atribuição obrigatória
  não documentada — rejeitados por FR-006/FR-009.
- **Nota de licenciamento/download**: O download efetivo do arquivo `.zip` deste pacote requer
  minha confirmação explícita com o usuário (nome do arquivo, fonte, tamanho) antes de ser
  realizado durante a implementação, por ser um download de arquivo de terceiro.

## Decision: Pacote de interface — Kenney "UI Pack" (CC0)

- **Decision**: Usar o pacote [UI Pack](https://kenney.nl/assets/ui-pack) do Kenney.nl (430
  arquivos: botões, painéis, sliders, checkboxes, 2 fontes TTF, licença CC0) como fonte dos
  assets de interface (botões, painéis, fonte).
  URL de download direto confirmada:
  `https://kenney.nl/media/pages/assets/ui-pack/f651646eab-1718203990/kenney_ui-pack.zip`
- **Rationale**: Mesmo pacote da mesma fonte já validada (Kenney, CC0), cobre exatamente os
  três tipos de elemento que os componentes de UI compartilhados do projeto já usam (botão,
  painel, texto), incluindo fontes prontas (substituindo a fonte legada `LegacyRuntime.ttf`
  usada hoje pelas demos).
- **Alternatives considered**: Pacotes temáticos (UI Pack — Adventure/Sci-Fi/RPG Expansion) —
  não escolhidos para o MVP por não terem necessidade concreta de tema fantasia específico
  agora (Princípio V); podem ser adotados depois sem mudança de arquitetura, já que o ponto de
  aplicação é centralizado (ver próxima decisão).
- **Nota de licenciamento/download**: Mesma nota de confirmação explícita antes do download.

## Decision: Extrair os componentes de UI duplicados para uma fábrica de widgets compartilhada

- **Decision**: Introduzir uma nova classe estática `GameRpg.UI.DemoUiKit` (ou similar) com os
  métodos `CreateText`, `CreateButton`, `CreatePanel` etc., e migrar `CombatDemoController`,
  `SkillTreeDemoController`, `SurvivalDemoController`, `ReputationEconomyDemoController` e
  `CharacterCreationUI` (todos da feature 001/002) para usá-la, em vez de cada um manter sua
  própria cópia local desses métodos.
- **Rationale**: Achado importante desta fase de pesquisa: embora a spec (FR-007) exija que o
  novo visual se propague automaticamente para "qualquer tela que use os componentes de UI
  compartilhados", esses métodos hoje **não são realmente compartilhados** — são copiados
  (duplicados) em cada arquivo de controller. Sem essa extração, aplicar os novos assets
  exigiria editar cada controller individualmente, o que contradiz FR-007 e o Princípio II
  (arquitetura modular). Esta extração é, portanto, um pré-requisito estrutural desta feature,
  não um "nice-to-have".
- **Alternatives considered**: Editar os assets visuais em cada controller individualmente —
  rejeitado por violar FR-007 diretamente. Um sistema de tema completo (ScriptableObject de
  "UI Theme" com múltiplos temas trocáveis em runtime) — avaliado e rejeitado por escopo: a
  spec não pede múltiplos temas, apenas substituir os retângulos de cor sólida atuais por um
  visual consistente (Princípio V).

## Decision: Mapeamento de características visuais para o sprite

- **Decision**: Usar `SpriteRenderer.color` (tingimento multiplicativo) sobre um sprite base do
  personagem para aproximar `VisualCharacteristics.SkinTone` (3 valores) e
  `VisualCharacteristics.HairColor` (cor livre, já existente desde a feature 002); usar sprites
  distintos do spritesheet do Kenney (quando houver frames suficientemente diferentes) para
  aproximar `BodyType`/`HairStyle`, com fallback para o tingimento quando não houver frame
  correspondente.
- **Rationale**: Atende FR-005 ("na medida do que o pacote de assets permitir") sem exigir um
  pipeline de seleção de sprite por combinação exata — abordagem de "melhor esforço" já
  documentada como Assumption na spec.
- **Alternatives considered**: Gerar/editar sprites customizados por combinação (18
  combinações) — rejeitado por escopo (exigiria trabalho de arte fora do que este projeto faz
  hoje, e contradiz a Assumption já registrada na spec).

## Decision: Movimentação do personagem na Exploração via `Input` legado (WASD/setas)

- **Decision**: Reaproveitar o mesmo padrão de leitura de teclado já usado em
  `Demo.DemoCameraController` (feature 001) — `UnityEngine.Input.GetKey` para WASD/setas —
  para mover o personagem na cena de Exploração, sem introduzir a nova Input System package
  (já instalada, mas não usada ativamente pelas demos) neste momento.
- **Rationale**: `ProjectSettings.activeInputHandler` já está configurado como "Both" (ver
  research.md da feature 001), então `Input` legado continua funcionando; reaproveitar o
  padrão já testado evita introduzir uma segunda forma de capturar input no mesmo projeto
  (Princípio V).
- **Alternatives considered**: Migrar para `UnityEngine.InputSystem` (pacote já referenciado no
  projeto) — avaliado e adiado: não há necessidade concreta agora que justifique o retrabalho
  em todas as demos existentes que já usam `Input` legado.

## Decision: Transição de Criação de Personagem para Exploração via carregamento de cena

- **Decision**: O botão "Finalizar" de `CharacterCreationUI` (feature 002), após chamar
  `CharacterCreationProfile.Finalize()`, usa `UnityEngine.SceneManagement.SceneManager
  .LoadScene("Exploration")` para carregar a cena de Exploração; o personagem finalizado é
  passado adiante por meio de um objeto estático/singleton simples
  (`GameRpg.Core.PendingPlayerCharacter` ou equivalente) lido pelo controlador da Exploração no
  `Start()` — não é necessário um sistema de gerenciamento de estado entre cenas mais robusto
  para este MVP.
- **Rationale**: É o mecanismo mais simples do Unity para "levar o jogador para outra cena"
  (Princípio V); como o projeto é single-player e local, um valor estático em memória é
  suficiente para carregar um único personagem entre duas cenas na mesma sessão de jogo — sem
  precisar de save/load do disco no meio do caminho (esse mecanismo já existe para
  persistência entre sessões, não é este o problema que esta feature resolve).
- **Alternatives considered**: Usar `DontDestroyOnLoad` em um GameObject portador do
  `Character` — funcionalmente equivalente, mas um valor estático simples é mais direto para
  um único objeto de transferência e evita GameObjects "fantasmas" sobrevivendo entre cenas de
  demo. Persistir em disco e recarregar — rejeitado por ser desnecessariamente pesado para uma
  transferência dentro da mesma sessão.

## Resumo das resoluções de Technical Context

| Campo | Resolução |
|---|---|
| Language/Version | C# (Unity 6000.5.9f1 — mesma versão das features 001/002) |
| Primary Dependencies | Unity UGUI (mesmas da feature 001/002); nenhum pacote novo do Package Manager |
| Storage | Arquivos locais em JSON (inalterado); mais os arquivos binários dos assets de terceiros (sprites/UI) importados como assets Unity |
| Testing | Unity Test Framework — EditMode (mapeamento de características visuais → tingimento/sprite; lógica de movimento) |
| Target Platform | PC desktop (mesmo alvo das features 001/002) |
| Project Type | Extensão do mesmo projeto Unity single-player |
| Performance Goals | Mesmos das features 001/002 (60 fps, <100ms de input) |
| Constraints | Assets de terceiros DEVEM ser CC0/licença aberta equivalente (FR-006/FR-009); nenhum download sem confirmação explícita do usuário |
| Scale/Scope | 1 sprite de personagem (com variação por tingimento/frame), 1 pacote de UI aplicado via componentes compartilhados, 1 nova transição de cena |
