# Quickstart: Validação da Feature Criação de Personagem

**Feature**: [spec.md](./spec.md) | **Plano**: [plan.md](./plan.md)

Guia para validar manualmente e via testes automatizados que a feature funciona ponta a ponta,
uma vez implementada. Segue o mesmo padrão do
[quickstart.md da feature 001](../001-isometric-sandbox-rpg/quickstart.md).

## Pré-requisitos

- Mesmos pré-requisitos da feature 001 (Unity Editor instalado, projeto aberto, pacotes
  restaurados).
- `/speckit-tasks` e `/speckit-implement` já executados para esta feature.

## Rodando os testes automatizados (Princípio III — não-negociável)

1. `Window > General > Test Runner` no Unity Editor.
2. Rodar **EditMode**: deve cobrir a tabela de custo do Point Buy, a validação de alocação (ver
   [contracts/point-buy-contract.md](./contracts/point-buy-contract.md)), a resolução do kit de
   equipamento por orientação, e as regras de finalização (ver
   [contracts/character-creation-finalization-contract.md](./contracts/character-creation-finalization-contract.md)).
   **Resultado esperado**: 100% dos testes passam.
3. Rodar **PlayMode**: deve cobrir o fluxo completo de criação (alocar → orientação → aparência
   → finalizar) e a persistência do personagem resultante via save/load.
   **Resultado esperado**: 100% dos testes passam.

## Validação manual — Atributos por Point Buy (User Story 1 / P1)

1. Abrir a tela de criação de personagem.
   **Esperado**: os 4 atributos começam em 8, com 18 pontos disponíveis.
2. Aumentar um atributo até 15.
   **Esperado**: o custo cumulativo correto é descontado a cada passo; não é possível passar de
   15.
3. Tentar finalizar com pontos ainda não gastos.
   **Esperado**: a finalização é bloqueada, com indicação de quantos pontos faltam alocar.
4. Gastar exatamente os 18 pontos.
   **Esperado**: a criação pode avançar para a próxima etapa.

## Validação manual — Orientação e equipamento inicial (User Story 2 / P2)

1. Escolher a orientação "combatente".
   **Esperado**: o personagem recebe o kit de equipamento fixo de combatente no inventário.
2. Repetir escolhendo "arcanista" em uma nova criação.
   **Esperado**: o personagem recebe o kit fixo de arcanista, diferente do de combatente.
3. Finalizar a criação e, em seguida, abrir a árvore de habilidades (demo da feature 001,
   `SkillTreeDemo.unity`).
   **Esperado**: é possível investir livremente em nós de qualquer trilha (Combatente,
   Arcanista, Híbrida), independentemente da orientação escolhida na criação.

## Validação manual — Aparência básica (User Story 3 / P3)

1. Selecionar um tipo de corpo, tom de pele e estilo/cor de cabelo.
   **Esperado**: o resumo de criação reflete as escolhas feitas.
2. Deixar uma característica visual sem selecionar e finalizar a criação.
   **Esperado**: a finalização não é bloqueada; um valor padrão é aplicado automaticamente
   àquela característica.

## Critério de conclusão do quickstart

Todas as seções acima (testes automatizados + os 3 blocos de validação manual) DEVEM passar
antes de considerar a feature `002-character-creation` pronta para revisão, conforme os
critérios de sucesso mensuráveis definidos em
[spec.md](./spec.md#success-criteria-mandatory).
