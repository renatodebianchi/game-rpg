# Quickstart: Validação da Feature Assets Visuais + Exploração com Personagem Criado

**Feature**: [spec.md](./spec.md) | **Plano**: [plan.md](./plan.md)

Guia para validar manualmente e via testes automatizados que a feature funciona ponta a ponta,
uma vez implementada.

## Pré-requisitos

- Mesmos pré-requisitos das features 001/002 (Unity Editor instalado, projeto aberto).
- Os arquivos `.zip` do Kenney "Roguelike Characters" e "UI Pack" já baixados e importados em
  `Assets/Art/` (com a confirmação explícita já dada pelo usuário durante a implementação).
- `/speckit-tasks` e `/speckit-implement` já executados para esta feature.

## Rodando os testes automatizados (Princípio III — não-negociável)

1. `Window > General > Test Runner` no Unity Editor.
2. Rodar **EditMode**: deve cobrir o mapeamento de `VisualCharacteristics` para
   sprite/tingimento (ver
   [contracts/character-sprite-mapping-contract.md](./contracts/character-sprite-mapping-contract.md)).
   **Resultado esperado**: 100% dos testes passam.
3. Rodar **PlayMode**: deve cobrir a transição de cena (ver
   [contracts/scene-transition-contract.md](./contracts/scene-transition-contract.md)): finalizar
   criação → carregar Exploração → personagem correto exibido.
   **Resultado esperado**: 100% dos testes passam.

## Validação manual — Ver e mover o personagem na Exploração (User Story 1 / P1)

1. Abrir `CharacterCreationDemo.unity`, completar a criação de um personagem e finalizar.
   **Esperado**: o jogo carrega a cena de Exploração automaticamente.
2. Observar o personagem na Exploração.
   **Esperado**: é exibido como um sprite humanoide (não um primitivo/cápsula).
3. Mover o personagem com o teclado (WASD/setas).
   **Esperado**: o personagem se move suavemente pela cena nas quatro direções.
4. Abrir `Exploration.unity` diretamente (sem passar pela criação).
   **Esperado**: um personagem com aparência padrão é exibido e pode ser movido normalmente.

## Validação manual — Aparência reflete as características escolhidas (User Story 2 / P2)

1. Criar dois personagens com tons de pele diferentes (feature 002, etapa de aparência).
   **Esperado**: os sprites exibidos na Exploração para cada um são visivelmente diferentes
   nesse aspecto.
2. Salvar o jogo, recarregar, e comparar a aparência do personagem antes/depois.
   **Esperado**: a aparência exibida é idêntica antes e depois do save/load.

## Validação manual — Interface visual estilizada (User Story 3 / P3)

1. Abrir cada uma das telas construídas em runtime (Criação de Personagem, Exploração, Combate,
   Árvore de Habilidades, Sobrevivência, Reputação/Economia).
   **Esperado**: todos os botões e painéis usam os assets do Kenney UI Pack — nenhum retângulo
   de cor sólida lisa remanescente.
2. Verificar `Assets/Art/CREDITS.md` (ou arquivo de créditos equivalente).
   **Esperado**: lista os pacotes usados (Roguelike Characters, UI Pack), fonte (kenney.nl) e
   licença (CC0) de cada um.

## Critério de conclusão do quickstart

Todas as seções acima (testes automatizados + os 3 blocos de validação manual) DEVEM passar
antes de considerar a feature `003-character-visual-exploration` pronta para revisão, conforme
os critérios de sucesso mensuráveis definidos em
[spec.md](./spec.md#success-criteria-mandatory).
