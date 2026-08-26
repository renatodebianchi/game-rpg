# Feature Specification: Assets Visuais do Personagem e da Interface + Exploração com Personagem Criado

**Feature Branch**: `003-character-visual-exploration`

**Created**: 2026-08-26

**Status**: Draft

**Input**: User description: "Crie uma nova /speckit-specify para adicionar assets visuais no personagem. Use arquivos abertos e livres de direitos autorais para criar a interface visual da aplicação e do personagem humanoide, também adicione capacidade da demo de exploração de usar o personagem criado."

## Clarifications

### Session 2026-08-26

- Q: Para o visual do personagem humanoide, qual abordagem seguimos (sprite 2D vs. modelo 3D rigado)? → A: Sprite 2D (top-down/isométrico), exibido sobre a câmera isométrica já existente — sem rig nem animação 3D.
- Q: Qual fonte de assets abertos/livres de direitos autorais devo usar como padrão? → A: Kenney.nl (licença CC0 — domínio público, sem exigência de atribuição), como fonte padrão para sprites de personagem e assets de interface.
- Q: O que entra no escopo de "interface visual da aplicação"? → A: Restilizar os componentes de UI compartilhados (botões, painéis, texto) já usados por todas as telas construídas em runtime, em vez de redesenhar cada tela individualmente.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ver e mover o personagem criado na demo de Exploração (Priority: P1)

Como jogador, depois de finalizar a criação do meu personagem, eu sou levado para a cena de
Exploração e vejo meu personagem representado por um sprite humanoide (em vez de um placeholder
genérico), podendo movê-lo pela cena usando o teclado.

**Why this priority**: É o núcleo do pedido — conectar a criação de personagem (feature 002) a
uma experiência de exploração real e visualmente reconhecível. Sem isso, "usar o personagem
criado" na exploração não existe; as demais histórias (fidelidade visual, reskin de UI) são
melhorias sobre essa base.

**Independent Test**: Pode ser testado finalizando uma criação de personagem, verificando a
transição para a Exploração, e movendo o personagem nas quatro direções, sem depender das
histórias de aparência fiel ou de reskin da interface.

**Acceptance Scenarios**:

1. **Given** o jogador acabou de finalizar a criação de personagem, **When** ele confirma a
   finalização, **Then** o jogo o leva para a cena de Exploração usando esse mesmo personagem
   (atributos, equipamento e características visuais escolhidos são preservados).
2. **Given** o jogador está na cena de Exploração, **When** a cena carrega, **Then** o
   personagem é exibido como um sprite humanoide, não como uma cápsula ou outro primitivo
   genérico.
3. **Given** o personagem está na cena de Exploração, **When** o jogador pressiona as teclas de
   movimento, **Then** o personagem se move na direção correspondente pela cena.
4. **Given** a cena de Exploração é aberta diretamente (sem passar pela criação de personagem
   nesta sessão), **When** a cena carrega, **Then** um personagem com características visuais
   padrão é exibido e pode ser movido normalmente.

---

### User Story 2 - Aparência do sprite reflete as características visuais escolhidas (Priority: P2)

Como jogador, ao personalizar a aparência do meu personagem na criação (tom de pele, tipo de
corpo, cabelo — feature 002, User Story 3), eu espero que essas escolhas sejam minimamente
perceptíveis no sprite exibido durante a exploração, para que meu personagem pareça
diferenciado do de outro jogador.

**Why this priority**: Reforça o valor da personalização já implementada (feature 002), mas
depende de a User Story 1 já existir (é preciso ter um sprite visível antes de fazê-lo variar).

**Independent Test**: Pode ser testado criando dois personagens com características visuais
diferentes (ex. tons de pele diferentes) e comparando os sprites exibidos na Exploração lado a
lado, sem depender da história de reskin da interface.

**Acceptance Scenarios**:

1. **Given** dois personagens foram criados com tons de pele diferentes, **When** cada um é
   exibido na Exploração, **Then** os sprites exibidos são visualmente diferenciáveis entre si
   por essa característica.
2. **Given** um personagem foi criado com uma combinação específica de características visuais,
   **When** ele é salvo e recarregado (persistência já existente na feature 001/002), **Then**
   o sprite exibido na Exploração reflete a mesma aparência de antes de salvar.

---

### User Story 3 - Interface visual estilizada com assets abertos (Priority: P3)

Como jogador, eu vejo botões, painéis e texto com uma aparência visual coerente (não retângulos
de cor sólida) em todas as telas construídas em runtime do jogo, para que a experiência pareça
mais polida.

**Why this priority**: É uma melhoria de acabamento visual que não bloqueia nenhuma
funcionalidade — pode ser adicionada por último sem prejudicar as demais histórias.

**Independent Test**: Pode ser testado abrindo qualquer uma das telas construídas em runtime
(Criação de Personagem, Exploração, ou as demos já existentes de combate/habilidades/
sobrevivência/reputação) e verificando que botões e painéis usam os novos assets visuais, sem
depender das histórias de personagem visual.

**Acceptance Scenarios**:

1. **Given** qualquer tela construída em runtime é aberta, **When** o jogador observa os botões
   e painéis, **Then** eles usam texturas/ícones de um pacote de assets aberto, não cor sólida
   lisa.
2. **Given** um novo botão ou painel é criado por qualquer controlador de UI existente (sem
   alteração individual nesse controlador), **When** a tela é exibida, **Then** o novo visual é
   aplicado automaticamente, por vir dos componentes de UI compartilhados.

---

### Edge Cases

