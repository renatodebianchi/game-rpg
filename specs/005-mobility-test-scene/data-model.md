# Phase 1 Data Model: Cena de Teste de Mobilidade

**Feature**: [spec.md](./spec.md) | **Research**: [research.md](./research.md)

Modelo conceitual das entidades introduzidas por esta feature. Não reutiliza nenhuma entidade
das features anteriores (Character/Combat) — é um sistema isolado (FR-014), exceto pela câmera
(`Demo.BoundedFollowCamera`, feature 004).

## MovementStateKind (enum)

Os estados de movimentação exigidos por FR-011: `Idle`, `Walking`, `Running`, `Jumping`,
`DoubleJumping`, `Falling`, `Crouching`, `WallSliding`. Cada um mapeia para uma pose de sprite
(research.md, "melhor esforço" quando não há pose 1-para-1 no pacote de assets).

## PlatformerMovementState (classe C# pura)

A máquina de decisão de movimentação — não sabe nada sobre `Rigidbody2D`, `Transform`, ou
sprites; apenas recebe informações de física (grounded/tocando parede) e comandos de input, e
produz decisões (pode pular? qual velocidade vertical resultante?).

| Campo/Membro | Descrição | Regras/Validação |
|---|---|---|
| `IsGrounded` | Definido a cada frame pelo controller, a partir do raycast de chão | — |
| `WallContactDirection` | -1 (parede à esquerda), 0 (nenhuma), 1 (parede à direita), definido pelo controller | Só relevante quando `!IsGrounded` |
| `IsCrouching` | Definido pelo controller a partir do input de abaixar (FR-008) | Guarda de `AdvanceCharge` — ver abaixo |
| `JumpsUsed` | Quantos pulos aéreos já foram gastos desde o último contato com o chão | Resetado para 0 por `NotifyGrounded()` |
| `MaxAerialJumps` | Pulos aéreos disponíveis além do pulo do chão (o pulo do chão não conta neste contador) | Propriedade com setter (não só valor de construtor), default 1 (pulo duplo padrão) — permite habilidades futuras aumentarem o limite em tempo de execução (ex. `State.MaxAerialJumps += 1`) sem alterar `PlatformerMovementState` |
| `CurrentChargeSeconds` / `MaxChargeSeconds` | Energia acumulada do salto carregado (FR-009) | `CurrentChargeSeconds` sempre `0..MaxChargeSeconds` |
| `MinChargeSecondsToLeap` | Limiar mínimo para o salto ocorrer ao soltar (Edge case: soltar sem carregar não salta) | `< MaxChargeSeconds` |
| `WallSlideFallSpeedMultiplier` | Fração da velocidade de queda livre aplicada durante o deslizar na parede (FR-006) | Configurável, `0 < valor < 1`, default 0.5 |

**Propriedade derivada**: `IsWallSliding => !IsGrounded && WallContactDirection != 0` — a mesma
condição já usada por `TryWallJump`, exposta para o controller ler em vez de recalculá-la
(evita duplicar a regra em dois lugares).

**Métodos-chave** (ver [contracts/movement-state-contract.md](./contracts/movement-state-contract.md)
e [contracts/charge-jump-contract.md](./contracts/charge-jump-contract.md)):

- `NotifyGrounded()` — chamado quando o controller detecta contato com o chão; reseta `JumpsUsed`.
- `TryGroundJump()` / `TryAerialJump()` — pulo do chão vs. pulo duplo no ar (FR-004, FR-005).
- `TryWallJump()` — só válido com `WallContactDirection != 0` e `!IsGrounded`; reseta
  `JumpsUsed` para 0 ao suceder (permite encadear pulos de parede indefinidamente, FR-007).
- `GetFallSpeedMultiplier()` — retorna `WallSlideFallSpeedMultiplier` quando `IsWallSliding`,
  senão `1f`; é a própria classe pura que decide o valor (não só a condição), para que a regra
  de FR-006 seja testável sem depender do `MonoBehaviour` (Princípio III).
- `AdvanceCharge(deltaSeconds)` — a própria classe verifica `IsGrounded && IsCrouching`
  internamente e não tem efeito algum se qualquer uma for falsa (FR-008/FR-009) — o controller
  não precisa (e não deve) replicar essa checagem antes de chamar.
- `ReleaseCharge()` — retorna a fração de energia acumulada (0 se abaixo do limiar mínimo) e
  zera `CurrentChargeSeconds` (FR-010, Edge Cases).

## PlatformerMovementController (comportamento, não dado)

O `MonoBehaviour` que: lê o input do jogador, faz os raycasts de chão/parede via
`Physics2D.Raycast` a partir do `BoxCollider2D`, alimenta `PlatformerMovementState` com esses
resultados (incluindo `IsCrouching`, a partir do input de abaixar), aplica a velocidade
resultante ao `Rigidbody2D` (multiplicando a queda por `GetFallSpeedMultiplier()` quando
relevante), e determina o `MovementStateKind` atual para o `SpriteFlipbookAnimator` (research.md,
"Rigidbody2D + raycasts" e "classe C# pura"). Ele **alimenta dados e aplica decisões já
tomadas** pela classe pura — não decide, por conta própria, se uma ação de gameplay é válida ou
qual seu efeito numérico (Princípio III: a apresentação nunca contém lógica que afeta o jogo).

## SpriteFlipbookAnimator (comportamento, não dado)

Troca `SpriteRenderer.sprite` entre os quadros configurados para o `MovementStateKind` atual, a
um intervalo fixo por estado — substitui a necessidade de um `Animator`/`AnimatorController`
autorado (research.md).

## MobilityTestScene (cenário, não classe)

A cena `MobilityTest.unity`: chão plano, ao menos uma plataforma elevada, e ao menos uma parede
vertical alta o suficiente para testar deslizar/pular na parede (FR-001), construída a partir do
tileset do pacote Anokolisa "Legacy Fantasy – High Forest" via `ProjectBootstrap`, com um fundo
texturizado (FR-012).

## Diagrama de relações (conceitual)

```
PlatformerMovementController --alimenta (grounded/wall contact)--> PlatformerMovementState
PlatformerMovementController --lê decisões de--> PlatformerMovementState
PlatformerMovementController --aplica velocidade a--> Rigidbody2D
PlatformerMovementController --informa estado atual a--> SpriteFlipbookAnimator
Demo.BoundedFollowCamera (feature 004) --segue--> PlatformerMovementController (personagem)
MobilityTestScene --contém--> PlatformerMovementController + tileset/fundo Anokolisa
```
