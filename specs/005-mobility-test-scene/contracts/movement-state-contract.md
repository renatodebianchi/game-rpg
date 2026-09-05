# Contrato: Decisões de Pulo e Parede (`PlatformerMovementState`)

Contrato de como `PlatformerMovementState` decide pulos e mobilidade de parede (FR-004, FR-005,
FR-006, FR-007). Ver [data-model.md](../data-model.md).

## Pulo do chão e pulo duplo (FR-004, FR-005)

1. `TryGroundJump()` só tem efeito quando `IsGrounded == true`; produz uma velocidade vertical
   de pulo e NÃO incrementa `JumpsUsed` (o pulo do chão não conta para o limite de pulo aéreo).
2. `TryAerialJump()` só tem efeito quando `IsGrounded == false` e `JumpsUsed < MaxAerialJumps`;
   produz uma velocidade vertical de pulo (pulo aéreo) e incrementa `JumpsUsed` em 1.
   `MaxAerialJumps` é 1 por padrão (o pulo duplo tradicional: um pulo aéreo, além do pulo do chão).
3. Uma tentativa de `TryAerialJump()` além do limite configurado, sem tocar o chão entre elas,
   NÃO tem efeito (`JumpsUsed >= MaxAerialJumps`).
4. `NotifyGrounded()` (chamado pelo controller ao detectar contato com o chão) sempre reseta
   `JumpsUsed` para 0.
5. `MaxAerialJumps` é uma propriedade com setter, não só um valor fixo de construtor — uma
   habilidade especial futura pode aumentá-la em tempo de execução (ex.
   `state.MaxAerialJumps += 1`) sem exigir mudança nesta classe.

## Deslizar e pular na parede (FR-006, FR-007)

1. Deslizar na parede é uma condição derivada, exposta como a propriedade somente-leitura
   `IsWallSliding => !IsGrounded && WallContactDirection != 0` — não recalculada de forma
   independente pelo controller (evita duplicar a regra em dois lugares).
2. `GetFallSpeedMultiplier()` retorna `WallSlideFallSpeedMultiplier` (< 1) quando `IsWallSliding`
   é verdadeiro, e `1f` caso contrário. É a própria `PlatformerMovementState` — não o
   `MonoBehaviour` — quem decide o valor da redução de velocidade de queda (FR-006), para que a
   regra seja testável em EditMode sem depender de uma cena carregada (Princípio III: o
   `MonoBehaviour` apenas multiplica a velocidade vertical pelo valor retornado, nunca decide um
   valor por conta própria).
3. `TryWallJump()` só tem efeito quando `!IsGrounded && WallContactDirection != 0`; produz uma
   velocidade que afasta o personagem da parede (direção oposta a `WallContactDirection`) e para
   cima.
4. Ao suceder, `TryWallJump()` reseta `JumpsUsed` para 0 — permite encadear pulos de parede em
   paredes diferentes indefinidamente, sem ser limitado pelo contador de pulo duplo (mesmo
   espírito da mobilidade "walljump" de Super Metroid referenciada na spec).
5. Tocar o chão enquanto desliza na parede sai da condição de "deslizando" imediatamente (via
   `NotifyGrounded()`), sem exigir nenhuma chamada adicional.

## Pós-condições

- `JumpsUsed` nunca é negativo nem excede `MaxAerialJumps` antes de um pulo aéreo ser recusado.
- Nenhum destes métodos (incluindo `GetFallSpeedMultiplier()`) aplica velocidade diretamente a
  um `Transform`/`Rigidbody2D` — apenas retornam a decisão; a aplicação física é
  responsabilidade exclusiva do `PlatformerMovementController` (mantém a classe testável sem uma
  cena carregada).

## Consumidores deste contrato

- `PlatformerMovementController` (aplica as decisões ao `Rigidbody2D`).
- Testes automatizados (`PlatformerMovementStateTests`).
