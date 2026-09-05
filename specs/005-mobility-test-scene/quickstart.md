# Quickstart: Validação da Feature Cena de Teste de Mobilidade

**Feature**: [spec.md](./spec.md) | **Plano**: [plan.md](./plan.md)

Guia para validar manualmente e via testes automatizados que a feature funciona ponta a ponta,
uma vez implementada.

## Pré-requisitos

- Mesmos pré-requisitos das features 001-004 (Unity Editor instalado, projeto aberto).
- O arquivo `.zip` da Anokolisa ("Legacy Fantasy – High Forest", itch.io) já baixado e importado em `Assets/Art/Platformer/`
  (com a confirmação explícita já dada pelo usuário durante a implementação).
- `/speckit-tasks` e `/speckit-implement` já executados para esta feature.

## Rodando os testes automatizados (Princípio III — não-negociável)

1. `Window > General > Test Runner` no Unity Editor.
2. Rodar **EditMode**: deve cobrir `PlatformerMovementState` — limite de pulo duplo (não há um
   terceiro pulo no ar), pulo/deslizar na parede (incluindo o reset do contador de pulo duplo
   ao pular da parede), e o acúmulo/liberação de energia do salto carregado (incluindo o caso de
   soltar sem ter carregado o suficiente). **Resultado esperado**: 100% dos testes passam.
3. Rodar a suíte completa das features 001-004 (atributos, habilidades, sobrevivência,
   reputação/economia, criação de personagem, combate em tempo real) — **Resultado esperado**:
   nenhuma regressão: 100% continuam passando sem alteração de comportamento (FR-014 — esta
   feature não deveria ter tocado nenhum desses sistemas).

## Validação manual — Movimentação básica (User Story 1 / P1)

1. Abrir a cena `MobilityTest.unity`.
   **Esperado**: chão plano visível, fundo com textura (não uma cor sólida), e ao menos uma
   plataforma elevada visível.
2. Mover o personagem para os lados.
   **Esperado**: o personagem anda, com animação de andar visível.
3. Segurar o comando de corrida enquanto anda.
   **Esperado**: o personagem se move visivelmente mais rápido, com animação de corrida
   diferente da de andar.
4. Pular do chão.
   **Esperado**: o personagem salta, com animação de pulo, e retorna ao chão pela gravidade sem
   atravessar o chão.

## Validação manual — Mobilidade aérea avançada (User Story 2 / P2)

1. Pular do chão e, no ar, acionar o pulo novamente.
   **Esperado**: um segundo pulo ocorre (pulo duplo), com animação distinta.
2. No ar após o pulo duplo, acionar o pulo uma terceira vez.
   **Esperado**: nada acontece — não há um terceiro pulo.
3. Encostar em uma das paredes verticais do cenário enquanto cai.
   **Esperado**: o personagem desliza pela parede a uma velocidade de queda reduzida, com
   animação de deslizar na parede.
4. Acionar o pulo enquanto desliza na parede.
   **Esperado**: o personagem é impulsionado para longe da parede e para cima; repetir contra
   outra parede próxima funciona da mesma forma.

## Validação manual — Salto de energia acumulada (User Story 3 / P3)

1. No chão, abaixo de uma plataforma elevada, acionar o comando de abaixar.
   **Esperado**: o personagem se abaixa, com animação de agachado.
2. Manter o comando pressionado por durações diferentes (curta vs. longa) em tentativas
   separadas.
   **Esperado**: energia acumulada visível aumentando; ao soltar, o personagem salta mais alto
   quanto mais tempo foi carregado, até o limite máximo ou colidir com a plataforma acima.
3. Soltar o comando quase imediatamente após abaixar (sem carregar).
   **Esperado**: o personagem apenas se levanta, sem saltar.

## Critério de conclusão do quickstart

Todas as seções acima (testes automatizados + os 3 blocos de validação manual) DEVEM passar
antes de considerar a feature `005-mobility-test-scene` pronta para revisão, conforme os
critérios de sucesso mensuráveis definidos em [spec.md](./spec.md#success-criteria-mandatory).