- Se o pacote de sprites escolhido não tiver variação suficiente para representar todas as
  combinações possíveis de características visuais (feature 002: até 18 combinações), a
  representação usa a variação mais próxima disponível (ex. apenas tingimento de cor para tom
  de pele), sem tentar cobrir exatamente cada combinação.
- A cena de Exploração pode ser aberta diretamente, sem um personagem vindo da criação —
  nesse caso, um personagem com aparência e atributos padrão é usado (mesmo padrão de
  auto-inicialização já usado pelas demais demos da feature 001).
- Nenhum asset de terceiros usado neste projeto pode exigir pagamento, criação de conta ou
  atribuição obrigatória não documentada — apenas conteúdo sob licença aberta (ex. CC0) é
  aceitável.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE exibir o personagem do jogador como um sprite humanoide 2D na cena
  de Exploração, em vez de um primitivo/placeholder genérico.
- **FR-002**: O sistema DEVE permitir que o jogador mova o personagem pela cena de Exploração
  usando o teclado.
- **FR-003**: Ao finalizar a criação de personagem (feature 002), o sistema DEVE permitir que o
  jogador prossiga diretamente para a cena de Exploração usando o personagem recém-criado
  (atributos, equipamento e características visuais preservados).
- **FR-004**: Se a cena de Exploração for aberta sem um personagem vindo da criação, o sistema
  DEVE usar um personagem com características visuais e atributos padrão, permanecendo
  jogável.
- **FR-005**: O sistema DEVE variar visualmente o sprite do personagem exibido de acordo com ao
  menos uma característica visual escolhida na criação (feature 002, ex. tom de pele), na
  medida do que o pacote de assets escolhido permitir.
- **FR-006**: O sistema DEVE obter os sprites do personagem e os assets de interface (botões,
  painéis, ícones, fonte) de um pacote de assets sob licença aberta/livre de direitos autorais
  (CC0 ou equivalente), sem custo e sem exigência de atribuição obrigatória não documentada.
- **FR-007**: O sistema DEVE restilizar os componentes de UI compartilhados (usados por todas
  as telas construídas em runtime) com os novos assets visuais, de forma que toda tela que use
  esses componentes receba o novo visual automaticamente, sem precisar ser redesenhada
  individualmente.
- **FR-008**: O sistema DEVE manter um registro de créditos de assets do projeto, documentando
  a fonte e a licença de cada asset de terceiros introduzido por esta feature.
- **FR-009**: O sistema NÃO DEVE introduzir nenhum asset cuja licença restrinja uso comercial,
  exija pagamento, ou seja incompatível com o uso/modificação/redistribuição livre já assumido
  pelo projeto (constituição, Princípio II).

### Key Entities

- **Sprite do Personagem**: Representação visual 2D humanoide exibida na Exploração, associada
  (por tingimento de cor ou variação de sprite) a uma ou mais características visuais do
  personagem (feature 002, `VisualCharacteristics`).
- **Componentes de UI Compartilhados**: Métodos/elementos reutilizados por todas as telas
  construídas em runtime (botão, painel, texto) — o ponto único onde o novo visual é aplicado.
- **Registro de Créditos de Assets**: Lista de assets de terceiros usados no projeto, cada um
  com fonte e licença documentadas.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Ao finalizar a criação de personagem, o jogador chega à cena de Exploração vendo
  seu personagem como um sprite humanoide em uma única ação (ex. um clique em "Finalizar" ou
  "Jogar"), sem etapas manuais adicionais.
- **SC-002**: O jogador consegue mover o personagem nas quatro direções cardeais na cena de
  Exploração usando o teclado.
- **SC-003**: Comparando dois personagens criados com características visuais diferentes, pelo
  menos uma diferença é perceptível nos sprites exibidos na Exploração.
- **SC-004**: 100% dos botões e painéis das telas construídas em runtime (Criação de
  Personagem, Exploração, e as demos já existentes) usam os novos assets visuais — nenhum
  retângulo de cor sólida remanescente nos componentes compartilhados.
- **SC-005**: 100% dos assets de terceiros introduzidos por esta feature estão listados no
  registro de créditos, com fonte e licença documentadas, e nenhum deles exige pagamento,
  conta ou atribuição obrigatória não cumprida.

## Assumptions

- O personagem humanoide é representado como sprite 2D (não modelo 3D rigado/animado),
  exibido sobre a câmera isométrica já existente no projeto — decisão tomada explicitamente
  para manter o escopo simples (Princípio V), consistente com o estilo visual atual de
  primitivos/placeholders.
- A fonte padrão de assets abertos é o Kenney.nl (licença CC0), tanto para sprites de
  personagem quanto para os assets de interface (botões, painéis, ícones, fonte). Pacotes
  específicos a usar serão escolhidos durante o planejamento técnico.
- A variação visual do sprite conforme as características escolhidas na criação é
  "melhor esforço": não é necessário cobrir exatamente as ~18 combinações possíveis
  (feature 002) com sprites distintos — tingimento de cor (ex. por tom de pele) é uma
  abordagem aceitável.
- O reskin da interface (User Story 3) se aplica através dos métodos/componentes de UI já
  compartilhados por todas as telas construídas em runtime (ver
  `specs/001-isometric-sandbox-rpg/research.md`, "Decision: Tela de criação de personagem
  construída em runtime via UGUI"), não através de um redesenho individual de cada tela.
- Movimentação do personagem na Exploração usa um esquema de teclado simples (ex. WASD/setas)
  equivalente ao já estabelecido para outras interações no projeto — não introduz um novo
  sistema de input além do que já existe.
- Este é um MVP visual: não inclui animações de caminhada, expressões faciais, ou uma segunda
  camada de customização visual além do que a feature 002 já captura.
