# Feature Specification: Criação de Personagem (Atributos, Aparência e Equipamento Inicial)

**Feature Branch**: `002-character-creation`

**Created**: 2026-08-26

**Status**: Draft

**Input**: User description: "adicione a feature de criação de personagem. Deve ser possível adicionar os atributos base do personagem, permitindo pontuação inicial de atributos usando o padrão D&D 5e, caracteristicas visuais básicas e equipamento."

## Clarifications

### Session 2026-08-26

- Q: O personagem hoje tem 4 atributos (Força, Destreza, Intelecto, Vontade). Mantemos esses 4 atributos (aplicando a pontuação D&D a eles) ou expandimos para os 6 atributos clássicos de D&D? → A: Manter os 4 atributos já existentes (Força, Destreza, Intelecto, Vontade); a pontuação D&D 5e é aplicada a esse conjunto, sem expandir para os 6 atributos clássicos.
- Q: Qual método de pontuação de atributos do D&D 5e o jogador vai usar? → A: Point Buy, rebalanceado para o modelo de 4 atributos (em vez do Point Buy oficial de 27 pontos para 6 atributos).
- Q: Como funciona a escolha de equipamento inicial na criação de personagem? → A: Kit fixo por trilha — o jogo atribui um conjunto fixo de itens iniciais com base na orientação predominante (combatente/arcanista) escolhida pelo jogador, sem seleção manual de itens.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Definir atributos base por Point Buy (Priority: P1)

Como jogador, ao criar um novo personagem eu distribuo um orçamento fixo de pontos entre os
quatro atributos base (Força, Destreza, Intelecto, Vontade), seguindo a curva de custo do
método Point Buy do D&D 5e, para moldar mecanicamente o estilo do meu personagem antes de
começar a jogar.

**Why this priority**: Os atributos base alimentam diretamente as fórmulas de combate, os
pré-requisitos de habilidades e as penalidades de sobrevivência já existentes no jogo (feature
001). Sem essa alocação, não há personagem mecanicamente válido para iniciar qualquer outra
parte da experiência — é o núcleo indispensável da criação de personagem.

**Independent Test**: Pode ser testado abrindo a tela de criação de personagem, distribuindo o
orçamento de pontos entre os quatro atributos e verificando que o personagem resultante reflete
exatamente os valores escolhidos, sem depender das histórias de equipamento ou aparência.

**Acceptance Scenarios**:

1. **Given** o jogador inicia a criação de um novo personagem, **When** a tela de atributos é
   aberta, **Then** os quatro atributos começam em 8 e o jogador tem 18 pontos disponíveis para
   gastar.
2. **Given** o jogador aumenta um atributo, **When** o novo valor está entre 9 e 15, **Then** o
   custo correspondente (1, 2, 3, 4, 5, 7 ou 9 pontos cumulativos a partir de 8) é descontado do
   orçamento disponível.
3. **Given** o jogador tenta aumentar um atributo além de 15, **When** essa ação é solicitada,
   **Then** o sistema impede o aumento e comunica que 15 é o valor máximo permitido na criação.
4. **Given** o jogador tenta finalizar a criação do personagem, **When** ainda restam pontos não
   gastos ou o orçamento foi excedido, **Then** o sistema impede a finalização e indica quantos
   pontos precisam ser ajustados.
5. **Given** o jogador gastou exatamente os 18 pontos disponíveis, **When** ele avança para a
   próxima etapa da criação, **Then** o sistema permite prosseguir.

---

### User Story 2 - Receber equipamento inicial pela orientação escolhida (Priority: P2)

Como jogador, eu escolho uma orientação predominante (combatente ou arcanista) para o meu
personagem e recebo automaticamente um kit fixo de equipamento inicial adequado a essa
orientação, para começar a jogar já equipado de forma coerente com o estilo pretendido.

**Why this priority**: Ter equipamento inicial é necessário para que o personagem participe de
um combate logo no início do jogo (feature 001, User Story 1), mas depende dos atributos (P1) já
estarem definidos para que a experiência de criação siga uma ordem lógica.

**Independent Test**: Pode ser testado escolhendo cada orientação disponível (combatente,
arcanista) isoladamente e verificando que o personagem resultante recebe o kit de equipamento
fixo correspondente, sem depender das outras histórias.

**Acceptance Scenarios**:

