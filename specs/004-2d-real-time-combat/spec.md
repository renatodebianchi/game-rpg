# Feature Specification: Combate em Tempo Real 2D (estilo Tales of Phantasia)

**Feature Branch**: `[004-2d-real-time-combat]`

**Created**: 2026-09-05

**Status**: Draft

**Input**: User description: "Mude a abordagem visual do jogo de 3D para 2D. O sistema de combate não deve mais ser baseado em turnos; deve seguir um modelo de combate em tempo real semelhante ao do jogo "Tales of Phantasia" (Linear Motion Battle System): batalhas em uma arena 2D de perspectiva lateral (side-view), onde o jogador controla diretamente um personagem em tempo real (movimento livre no eixo horizontal, ataques corpo a corpo/à distância e habilidades/magias com tempo de conjuração), enquanto os demais membros do grupo são controlados por IA seguindo táticas configuráveis. Deve substituir o sistema de grid/turnos existente (feature 001) mantendo os sistemas de atributos, habilidades, sobrevivência e reputação/economia já implementados."

## Clarifications

### Session 2026-09-05

- Q: Quantos personagens compõem o grupo do jogador nesta feature? → A: Só o personagem criado
  (sem grupo) — o jogador luta sozinho; aliados controlados por IA com táticas configuráveis
  ficam fora do escopo desta feature, como uma extensão futura.
- Q: Qual perspectiva a exploração fora de combate deve usar após a conversão para 2D? → A:
  Side-view — a mesma perspectiva lateral do combate, para uma única perspectiva consistente em
  todo o jogo.
- Q: Como a câmera deve se posicionar em relação ao personagem? → A: O personagem sempre fica
  centralizado na câmera, exceto perto das extremidades do mapa/arena — nesse caso a câmera para
  de seguir e mantém a borda do cenário visível, em vez de mostrar espaço vazio além do limite.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Combate em tempo real controlando o personagem diretamente (Priority: P1)

Como jogador, ao entrar em combate eu assumo o controle direto do meu personagem em uma arena 2D
vista de lado (side-view): posso me mover livremente no eixo horizontal, desferir um ataque
corpo a corpo quando estou perto do alvo, usar um ataque à distância, ou conjurar uma
habilidade/magia que leva um tempo até se concretizar. O combate não pausa para eu escolher uma
ação em uma lista de turno — tudo acontece continuamente, e os inimigos podem agir e me atingir
enquanto decido o que fazer.

**Why this priority**: É a mudança central pedida — sem isso, não há "combate em tempo real
estilo Tales of Phantasia"; é o que torna esta feature diferente da feature 001. Entrega valor
completo por si só: um personagem lutando sozinho contra um ou mais inimigos em tempo real já é
um MVP jogável (ver Assumptions sobre o grupo do jogador).

**Independent Test**: Iniciar um encontro de combate, mover o personagem livremente pela arena,
conectar um ataque corpo a corpo em um inimigo adjacente, e conjurar uma habilidade com tempo de
conjuração observável — tudo sem qualquer prompt de "sua vez"/"fim de turno".

**Acceptance Scenarios**:

1. **Given** um combate em andamento, **When** o jogador segura o comando de movimento, **Then**
   o personagem se move continuamente no eixo horizontal da arena, respeitando os limites dela.
2. **Given** um inimigo ao alcance corpo a corpo, **When** o jogador aciona o ataque básico,
   **Then** o ataque é resolvido e o dano é aplicado sem esperar por um "turno".
3. **Given** o jogador aciona uma habilidade com tempo de conjuração, **When** a conjuração está
   em andamento, **Then** o personagem fica vulnerável durante esse tempo e a habilidade só
   produz efeito ao final dele (podendo ser interrompida se o personagem for atingido, conforme
   FR-009).
4. **Given** o jogador é atingido por um ataque inimigo, **Then** o dano é aplicado
   imediatamente, refletido na barra de vida, sem depender de uma sequência de turnos.
5. **Given** um ou mais inimigos na arena, **When** o combate está em andamento, **Then** cada
   inimigo age (mover, atacar) de forma autônoma e contínua, independente das ações do jogador.
6. **Given** o personagem do jogador é derrotado, **Then** o combate termina em derrota (mesma
   consequência já definida na feature 001), já que não há outro membro de grupo para assumir o
   controle.
7. **Given** o personagem se move pela arena, **When** ele está longe das extremidades da arena,
   **Then** a câmera o mantém centralizado; **When** ele se aproxima de uma extremidade, **Then**
   a câmera para de segui-lo e mantém a borda da arena visível, sem revelar espaço vazio além do
   limite do cenário (FR-015).

---

