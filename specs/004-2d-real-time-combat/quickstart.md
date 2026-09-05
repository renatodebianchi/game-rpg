# Quickstart: Validação da Feature Combate em Tempo Real 2D

**Feature**: [spec.md](./spec.md) | **Plano**: [plan.md](./plan.md)

Guia para validar manualmente e via testes automatizados que a feature funciona ponta a ponta,
uma vez implementada.

## Pré-requisitos

- Mesmos pré-requisitos das features 001/002/003 (Unity Editor instalado, projeto aberto).
- `/speckit-tasks` e `/speckit-implement` já executados para esta feature.
- O renderer URP do projeto já trocado para 2D (`Renderer2DData`) por `ProjectBootstrap`.

## Rodando os testes automatizados (Princípio III — não-negociável)

1. `Window > General > Test Runner` no Unity Editor.
2. Rodar **EditMode**: deve cobrir os limites de `BattleArena`
   ([contracts](./contracts/)), a execução/interrupção de `RealTimeActionExecutor`
   ([realtime-action-contract.md](./contracts/realtime-action-contract.md), incluindo FR-009),
   a decisão de `EnemyCombatAI`, e o canal de
   `RealTimeFleeAction` ([flee-channel-contract.md](./contracts/flee-channel-contract.md)).
   **Resultado esperado**: 100% dos testes passam.
3. Rodar **PlayMode**: deve cobrir o fluxo completo de um `CombatArenaEncounter` (vitória,
   derrota, fuga). **Resultado esperado**: 100% dos testes passam.
4. Rodar a suíte completa de EditMode/PlayMode das features 001/002/003 (atributos, habilidades,
   sobrevivência, reputação/economia, criação de personagem) — **Resultado esperado**: nenhuma
   regressão (SC-004): 100% continuam passando sem alteração de comportamento.

## Validação manual — Combate em tempo real (User Story 1 / P1)

1. Abrir a demo de arena de combate (`BattleArenaDemoController`) e iniciar um encontro.
   **Esperado**: nenhum prompt de "sua vez"/seleção de ação bloqueia a tela; o personagem já
   pode se mover imediatamente.
2. Segurar o comando de movimento horizontal.
   **Esperado**: o personagem se move continuamente, parando nos limites da arena.
3. Aproximar-se de um inimigo e acionar o ataque básico.
   **Esperado**: o dano é aplicado imediatamente (barra de vida do inimigo reage), sem esperar
   por nenhum turno.
4. Acionar uma habilidade com tempo de conjuração visível.
   **Esperado**: há um intervalo perceptível antes do efeito se concretizar; se o personagem for
   atingido durante esse intervalo, a habilidade é cancelada sem efeito (FR-009) e o recurso
   gasto não retorna.
5. Observar o(s) inimigo(s) sem realizar nenhuma ação.
   **Esperado**: eles se movem e atacam por conta própria, continuamente (FR-007).
6. Deixar o personagem ser derrotado.
   **Esperado**: o combate termina em derrota imediatamente (mesma consequência da feature 001).
7. Segurar o comando de fuga em direção a uma borda da arena por alguns segundos.
   **Esperado**: após o tempo mínimo de canal, uma tentativa de fuga é resolvida (sucesso ou
   falha, nunca antes do tempo mínimo).

## Validação manual — Exploração 2D side-view (User Story 2 / P2)

1. Abrir a demo de Exploração.
   **Esperado**: a cena é 2D side-view (sem câmera isométrica 3D); o personagem se move
   livremente como antes.
2. Mover o personagem para perto de uma das extremidades do mapa.
   **Esperado**: a câmera para de segui-lo naquele eixo e mantém a borda do mapa visível, nunca
   revelando espaço vazio além do limite (FR-015, SC-006). O mesmo vale dentro da arena de
   combate.
3. Encontrar um inimigo e iniciar um combate.
   **Esperado**: a transição leva à arena 2D side-view (User Story 1), preservando atributos,
   inventário e aparência do personagem, sem mudança perceptível de perspectiva além da troca de
   cenário.

## Critério de conclusão do quickstart

Todas as seções acima (testes automatizados + os 2 blocos de validação manual) DEVEM passar
antes de considerar a feature `004-2d-real-time-combat` pronta para revisão, conforme os
critérios de sucesso mensuráveis definidos em [spec.md](./spec.md#success-criteria-mandatory).
