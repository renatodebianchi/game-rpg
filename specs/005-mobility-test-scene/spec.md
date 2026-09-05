# Feature Specification: Cena de Teste de Mobilidade

**Feature Branch**: `[005-mobility-test-scene]`

**Created**: 2026-09-05

**Status**: Draft

**Input**: User description: "Crie uma cena inicial de teste de mobilidade: um cenário 2D plano com plataformas/obstáculos para testar a movimentação do personagem. O personagem deve poder: andar, correr, pular, pulo duplo, deslizar por uma parede lateral (wall slide), pular a partir da parede (wall jump), dar um "kick" na parede para impulsionar o pulo (mobilidade estilo Samus em Super Metroid), e um salto de energia acumulada — o personagem se abaixa para carregar energia e então salta verticalmente até colidir com o teto (estilo a habilidade da Hornet em Silksong). Adicione um background 2D com textura ao ambiente da cena, e um sprite visual do personagem com animações de andar, abaixar, pular e demais estados de movimentação. Pode ser necessário baixar novos assets visuais (sprites/tilesets) de fontes abertas/livres de direitos autorais para viabilizar isso — download autorizado."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Movimentação básica no cenário de teste (Priority: P1)

Como jogador, eu abro uma cena dedicada de teste de mobilidade e vejo um cenário 2D plano, com
fundo texturizado e algumas plataformas/obstáculos de alturas diferentes, e consigo andar e
correr pelo chão e pular, vendo o personagem animado de acordo (parado, andando, correndo,
pulando).

**Why this priority**: É a base sem a qual nenhuma das habilidades mais avançadas (P2, P3) pode
ser testada — locomoção e salto básicos são pré-requisito de tudo mais nesta cena.

**Independent Test**: Abrir a cena de teste de mobilidade, andar de um lado a outro do cenário,
correr segurando o comando de corrida, e pular do chão — observando o personagem animado em cada
estado, sem depender de nenhuma habilidade das outras histórias.

**Acceptance Scenarios**:

1. **Given** a cena de teste de mobilidade aberta, **Then** o cenário exibe um chão plano, um
   fundo com textura visual (não uma cor sólida), e ao menos uma plataforma/obstáculo elevado.
2. **Given** o personagem no chão, **When** o jogador aciona o comando de movimento horizontal,
   **Then** o personagem anda, com a animação de andar visível.
3. **Given** o personagem andando, **When** o jogador mantém pressionado o comando de corrida,
   **Then** o personagem se move mais rápido que andando, com uma animação de corrida
   perceptivelmente diferente da de andar.
4. **Given** o personagem no chão, **When** o jogador aciona o comando de pular, **Then** o
   personagem salta, com uma animação de pulo, e retorna ao chão pela gravidade.

---

### User Story 2 - Mobilidade aérea avançada (pulo duplo e parede) (Priority: P2)

Como jogador, no mesmo cenário de teste, eu consigo pular novamente no ar (pulo duplo), e ao
encostar em uma parede vertical enquanto caio, deslizo por ela em vez de cair livremente, e
posso pular a partir dela — impulsionando o personagem para longe da parede e para cima, como no
estilo de mobilidade da Samus em Super Metroid.

**Why this priority**: Depende do pulo básico da User Story 1 já existir, mas é a mobilidade
"estilo Metroidvania" central que o pedido enfatiza — entrega valor identificável por si só,
mesmo sem o salto de energia acumulada da User Story 3.

**Independent Test**: No cenário de teste, pular do chão e acionar o pulo novamente no ar
(pulo duplo), e separadamente pular contra uma das paredes verticais do cenário, deslizar por
ela, e pular a partir dela para o lado oposto — sem depender do salto de energia acumulada.

**Acceptance Scenarios**:

1. **Given** o personagem no ar após o pulo do chão, **When** o jogador aciona o comando de
   pular novamente (sem ter tocado o chão), **Then** o personagem realiza um pulo aéreo (pulo
   duplo), com animação distinta do pulo do chão. Por padrão, apenas 1 pulo aéreo é permitido
   por salto do chão.
2. **Given** o personagem já usou o pulo aéreo disponível (1 por padrão) sem tocar o chão,
   **When** o jogador aciona o comando de pular novamente, **Then** nada acontece — não há um
   terceiro pulo no ar por padrão. O limite de pulos aéreos é configurável para permitir mais
   no futuro (ex. uma habilidade especial), mas nunca é excedido enquanto não for explicitamente
   aumentado.
3. **Given** o personagem caindo e encostando em uma parede vertical, **Then** ele passa a
   deslizar por ela a uma velocidade de queda reduzida, com uma animação de deslizar na parede.