### User Story 2 - Mundo de exploração convertido para 2D side-view (Priority: P2)

Como jogador, ao explorar o mundo fora de combate (feature 003), eu vejo e me movo por um cenário
2D de perspectiva lateral (a mesma do combate) em vez da perspectiva isométrica 3D atual,
mantendo a mesma liberdade de movimento e o personagem visual já criado, para que a apresentação
do jogo seja consistente em 2D dentro e fora do combate.

**Why this priority**: É a parte "visual" do pedido, mas não bloqueia o valor central (o novo
combate); pode ser entregue depois da User Story 1 sem invalidá-la.

**Independent Test**: Abrir a demo de Exploração e confirmar que a câmera/cenário são 2D
side-view (sem câmera isométrica 3D), o personagem se move livremente como antes, e a transição
para um combate leva à mesma arena 2D side-view da User Story 1.

**Acceptance Scenarios**:

1. **Given** a demo de Exploração aberta, **Then** a cena é renderizada em 2D side-view (sem
   câmera isométrica 3D), com o personagem e o cenário nessa mesma perspectiva lateral.
2. **Given** o jogador encontra um inimigo na Exploração, **When** o combate começa, **Then** a
   transição leva à arena de combate 2D side-view (User Story 1), preservando os dados do
   personagem (atributos, inventário, aparência) sem nenhuma mudança de perspectiva perceptível
   além de trocar de cenário.
3. **Given** o personagem se move pelo cenário de Exploração, **When** ele está longe das
   extremidades do mapa, **Then** a câmera o mantém centralizado; **When** ele se aproxima de
   uma extremidade do mapa, **Then** a câmera para de segui-lo e mantém a borda do mapa visível
   (mesmo comportamento de câmera do combate, FR-015).

---

### Edge Cases

- O que acontece se o jogador tentar fugir? Segurar o comando de fuga em direção à borda da
  arena por um tempo contínuo tenta a fuga (chance influenciada pelos atributos, reaproveitando
  a fórmula de fuga da feature 001), em vez de uma ação instantânea de menu.
- O que acontece se o personagem for atingido durante a conjuração de uma habilidade? A
  conjuração é interrompida, o custo do recurso já gasto (FR-008) não é devolvido, e nenhum
  efeito da habilidade ocorre (FR-009).
- O que acontece com as penalidades de fome/sanidade (feature 001, FR-021) durante o combate em
  tempo real? Continuam se aplicando da mesma forma (redução cumulativa de dano/eficácia),
  apenas verificadas continuamente em vez de no início de cada turno.
- O que acontece se houver mais de um inimigo na arena? Cada inimigo age de forma independente e
  simultânea; não há ordem de iniciativa/turno entre eles.
- O que acontece quando o personagem do jogador é derrotado? O combate termina em derrota
  imediatamente (não há outro membro de grupo para assumir o controle nesta feature — ver
  Assumptions).
- O que acontece quando o personagem chega a uma extremidade do mapa/arena? A câmera para de
  segui-lo naquele eixo e mantém a borda do cenário na tela, em vez de continuar centralizando o
  personagem e revelar espaço vazio além do limite do mundo (FR-015).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE substituir o combate baseado em grid/turnos (feature 001) por um
  combate em tempo real que ocorre em uma arena 2D de perspectiva lateral (side-view).
- **FR-002**: O jogador DEVE controlar diretamente seu personagem, com movimento livre (não
  restrito a um grid) ao longo do eixo horizontal da arena, dentro dos limites dela.
- **FR-003**: O jogador DEVE poder desferir um ataque corpo a corpo quando o alvo está dentro do
  alcance corpo a corpo do personagem.
- **FR-004**: O jogador DEVE poder desferir um ataque à distância quando o personagem possuir
  essa capacidade (reaproveitando as capacidades já concedidas pela árvore de habilidades da
  feature 001).
- **FR-005**: O jogador DEVE poder acionar uma habilidade/magia que possui um tempo de conjuração
  configurável maior que zero antes de produzir efeito.
- **FR-006**: O sistema DEVE aplicar dano/efeitos assim que uma ação (ataque ou habilidade) se
  resolve, sem esperar por uma estrutura de turnos ou pela ação de outro combatente.
- **FR-007**: Inimigos DEVEM poder agir (mover, atacar) a qualquer momento durante o combate, de
  forma independente e simultânea às ações do jogador.
- **FR-008**: O sistema DEVE continuar exigindo um recurso limitado (equivalente aos pontos de
  ação/habilidade já existentes) para o uso de habilidades/magias em combate, para que seu uso
  continue sendo uma escolha com custo, não ilimitado.