1. **Given** o jogador está na etapa de orientação, **When** ele escolhe "combatente", **Then**
   o personagem recebe o kit de equipamento inicial fixo associado a essa orientação.
2. **Given** o jogador está na etapa de orientação, **When** ele escolhe "arcanista", **Then** o
   personagem recebe o kit de equipamento inicial fixo associado a essa orientação.
3. **Given** o jogador escolheu uma orientação na criação, **When** o personagem entra no jogo e
   investe pontos de habilidade posteriormente, **Then** ele pode investir livremente em
   qualquer trilha da árvore de habilidades (combatente, arcanista ou híbrida), sem estar restrito
   pela orientação escolhida na criação — essa escolha afeta apenas o kit de equipamento inicial.

---

### User Story 3 - Personalizar características visuais básicas (Priority: P3)

Como jogador, eu escolho características visuais básicas (tipo de corpo, tom de pele, estilo e
cor de cabelo) entre opções predefinidas, para que meu personagem tenha uma aparência
distinguível da de outros jogadores.

**Why this priority**: É a camada de personalização de menor impacto mecânico — não afeta
combate, habilidades ou sobrevivência — e por isso é a mais segura de priorizar por último caso
o escopo precise ser reduzido.

**Independent Test**: Pode ser testado abrindo a etapa de aparência isoladamente, selecionando
cada característica visual disponível e verificando que o personagem resultante reflete as
escolhas feitas, sem depender das histórias de atributos ou equipamento.

**Acceptance Scenarios**:

1. **Given** o jogador está na etapa de aparência, **When** ele seleciona um tipo de corpo, um
   tom de pele e um estilo/cor de cabelo entre as opções predefinidas, **Then** o resumo do
   personagem reflete essas escolhas.
2. **Given** o jogador ainda não fez uma seleção em alguma característica visual, **When** ele
   tenta finalizar a criação do personagem, **Then** o sistema aplica uma opção padrão
   predefinida em vez de bloquear a finalização.

---

### Edge Cases

- Se o jogador sair da criação de personagem antes de finalizar, o progresso feito não é salvo
  como rascunho — reabrir a criação começa do zero.
- A orientação (combatente/arcanista) escolhida na criação determina apenas o kit de
  equipamento inicial; ela não bloqueia nem prioriza nenhuma trilha da árvore de habilidades
  (feature 001, FR-004/FR-005) durante o restante do jogo.
- Uma vez finalizada a criação, os atributos base não podem mais ser redistribuídos — isso é
  distinto do respec da árvore de habilidades (feature 001, FR-018), que afeta apenas nós de
  habilidade adquiridos, não os atributos base.
- O que acontece se o jogador tentar reduzir um atributo abaixo de 8 (o mínimo do Point Buy)?
- O que acontece se o jogo for encerrado/travar durante a criação, antes da finalização — o
  personagem incompleto pode acabar entrando em uma sessão de jogo?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer um fluxo de criação de personagem que é concluído antes de
  o personagem entrar em qualquer sistema de jogo (combate, árvore de habilidades, sobrevivência,
  reputação/economia — feature 001).
- **FR-002**: O sistema DEVE permitir que o jogador distribua pontos entre os quatro atributos
  base (Força, Destreza, Intelecto, Vontade) usando o método Point Buy: cada atributo começa em
  8, o jogador tem 18 pontos no total, e o custo para elevar um atributo segue a curva do D&D 5e
  (9→1, 10→2, 11→3, 12→4, 13→5, 14→7, 15→9 pontos cumulativos a partir de 8), sem nenhum
  atributo podendo passar de 15 durante a criação.
- **FR-003**: O sistema DEVE impedir a finalização da criação do personagem enquanto restarem
  pontos de atributo não gastos ou o orçamento tiver sido excedido, indicando ao jogador o que
  precisa ser ajustado.
- **FR-004**: O sistema DEVE permitir que o jogador escolha uma orientação predominante
  (combatente ou arcanista) exclusivamente para determinar o kit de equipamento inicial, sem
  restringir em quais trilhas da árvore de habilidades (combatente, arcanista ou híbrida) o
  jogador poderá investir posteriormente.
- **FR-005**: O sistema DEVE atribuir ao personagem um kit de equipamento inicial fixo
  correspondente à orientação escolhida.
- **FR-006**: O sistema DEVE permitir que o jogador selecione características visuais básicas
  (no mínimo: tipo de corpo, tom de pele e estilo/cor de cabelo) a partir de um conjunto
  predefinido de opções.
