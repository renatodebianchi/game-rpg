# Contrato: Finalização da Criação de Personagem

Contrato do que acontece quando o jogador finaliza a criação de personagem (FR-007, FR-010,
FR-011, FR-012). Ver [data-model.md](../data-model.md#charactercreationprofile).

## Pré-condições

1. `AttributeAllocationState.pointsRemaining == 0` (ver
   [point-buy-contract.md](./point-buy-contract.md), regra 4). Caso contrário, a finalização
   DEVE ser recusada e o jogador informado de quantos pontos faltam ajustar (FR-003).
2. `CharacterOrientation` DEVE ter sido escolhida. Não há valor padrão para orientação — ao
   contrário das características visuais, o kit de equipamento inicial não tem um "kit
   genérico" (a spec não define um); a UI DEVE impedir o avanço para a finalização sem essa
   escolha.
3. Características visuais não escolhidas explicitamente assumem o valor padrão definido em
   `VisualCharacteristics` (FR-007) — isso NUNCA bloqueia a finalização.

## Efeitos da finalização (ordem)

1. `Character.Attributes` recebe os valores de `AttributeAllocationState.scores`.
2. O `EquipmentKitDefinition` cuja `orientation` casa com a escolha do jogador é resolvido
   (falha explícita — `ContentValidationException`, conforme `Core.ContentValidation` da
   feature 001 — se nenhum kit correspondente existir, nunca um kit vazio silencioso) e cada um
   de seus itens é adicionado a `Character.Inventory` via `Inventory.Add(resourceId, quantity)`.
3. `Character.Visuals` recebe o valor final de `VisualCharacteristics` (com padrões aplicados
   onde o jogador não escolheu explicitamente).
4. Após este ponto, nenhuma API deste feature permite alterar `Character.Attributes` —
   distinto do respec de habilidades (feature 001, FR-018), que continua disponível
   normalmente e não é afetado por esta regra (FR-012).

## Pós-condição de persistência

- O próximo `SaveSystem.CaptureGameState(...)` (feature 001, estendido por esta feature — ver
  [data-model.md](../data-model.md#extensão-de-coresavedata-feature-001-contractssave-data-contractmd))
  DEVE incluir os atributos finais, o inventário (já contendo os itens do kit) e
  `Character.Visuals` — sem exigir nenhum código adicional específico de "criação de
  personagem" no momento do save, já que tudo já está representado nos campos existentes de
  `Character` mais o novo campo `Visuals`.

## Consumidores deste contrato

- `CharacterCreationUI` (botão "Finalizar"/"Confirmar").
- Testes de integração (PlayMode) que validam o fluxo completo: alocar atributos → escolher
  orientação → escolher aparência → finalizar → verificar o `Character` resultante.