- **FR-009**: Uma conjuração em andamento DEVE ser interrompida (sem produzir efeito) se o
  personagem que a conjura for atingido por um ataque antes de concluí-la.
- **FR-010**: O sistema DEVE fornecer feedback visual contínuo de vida/estado de cada
  combatente (jogador e inimigos) durante o combate, sem depender de uma tela de "status do
  turno".
- **FR-011**: A exploração fora de combate DEVE ser apresentada na mesma perspectiva 2D
  side-view usada no combate, substituindo a câmera isométrica 3D atual (feature 001/003).
- **FR-012**: O sistema DEVE preservar, sem alteração de regras, os sistemas já implementados de
  atributos, árvore de habilidades, sobrevivência (fome/sanidade) e reputação/economia — apenas
  a camada de combate e a apresentação visual mudam.
- **FR-013**: O sistema DEVE permitir fugir do combate em tempo real (reaproveitando a fórmula de
  chance de fuga já existente), sem depender de uma ação de menu de turno.
- **FR-014**: Quando o personagem do jogador é derrotado, o combate DEVE terminar em derrota
  imediatamente (mesma consequência já definida na feature 001).
- **FR-015**: A câmera (tanto na arena de combate quanto na Exploração) DEVE manter o personagem
  do jogador centralizado na tela, exceto quando ele estiver próximo de uma extremidade do
  mapa/arena — nesse caso a câmera DEVE parar de segui-lo naquele eixo e manter a borda do
  cenário visível, em vez de revelar espaço vazio além do limite do mundo.

### Key Entities

- **BattleArena**: espaço de combate 2D side-view com limites horizontais; substitui o `GridMap`
  da feature 001. Seus limites também definem até onde a câmera segue o personagem (FR-015).
- **RealTimeAction**: uma ação de combate (ataque básico, ataque à distância, habilidade/magia)
  com um tempo de execução/conjuração e, quando aplicável, um custo de recurso.
- **CombatArenaEncounter**: o estado de um combate em andamento — participantes (jogador e
  inimigos), suas posições na arena, e vida atual; substitui o `CombatEncounter` baseado em
  turnos da feature 001.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Um jogador consegue mover o personagem e conectar um ataque corpo a corpo em um
  inimigo em menos de 5 segundos após o início do combate, sem qualquer tela de seleção de ação
  bloqueando a jogabilidade.
- **SC-002**: Uma habilidade/magia com tempo de conjuração é interrompida corretamente sempre
  que o personagem que conjura é atingido antes da conclusão, em 100% dos casos testados.
- **SC-003**: O jogador percebe a mudança de perspectiva 3D-para-2D tanto na exploração quanto no
  combate como uma experiência visualmente coerente (mesma perspectiva lateral em ambas as
  telas).
- **SC-004**: Todos os sistemas preservados (atributos, habilidades, sobrevivência,
  reputação/economia) continuam passando em seus testes automatizados existentes sem alteração
  de comportamento, após a migração do combate.
- **SC-005**: Inimigos reagem e agem de forma autônoma e contínua durante 100% da duração de um
  combate observado em teste manual, sem depender de nenhuma ação do jogador para "liberar" a
  vez deles.
- **SC-006**: Em teste manual, o jogador nunca vê espaço vazio além da borda do mapa/arena ao
  caminhar até uma extremidade — a câmera permanece dentro dos limites do cenário em 100% das
  vezes observadas, tanto na Exploração quanto no combate.

## Assumptions

- **Sem grupo (party) nesta feature**: o jogador luta sozinho com o personagem criado na
  feature 002, contra um ou mais inimigos. Aliados controlados por IA com táticas configuráveis
  (mencionados na descrição original como inspirados no Tales of Phantasia) ficam fora do
  escopo — uma extensão futura que dependeria primeiro de um sistema de grupo/recrutamento ainda
  inexistente no projeto.
- O tempo de conjuração e os alcances (corpo a corpo/à distância) são valores configuráveis por
  habilidade (dados), não fixos no código, seguindo o Princípio II (arquitetura orientada a
  dados) da constituição.
- A arena de combate é gerada/configurada por encontro (como já ocorre com o `GridMap` da
  feature 001), não sendo necessário um editor visual de arenas nesta feature.
- Este pivô de arquitetura de combate substitui — não coexiste com — o sistema baseado em
  turnos/grid da feature 001; o código de grid/turnos existente é removido ou torna-se código
  morto após esta feature, conforme o Princípio V (Simplicidade).
- A exploração e o combate passam a compartilhar a mesma perspectiva 2D side-view (câmera
  ortográfica lateral), eliminando a câmera isométrica 3D usada até a feature 003.