4. **Given** o personagem deslizando em uma parede, **When** o jogador aciona o comando de
   pular, **Then** o personagem é impulsionado para longe da parede e para cima (pulo a partir
   da parede), podendo repetir a manobra ao alcançar outra parede.

---

### User Story 3 - Salto de energia acumulada (Priority: P3)

Como jogador, no mesmo cenário de teste, eu consigo abaixar o personagem no chão, carregar
energia enquanto mantenho o comando de abaixar pressionado, e ao soltá-lo o personagem salta
verticalmente com força proporcional à energia acumulada, subindo até colidir com um teto ou
até perder o impulso — como a habilidade da Hornet em Silksong.

**Why this priority**: É a habilidade mais especializada e menos essencial das três; agrega
valor de teste adicional, mas o cenário já é útil para testar mobilidade sem ela (P1/P2).

**Independent Test**: No cenário de teste, abaixar o personagem no chão sob uma das plataformas
elevadas, segurar o comando de abaixar por diferentes durações, soltar, e observar o personagem
saltar verticalmente com altura proporcional ao tempo carregado, até colidir com a plataforma
acima ou atingir o limite de altura — sem depender de nenhuma habilidade das outras histórias.

**Acceptance Scenarios**:

1. **Given** o personagem no chão, **When** o jogador aciona o comando de abaixar, **Then** o
   personagem se abaixa, com uma animação de agachado.
2. **Given** o personagem agachado, **When** o jogador mantém o comando pressionado, **Then**
   uma quantidade de energia acumulada aumenta continuamente (com feedback visual), até um
   limite máximo.
3. **Given** o personagem agachado com energia acumulada, **When** o jogador solta o comando de
   abaixar, **Then** o personagem salta verticalmente com altura proporcional à energia
   acumulada (mais energia mantida = salto mais alto, até o limite máximo).
4. **Given** o personagem saltando por energia acumulada, **When** ele colide com um teto ou
   obstáculo acima, **Then** o salto é interrompido nesse ponto, sem atravessar o obstáculo.
5. **Given** o personagem agachado, **When** o jogador solta o comando de abaixar sem ter
   acumulado energia (soltou quase imediatamente), **Then** o personagem apenas se levanta, sem
   saltar.

---

### Edge Cases

- O que acontece se o jogador tentar correr no ar (sem estar no chão)? O comando de corrida não
  tem efeito enquanto o personagem está no ar — a velocidade aérea é definida pelo pulo/queda,
  não pela corrida.
- O que acontece se o personagem estiver deslizando na parede e tocar o chão? Ele volta ao
  estado normal de chão (parado/andando), sem manter o deslizar.
- O que acontece se o jogador tentar abaixar enquanto o personagem está no ar? O comando de
  abaixar não tem efeito no ar — abaixar e carregar energia só ocorrem com o personagem no chão.
- O que acontece se o teto acima do salto de energia acumulada estiver muito próximo do chão? O
  salto termina ao colidir com ele, mesmo que a energia acumulada permitisse mais altura.
- O que acontece se o jogador pular da parede e imediatamente encostar em outra parede próxima?
  O personagem passa a deslizar por essa nova parede normalmente, podendo repetir a manobra.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer uma cena dedicada de teste de mobilidade, com um cenário
  2D plano contendo o chão, ao menos uma plataforma elevada, e ao menos uma parede vertical
  alta o suficiente para testar deslizar/pular na parede.
- **FR-002**: O jogador DEVE poder mover o personagem horizontalmente (andar) enquanto ele está
  no chão.
- **FR-003**: O jogador DEVE poder correr (deslocamento horizontal mais rápido que andar)
  enquanto mantém pressionado um comando dedicado de corrida, com o personagem no chão.
- **FR-004**: O jogador DEVE poder pular a partir do chão.
- **FR-005**: O jogador DEVE poder realizar um pulo aéreo adicional (pulo duplo) após o pulo do
  chão, sem tocar o chão entre eles, até um limite configurável — 1 pulo aéreo por padrão; além
  do limite configurado, pular no ar não tem efeito. O limite DEVE ser ajustável (não fixo no
  código) para permitir mais pulos aéreos no futuro via uma habilidade especial, sem
  exigir mudança na regra em si.
- **FR-006**: Quando o personagem está no ar e em contato com uma parede vertical, ele DEVE
  deslizar por ela a uma velocidade de queda reduzida, em vez de cair livremente.
- **FR-007**: Enquanto desliza por uma parede, o jogador DEVE poder pular a partir dela,
  impulsionando o personagem para longe da parede e para cima (pulo a partir da parede/"kick"
  na parede, estilo Super Metroid).