- **FR-007**: O sistema DEVE aplicar uma opção visual padrão para qualquer característica visual
  não selecionada explicitamente pelo jogador, em vez de bloquear a finalização da criação.
- **FR-008**: O sistema DEVE apresentar um resumo de todas as escolhas de criação (atributos,
  orientação/equipamento, características visuais) antes da finalização.
- **FR-009**: O sistema DEVE permitir que o jogador revise e altere qualquer escolha anterior
  (atributos, orientação, características visuais) antes de finalizar a criação do personagem.
- **FR-010**: Ao finalizar, o sistema DEVE aplicar os atributos, o equipamento inicial e as
  características visuais escolhidos ao personagem usado em jogo (combate, árvore de
  habilidades, sobrevivência, reputação — feature 001).
- **FR-011**: O sistema DEVE persistir os atributos, o equipamento inicial e as características
  visuais do personagem finalizado como parte dos dados de save (consistente com o requisito de
  persistência FR-017 da feature 001).
- **FR-012**: Uma vez finalizada a criação do personagem, o sistema DEVE tratar os atributos
  base como fixos pelo restante da campanha — distinto do respec da árvore de habilidades
  (feature 001, FR-018), que afeta apenas os nós de habilidade adquiridos, não os atributos base.

### Key Entities

- **Perfil de Criação de Personagem**: Agrega as escolhas feitas durante o fluxo de criação
  (alocação de atributos, orientação escolhida, características visuais) até serem finalizadas e
  aplicadas ao personagem de jogo.
- **Alocação de Atributos**: Registro de quantos pontos do orçamento Point Buy foram investidos
  em cada um dos quatro atributos base.
- **Kit de Equipamento Inicial**: Conjunto fixo de itens associado a uma orientação
  (combatente ou arcanista), atribuído ao personagem ao final da criação.
- **Características Visuais**: Conjunto de escolhas cosméticas (tipo de corpo, tom de pele,
  estilo/cor de cabelo) aplicadas ao personagem, sem efeito em atributos, combate ou habilidades.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Jogadores conseguem concluir o fluxo completo de criação de personagem (atributos,
  orientação/equipamento e aparência) em menos de 5 minutos.
- **SC-002**: 100% dos personagens que entram em uma sessão de jogo possuem uma alocação de
  Point Buy válida (todos os 18 pontos gastos, nenhum atributo acima de 15).
- **SC-003**: 100% dos personagens recém-criados iniciam seu primeiro encontro de combate já
  equipados com o kit inicial correspondente à orientação escolhida.
- **SC-004**: Jogadores conseguem produzir pelo menos 3 aparências de personagem visualmente
  distintas usando apenas as opções básicas de personalização disponíveis.

## Assumptions

- A criação de personagem acontece uma única vez por campanha/save; não há recriação do
  personagem em uma campanha em andamento (o respec de habilidades já existente na feature 001
  é um mecanismo separado e não se aplica aos atributos base).
- Sair da criação de personagem antes de finalizar descarta todo o progresso feito nela; não há
  persistência de rascunho para esta funcionalidade.
- O kit de equipamento inicial é conteúdo fixo por orientação nesta primeira versão; um
  catálogo navegável de equipamentos para seleção manual item a item está fora de escopo (por
  escolha explícita do usuário: "kit fixo por trilha").
- As características visuais desta primeira versão se limitam a um pequeno conjunto de opções
  predefinidas (tipo de corpo, tom de pele, estilo/cor de cabelo), representadas como dados de
  configuração, consistente com a ausência atual de um pipeline de arte/customização visual 3D
  no projeto. Customização visual mais avançada (sliders contínuos, texturas customizadas) é
  uma evolução futura fora de escopo aqui.
- O orçamento de 18 pontos do Point Buy (em vez dos 27 pontos oficiais do D&D 5e para 6
  atributos) é um rebalanceamento proporcional para o modelo de 4 atributos deste jogo
  (27 × 4/6 = 18), preservando a mesma curva de custo por valor de atributo do D&D 5e original
  (8→0, 9→1, 10→2, 11→3, 12→4, 13→5, 14→7, 15→9).
- Esta feature estende o modelo de personagem já existente (feature 001); ela não introduz um
  novo tipo de personagem jogável, apenas o fluxo e os dados usados para inicializá-lo.
