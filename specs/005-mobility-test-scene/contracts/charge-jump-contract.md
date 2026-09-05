# Contrato: Salto de Energia Acumulada (`PlatformerMovementState`)

Contrato de como a energia acumulada ao agachar vira um salto vertical (FR-008, FR-009, FR-010).
Ver [data-model.md](../data-model.md).

## Acúmulo (FR-008, FR-009)

1. `AdvanceCharge(deltaSeconds)` verifica `IsGrounded && IsCrouching` **internamente** e não
   tem efeito algum (não altera `CurrentChargeSeconds`) se qualquer uma das duas for falsa (Edge
   Cases: abaixar não tem efeito no ar) — o controller pode chamá-lo a cada frame
   incondicionalmente; a guarda é responsabilidade da própria `PlatformerMovementState`, não do
   `MonoBehaviour` (Princípio III: a regra de quando a energia acumula é lógica de jogo, e
   precisa estar na classe testável, não na apresentação).
2. Quando a guarda passa, `CurrentChargeSeconds` aumenta por `deltaSeconds` a cada chamada,
   nunca excedendo `MaxChargeSeconds` (limite máximo, FR-009).

## Liberação (FR-010, Edge Cases)

1. `ReleaseCharge()` é chamado quando o jogador solta o comando de abaixar.
2. Se `CurrentChargeSeconds < MinChargeSecondsToLeap`: retorna `0` (nenhum salto ocorre — o
   personagem apenas se levanta, Edge Case da spec).
3. Caso contrário: retorna `CurrentChargeSeconds / MaxChargeSeconds`, uma fração entre o limiar
   mínimo (exclusive) e `1.0` — o controller multiplica essa fração pela força de salto máxima
   configurada para obter a velocidade vertical real.
4. Em ambos os casos, `CurrentChargeSeconds` é zerado após a chamada (a energia não persiste
   entre agachamentos).

## Colisão com o teto (FR-010, Edge Cases)

- O salto resultante é aplicado como uma velocidade vertical ao `Rigidbody2D` pelo controller;
  a física do Unity resolve a colisão contra um teto/obstáculo normalmente — este contrato não
  precisa (nem deve) modelar isso: `PlatformerMovementState` só decide a velocidade inicial do
  salto, nunca sabe sobre colisões durante o trajeto.

## Consumidores deste contrato

- `PlatformerMovementController` (chama `AdvanceCharge`/`ReleaseCharge` e aplica a velocidade
  resultante ao `Rigidbody2D`).
- Testes automatizados (`PlatformerMovementStateTests`).