- **FR-008**: O jogador DEVE poder abaixar o personagem enquanto ele está no chão.
- **FR-009**: Enquanto o personagem está abaixado, uma quantidade de energia acumulada DEVE
  aumentar continuamente enquanto o comando de abaixar permanece pressionado, até um limite
  máximo.
- **FR-010**: Ao soltar o comando de abaixar com energia acumulada, o personagem DEVE saltar
  verticalmente com altura proporcional à energia acumulada (dentro de um limite máximo), até
  colidir com um teto/obstáculo acima ou perder o impulso.
- **FR-011**: O sistema DEVE fornecer feedback visual de cada estado de movimentação do
  personagem (parado, andando, correndo, pulando, pulo duplo, caindo, agachado/carregando
  energia, deslizando na parede) através de um sprite com animações correspondentes a cada
  estado.
- **FR-012**: O cenário DEVE ter um fundo 2D com textura visual (não uma cor sólida), dando
  sensação de ambiente em vez de um vazio.
- **FR-013**: Todos os assets visuais de terceiros usados (sprites do personagem, texturas de
  cenário/fundo) DEVEM ser de fontes abertas/livres de direitos autorais, com os créditos
  correspondentes registrados (mesma política já usada na feature 003).
- **FR-014**: A cena de teste de mobilidade DEVE ser um cenário isolado/independente, aberto
  diretamente para teste manual — sem alterar a movimentação já existente na Exploração
  (feature 003/004) ou no combate (feature 004).

### Key Entities

- **MobilityTestScene**: a cena dedicada com o cenário plano (chão, plataformas, paredes) usado
  para testar a movimentação.
- **CharacterMovementController**: o comportamento que rege os estados de movimentação do
  personagem nesta cena (parado, andando, correndo, pulando, pulo duplo, caindo, agachado,
  carregando energia, deslizando na parede, saltando por energia acumulada) e a transição de
  animação correspondente a cada um.
- **ChargeJump**: a mecânica de energia acumulada — quantidade acumulada (0 até um máximo) e a
  altura de salto resultante ao ser liberada.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Um jogador consegue, em uma única sessão de teste manual, exercitar cada uma das
  habilidades de movimentação desta feature (andar, correr, pular, pulo duplo, deslizar na
  parede, pular da parede, agachar/carregar/saltar por energia) pelo menos uma vez.
- **SC-002**: A diferença de velocidade entre andar e correr é perceptível imediatamente ao
  acionar o comando de corrida, sem atraso perceptível.
- **SC-003**: O personagem nunca atravessa o chão, uma plataforma, ou uma parede durante nenhuma
  das manobras (pulo, pulo duplo, deslizar/pular na parede, salto de energia acumulada), em
  100% das tentativas observadas em teste manual.
- **SC-004**: O salto de energia acumulada varia visivelmente de altura conforme o tempo em que
  o comando de abaixar foi mantido pressionado, até o limite máximo (teto ou altura máxima).
- **SC-005**: Em 100% dos estados de movimentação listados em FR-011, o jogador consegue
  identificar visualmente em qual estado o personagem está, apenas observando a animação do
  sprite.

## Assumptions

- **Cena isolada de teste técnico**: esta cena não se conecta à navegação de Exploração/Combate
  existente (FR-014) — é aberta diretamente no Editor para teste manual, seguindo o mesmo padrão
  já estabelecido pelas demais cenas de demo do projeto (feature 001-004). As habilidades de
  movimentação aqui testadas não substituem nem afetam o `ExplorationCharacterController` ou o
  `BattleArenaDemoController` existentes.
- Pulo duplo, deslizar/pular na parede, e o salto de energia acumulada são habilidades sempre
  disponíveis ao personagem nesta cena (não desbloqueadas via árvore de habilidades) — é uma
  cena de teste técnico de movimentação, não conteúdo de progressão do RPG.
- "Correr" é ativado ao segurar um comando dedicado (ex. uma tecla modificadora), reaproveitando
  o padrão de comandos por teclado já usado nos demais controllers do projeto.
- Os obstáculos/plataformas do cenário são estáticos — não há inimigos, hazards, ou elementos
  que causem dano nesta feature.
- O sprite do personagem usado nesta cena pode ser um asset novo, baixado de uma fonte gratuita
  (Anokolisa, no itch.io), diferente do sprite estático da feature 003 caso este não possua os
  quadros de animação (andar, abaixar, pular, etc.) exigidos aqui.
- O fundo texturizado do cenário (FR-012) é uma imagem 2D estática (parallax não é exigido nesta
  feature).
