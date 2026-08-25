# Quickstart: Validação da Feature RPG Sandbox

**Feature**: [spec.md](./spec.md) | **Plano**: [plan.md](./plan.md)

Guia para validar manualmente e via testes automatizados que a feature funciona ponta a ponta,
uma vez implementada. Não contém código de implementação — apenas passos de execução e
resultados esperados.

## Pré-requisitos

- Unity Editor (versão LTS definida em [research.md](./research.md#decision-engine-e-linguagem))
  instalado.
- Projeto Unity deste repositório aberto no Editor.
- Pacotes do projeto restaurados (URP, Cinemachine, Input System, Unity Test Framework —
  gerenciados via Unity Package Manager, conforme `research.md`).

## Rodando os testes automatizados (Princípio III — não-negociável)

1. Abrir `Window > General > Test Runner` no Unity Editor.
2. Rodar a suíte **EditMode**: deve cobrir fórmulas de combate, regras de árvore de habilidades
   (incluindo os invariantes do [contrato de SkillNode](./contracts/skill-node-data-contract.md)),
   simulação de fome/sanidade e o [contrato de simulação de economia de vila](./contracts/village-economy-simulation-contract.md).
   **Resultado esperado**: 100% dos testes passam.
3. Rodar a suíte **PlayMode**: deve cobrir um encontro de combate completo (início → fim) e o
   ciclo de save/load descrito no [contrato de save data](./contracts/save-data-contract.md).
   **Resultado esperado**: 100% dos testes passam.

## Validação manual — Combate (User Story 1 / P1)

1. Carregar a cena de exploração e acionar um encontro de combate contra 2–4 inimigos.
   **Esperado**: a câmera assume a visão isométrica de combate, a ordem de iniciativa é exibida.
2. No turno do jogador, mover o personagem, usar uma ação e uma ação bônus.
   **Esperado**: cada recurso de turno fica indisponível após o uso e é restaurado apenas no
   próximo turno do personagem.
3. Derrotar todos os inimigos.
   **Esperado**: o combate termina, recompensas são exibidas, o jogo retorna à exploração.

## Validação manual — Árvore de habilidades (User Story 2 / P2)

1. Com pontos de habilidade disponíveis, abrir a árvore de habilidades.
   **Esperado**: nós das trilhas Combatente e Arcanista são exibidos; nós híbridos aparecem
   bloqueados até que pré-requisitos de ambas as trilhas sejam satisfeitos.
2. Investir pontos majoritariamente em Combatente, depois iniciar um combate.
   **Esperado**: capacidades físicas aprendidas estão disponíveis nas ações de combate.
3. Tentar reinvestir em um nó já adquirido.
   **Esperado**: o sistema bloqueia e comunica que a habilidade já foi adquirida.

## Validação manual — Sobrevivência (User Story 3 / P3)

1. Deixar o personagem sem se alimentar por um período de jogo prolongado.
   **Esperado**: o indicador de fome cruza os limiares críticos e penalidades mensuráveis
   (ex.: redução de atributos físicos) são aplicadas.
2. Consumir alimento disponível.
   **Esperado**: o indicador de fome é restaurado e as penalidades são removidas.
3. Expor o personagem a um evento perturbador definido (ex.: combate extremo).
   **Esperado**: o indicador de sanidade cai; abaixo do limiar crítico, efeitos negativos
   definidos (ex.: penalidade em testes mentais/mágicos) são aplicados.

## Validação manual — Reputação e mundo reativo (User Story 4 / P4)

1. Escolher **não** salvar um NPC em perigo em uma comunidade específica.
   **Esperado**: a reputação do jogador com essa comunidade diminui; NPCs relacionados reagem
   negativamente ou missões associadas deixam de estar disponíveis.
2. Remover o suprimento essencial (ex.: alimento) de uma vila sem repor, e avançar o tempo
   in-game além do limiar de sustentação (ver [contrato de simulação de vila](./contracts/village-economy-simulation-contract.md)).
   **Esperado**: a população da vila diminui de forma visível e a economia local piora.
3. Transportar o recurso essencial de volta para a vila afetada.
   **Esperado**: a degradação populacional/econômica é interrompida a partir desse ponto (perdas
   já ocorridas não são revertidas) e a reputação do jogador com a vila aumenta.

## Critério de conclusão do quickstart

Todas as seções acima (testes automatizados + 4 blocos de validação manual) DEVEM passar antes
de considerar a feature `001-isometric-sandbox-rpg` pronta para revisão, conforme os critérios
de sucesso mensuráveis definidos em [spec.md](./spec.md#success-criteria-mandatory).
