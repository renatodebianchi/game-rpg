# Feature Specification: RPG Sandbox com Árvore de Habilidades, Combate Tático e Mundo Reativo

**Feature Branch**: `001-isometric-sandbox-rpg`

**Created**: 2026-08-25

**Status**: Draft

**Input**: User description: "Crie um novo projeto de jogo RPG sandbox com árvore de habilidades muito rica e que possibilite o personagem principal atuar de maneira versátil, podendo optar por trilhas de combatente aprimorando aspectos físicos ou arcanista aprimorando aspectos místicos e intelectuais. O jogo não deve ter um sistema de classes tradicionais, mas as habilidades escolhidas na árvore devem fornecer os meios para o jogador diversificar ou especializar seu personagem da maneira mais criativa possível. Os combates devem ser em turnos, semelhante a Baldur's Gate 3, em visão isométrica. Importante, as escolhas do personagem devem impactar diretamente sua reputação e a forma como os NPC e a história local se desenvolve, escolhas como salvar ou não alguém ou mover um recurso de um lugar para outro devem ser relevantes, a ponto de se não houver recursos em uma determinada vila, os NPC podem morrer de fome e isso impactar a economia do jogo. O jogo também precisa adicionar critérios de survival, adicionando mecânicas de fome e sanidade. Em resumo, o jogo deve fornecer uma experiência de vivenciar um mundo fantástico, sobrevivendo a perigos, personalizando seu personagem e lutando com inimigos em turnos em uma visão isométrica."

## Clarifications

### Session 2026-08-25

- Q: Os pontos investidos na árvore de habilidades podem ser desalocados/redistribuídos (respec) depois, ou o investimento é permanente? → A: Respec livre e ilimitado — o jogador pode redistribuir pontos a qualquer momento, sem custo.
- Q: Quando uma vila perde 100% da população por fome, ela fica permanentemente inativa pelo resto da campanha ou pode voltar a ser repovoada? → A: Permanentemente inativa — estado terminal; repor recursos depois não reverte a perda populacional já ocorrida.
- Q: A reputação do jogador é totalmente independente por comunidade, ou ações que favorecem uma facção afetam automaticamente a reputação com facções rivais? → A: Totalmente independente — cada comunidade tem sua própria reputação isolada, sem efeito cruzado entre facções.
- Q: Quando fome e sanidade estão ambas em nível crítico ao mesmo tempo, as penalidades de ambas se acumulam, ou existe alguma prioridade/limite entre elas? → A: Penalidades acumulam — os efeitos de fome e sanidade são aplicados juntos, sem teto especial nem prioridade entre eles.
- Q: Se um NPC com reputação positiva for forçado a lutar contra o jogador, ferir ou matar esse NPC conta como uma escolha de impacto negativa igual a uma escolha deliberada, ou é tratado como exceção sem penalidade de reputação? → A: Conta como escolha negativa — aplica o mesmo efeito de reputação que uma escolha deliberada de não salvar/prejudicar o NPC.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Combate tático por turnos em visão isométrica (Priority: P1)

Como jogador, eu entro em um encontro hostil e resolvo o combate através de um sistema de
turnos em uma câmera isométrica, movimentando meu personagem em uma grade, usando ações,
ações bônus e movimento, e vendo o resultado de cada decisão tática imediatamente.

**Why this priority**: O combate é o loop de jogabilidade mais frequente e testável isoladamente;
sem ele não há forma de validar se as habilidades escolhidas na árvore têm efeito prático. É a
base sobre a qual as demais histórias (customização, sobrevivência, reputação) se apoiam.

**Independent Test**: Pode ser totalmente testado colocando o jogador em um encontro contra um
ou mais inimigos em um mapa de grade isométrico e verificando que o combate pode ser vencido,
perdido ou fugido através de decisões em turnos, sem depender de nenhuma outra história.

**Acceptance Scenarios**:

1. **Given** o personagem está explorando o mundo e encontra um inimigo hostil, **When** o
   encontro é iniciado, **Then** o jogo transiciona para o modo de combate em turnos, calcula a
   ordem de iniciativa de todos os combatentes e exibe o mapa em grade isométrica.
2. **Given** é o turno do personagem do jogador, **When** o jogador gasta movimento, uma ação e
   uma ação bônus (por exemplo, mover-se, atacar e usar uma habilidade rápida), **Then** cada
   recurso de turno é consumido corretamente e não pode ser reutilizado até o próximo turno.
3. **Given** um combate está em andamento, **When** todos os inimigos são derrotados ou fogem,
   **Then** o combate termina, recompensas (experiência/loot) são concedidas e o jogo retorna ao
   modo de exploração.
4. **Given** o personagem do jogador é derrotado em combate, **When** seus pontos de vida chegam
   a zero, **Then** o jogo aplica uma consequência clara de derrota (por exemplo, reinício a
   partir do último ponto seguro) sem travar ou corromper o progresso salvo.

---

### User Story 2 - Árvore de habilidades sem classes fixas (Priority: P2)

Como jogador, eu distribuo pontos de habilidade em uma árvore rica e interconectada, podendo
investir tanto em trilhas de combatente (aspectos físicos) quanto de arcanista (aspectos
místicos/intelectuais), combinando-as livremente para criar uma build própria, sem ser
travado em uma classe predefinida.

**Why this priority**: É o segundo pilar central da experiência (diferenciação e progressão do
personagem) e depende do sistema de combate (P1) já existir para que o impacto das escolhas seja
observável e testável em situações reais.

**Independent Test**: Pode ser testado criando um personagem novo, distribuindo pontos de
habilidade em nós de diferentes trilhas e verificando que as habilidades escolhidas ficam
disponíveis para uso (em combate ou exploração) refletindo a combinação específica escolhida,
sem exigir que as histórias de sobrevivência ou reputação estejam implementadas.

**Acceptance Scenarios**:

1. **Given** o jogador ganha um ponto de habilidade, **When** ele abre a árvore de habilidades,
   **Then** o jogo mostra os nós disponíveis para investimento nas trilhas de combatente e
   arcanista, incluindo nós híbridos que exigem pré-requisitos de ambas as trilhas.
2. **Given** o jogador investe pontos majoritariamente em nós de combatente, **When** ele entra
   em combate, **Then** as habilidades físicas aprendidas (por exemplo, ataques aprimorados,
   resistência) estão disponíveis e mensuravelmente diferentes de uma build arcanista.
3. **Given** o jogador investe pontos em nós de ambas as trilhas, **When** os pré-requisitos de
   um nó híbrido são atendidos, **Then** esse nó híbrido se torna disponível para investimento,
   permitindo uma build de especialização mista.
4. **Given** uma habilidade já foi aprendida, **When** o jogador tenta reinvestir os mesmos
   pontos nela, **Then** o sistema impede o investimento duplicado e comunica claramente que a
   habilidade já foi adquirida.
5. **Given** o jogador já investiu pontos em nós da árvore, **When** ele solicita redistribuir
   (respec) esses pontos, **Then** o sistema desfaz a aquisição dos nós selecionados, devolve os
   pontos de habilidade como disponíveis e permite reinvesti-los livremente, sem custo.

---

### User Story 3 - Sobrevivência: fome e sanidade (Priority: P3)

Como jogador, eu preciso gerenciar as necessidades básicas do meu personagem — alimentação e
estabilidade mental — enquanto exploro o mundo, sofrendo penalidades crescentes quando essas
necessidades são negligenciadas.

**Why this priority**: Adiciona a camada de risco/gerenciamento de recursos que diferencia um
sandbox de sobrevivência de um RPG tático puro. Depende do combate (P1) e da progressão de
personagem (P2) já existirem, pois as penalidades de sobrevivência devem afetar essas
capacidades de forma observável.

**Independent Test**: Pode ser testado deixando o personagem sem se alimentar ou exposto a
eventos perturbadores por um período definido e verificando que os indicadores de fome/sanidade
degradam e aplicam penalidades mensuráveis (por exemplo, redução de desempenho em combate),
independentemente do estado de reputação/economia do mundo.

**Acceptance Scenarios**:

1. **Given** o personagem não consome alimento por um período prolongado, **When** o indicador de
   fome atinge limiares críticos, **Then** o personagem sofre penalidades crescentes (por
   exemplo, redução de atributos físicos ou de recuperação) até a fome ser satisfeita.
2. **Given** o personagem consome alimento disponível, **When** a ação de se alimentar é
   concluída, **Then** o indicador de fome é restaurado proporcionalmente à qualidade/quantidade
   do alimento consumido.
3. **Given** o personagem vivencia eventos perturbadores (combates extremos, ambientes
   sobrenaturais, isolamento prolongado), **When** esses eventos ocorrem, **Then** o indicador de
   sanidade é reduzido e, abaixo de limiares críticos, efeitos negativos (por exemplo,
   alucinações ou penalidades em testes mentais/mágicos) são aplicados.
4. **Given** o indicador de sanidade está criticamente baixo, **When** o jogador toma ações para
   restaurá-lo (descanso, itens específicos, ambientes seguros), **Then** o indicador de sanidade
   se recupera e os efeitos negativos associados são removidos.

---

### User Story 4 - Reputação e mundo reativo (economia e NPCs) (Priority: P4)

Como jogador, minhas escolhas morais e logísticas (salvar ou não um NPC, mover recursos entre
locais) alteram permanentemente minha reputação e o estado do mundo — incluindo a possibilidade
de uma vila entrar em colapso econômico e seus NPCs morrerem de fome se recursos essenciais forem
removidos ou não repostos.

**Why this priority**: É a camada de maior complexidade sistêmica (simulação de economia e
população) e tem maior valor quando as outras três histórias já existem, pois a reputação e as
consequências do mundo devem se refletir em como o combate, a progressão e a sobrevivência são
vivenciados pelo jogador.

**Independent Test**: Pode ser testado realizando uma escolha de impacto (por exemplo, remover
o suprimento de comida de uma vila ou decidir não salvar um NPC) e verificando, após um período
de tempo de jogo definido, que a reputação do jogador junto àquela comunidade muda e que o
estado da vila (população, disponibilidade de recursos, comportamento dos NPCs) reflete a
consequência, sem depender de as outras histórias estarem presentes na mesma sessão de teste.

**Acceptance Scenarios**:

1. **Given** o jogador decide salvar um NPC em perigo, **When** a ação é concluída com sucesso,
   **Then** a reputação do jogador junto à comunidade daquele NPC aumenta e o NPC permanece
   disponível para interações futuras (diálogo, missões, comércio).
2. **Given** o jogador decide não salvar um NPC em perigo (ou causar seu dano), **When** essa
   escolha é registrada, **Then** a reputação do jogador junto à comunidade correspondente diminui
   e o desenrolar da história local reflete essa perda (por exemplo, NPCs relacionados reagem
   negativamente ou missões associadas àquele NPC deixam de estar disponíveis).
3. **Given** o jogador remove um recurso essencial (por exemplo, o suprimento de comida) de uma
   vila sem repô-lo, **When** o tempo de jogo avança além do limiar de sustentação da vila,
   **Then** o número de NPCs da vila diminui de forma visível (por fome) e os preços/disponibi-
   lidade de bens no comércio local pioram, refletindo o colapso econômico parcial ou total.
4. **Given** o jogador transporta um recurso escasso de um local com excedente para um local em
   carência, **When** a entrega é concluída, **Then** o estado econômico do local que recebeu o
   recurso melhora de forma mensurável (por exemplo, NPCs deixam de estar em risco de fome,
   preços se normalizam) e a reputação do jogador nessa comunidade aumenta.
5. **Given** a reputação do jogador com uma facção/comunidade está muito baixa, **When** o
   jogador interage com NPCs dessa comunidade, **Then** o jogo reflete essa hostilidade (preços
   piores, recusa de missões, diálogos hostis ou até agressão).

---

### Edge Cases

- Uma vila que perde 100% de sua população por fome fica permanentemente inativa como local
  funcional (sem comércio, missões ou NPCs) pelo resto da campanha; repor os recursos essenciais
  depois desse ponto não reverte a perda populacional já ocorrida (apenas evita perdas futuras em
  outras vilas ainda ativas).
- A reputação do jogador com uma comunidade/facção é totalmente independente das demais: salvar
  um NPC de uma comunidade não produz efeito automático (positivo ou negativo) sobre a reputação
  com nenhuma outra comunidade, mesmo que sejam rivais entre si.
- Quando fome e sanidade atingem níveis críticos simultaneamente, as penalidades de ambos os
  sistemas são aplicadas de forma cumulativa (somadas), sem teto especial nem prioridade de um
  sistema sobre o outro.
- Ferir ou matar um NPC com reputação positiva durante um encontro em que ele foi forçado a lutar
  contra o jogador é tratado como uma escolha de impacto negativa equivalente a uma escolha
  deliberada de não salvar/prejudicar esse NPC, aplicando a mesma penalidade de reputação.
- O que acontece se o jogador tentar iniciar um combate enquanto já está em outro combate ativo?
- O que acontece se o jogador tentar investir um ponto de habilidade sem ter pontos disponíveis,
  ou tentar acessar um nó híbrido sem atender aos pré-requisitos de ambas as trilhas?
- Como o jogo lida com o personagem entrando em combate já com fome ou sanidade crítica (além das
  penalidades contínuas de FR-009/FR-011/FR-021, que permanecem ativas durante o combate)?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE fornecer um modo de combate por turnos em uma grade sobre visão
  isométrica, com ordem de iniciativa calculada por combatente no início de cada encontro.
- **FR-002**: O sistema DEVE conceder a cada personagem, em seu turno, recursos de ação
  distintos (movimento, ação principal, ação bônus) que são consumidos ao serem usados e
  restaurados no início do próximo turno do personagem.
- **FR-003**: O sistema DEVE permitir que o jogador encerre um combate através de vitória
  (derrota de todos os inimigos), fuga bem-sucedida, ou derrota do próprio personagem, cada um
  levando a uma consequência de jogo diferente e claramente comunicada.
- **FR-004**: O sistema DEVE fornecer uma árvore de habilidades com pelo menos duas trilhas
  temáticas — combatente (aspectos físicos) e arcanista (aspectos místicos/intelectuais) — sem
  atribuir ao jogador uma classe fixa ou predefinida.
- **FR-005**: O sistema DEVE permitir que o jogador invista pontos de habilidade livremente
  entre as trilhas disponíveis, incluindo combinações híbridas, de forma que builds
  especializadas e builds generalistas sejam ambas viáveis.
- **FR-006**: O sistema DEVE impedir a aquisição duplicada de uma mesma habilidade e DEVE
  comunicar claramente ao jogador quais habilidades já foram adquiridas.
- **FR-007**: O sistema DEVE refletir as habilidades adquiridas pelo jogador como capacidades
  utilizáveis em combate e/ou exploração (por exemplo, novas ações, bônus passivos, ou
  interações especiais com o mundo).
- **FR-008**: O sistema DEVE rastrear um indicador de fome por personagem, que aumenta com o
  tempo/atividade e é reduzido pelo consumo de alimento.
- **FR-009**: O sistema DEVE aplicar penalidades progressivas às capacidades do personagem
  quando o indicador de fome atinge limiares críticos, e remover essas penalidades quando a
  fome é satisfeita.
- **FR-010**: O sistema DEVE rastrear um indicador de sanidade por personagem, que é reduzido
  por eventos perturbadores definidos (combates extremos, ambientes sobrenaturais, isolamento) e
  restaurado por ações de recuperação (descanso, itens, ambientes seguros).
- **FR-011**: O sistema DEVE aplicar efeitos negativos definidos (por exemplo, penalidades em
  testes mentais/mágicos ou alucinações) quando o indicador de sanidade atinge limiares críticos.
- **FR-012**: O sistema DEVE rastrear a reputação do jogador separadamente por comunidade/facção
  local, permitindo que ela aumente ou diminua em resposta a escolhas específicas do jogador.
- **FR-013**: O sistema DEVE registrar escolhas de impacto do jogador — incluindo salvar ou não
  um NPC e mover recursos entre locais — e propagar consequências observáveis no estado do
  mundo (disponibilidade de NPCs, comércio, disponibilidade de missões).
- **FR-014**: O sistema DEVE simular a necessidade de recursos essenciais (como alimento) em
  cada vila/assentamento, de forma que a ausência prolongada desses recursos reduza a população
  de NPCs daquele local e afete negativamente sua economia local (preços, disponibilidade de
  bens).
- **FR-015**: O sistema DEVE permitir que o jogador transporte recursos entre locais e DEVE
  refletir a chegada desses recursos como uma melhora mensurável no estado econômico e/ou
  populacional do local receptor.
- **FR-016**: O sistema DEVE ajustar o comportamento e as opções de interação dos NPCs (preços,
  disponibilidade de missões, tom de diálogo, hostilidade) com base na reputação do jogador junto
  à comunidade/facção daquele NPC.
- **FR-017**: O sistema DEVE persistir o progresso do jogador — incluindo habilidades adquiridas,
  indicadores de sobrevivência, reputação e estado do mundo (população e recursos de cada
  vila) — entre sessões de jogo.
- **FR-018**: O sistema DEVE permitir que o jogador redistribua (respec) pontos de habilidade já
  investidos livremente e sem custo, a qualquer momento, desfazendo a aquisição dos nós
  selecionados e devolvendo os pontos como disponíveis para reinvestimento.
- **FR-019**: O sistema DEVE tratar a perda de 100% da população de uma vila por fome como um
  estado terminal permanente — a vila deixa de oferecer comércio, missões ou NPCs pelo resto da
  campanha, mesmo que recursos essenciais sejam repostos posteriormente.
- **FR-020**: O sistema DEVE manter a reputação do jogador com cada comunidade/facção totalmente
  independente das demais, sem propagar automaticamente ganhos ou perdas de reputação de uma
  comunidade para outra (incluindo comunidades rivais entre si).
- **FR-021**: O sistema DEVE aplicar cumulativamente as penalidades de fome e de sanidade quando
  ambos os indicadores estiverem em nível crítico ao mesmo tempo, sem impor um teto combinado
  nem priorizar um sistema sobre o outro.
- **FR-022**: O sistema DEVE aplicar a mesma penalidade de reputação de uma escolha deliberada de
  não salvar/prejudicar um NPC também nos casos em que um NPC com reputação positiva é ferido ou
  morto em um combate no qual foi forçado a lutar contra o jogador.

### Key Entities

- **Personagem (Jogador)**: Representa o avatar controlado pelo jogador; possui atributos de
  combate, pontos de habilidade disponíveis/investidos (redistribuíveis livremente via respec,
  FR-018), indicadores de fome e sanidade, e um inventário de recursos/itens.
- **Nó de Habilidade**: Unidade individual da árvore de habilidades; pertence a uma trilha
  (combatente, arcanista, ou híbrida), possui pré-requisitos e concede uma capacidade específica
  ao personagem quando adquirido. A aquisição pode ser desfeita via respec (FR-018).
- **Encontro de Combate**: Instância de combate por turnos; agrega os combatentes participantes
  (personagem, aliados, inimigos), o mapa em grade isométrica e o estado de iniciativa/turnos.
- **NPC**: Personagem não-jogável que pertence a uma comunidade/facção, possui um estado de vida
  (vivo, morto, resgatado, em risco) e reage à reputação do jogador com essa comunidade. A morte
  de um NPC (inclusive em combate forçado, FR-022) é permanente.
- **Comunidade/Facção**: Agrupamento de NPCs (por exemplo, uma vila) que possui um nível de
  reputação em relação ao jogador — totalmente independente da reputação com qualquer outra
  comunidade (FR-020) —, um estoque de recursos essenciais e um estado econômico derivado desse
  estoque. Uma comunidade que perde 100% de sua população entra em um estado terminal permanente
  de inatividade (FR-019).
- **Recurso**: Bem essencial ou comerciável (por exemplo, alimento) que pode ser produzido,
  consumido, armazenado por uma comunidade, ou transportado pelo jogador entre locais.
- **Escolha de Impacto**: Registro de uma decisão relevante do jogador (salvar/não salvar um
  NPC, mover um recurso) usada para determinar mudanças de reputação e consequências no mundo.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Jogadores conseguem concluir um encontro de combate típico (2-4 inimigos) usando
  apenas decisões em turnos, sem travamentos ou estados inconsistentes, em 100% dos testes de
  aceitação de combate.
- **SC-002**: Jogadores conseguem criar pelo menos três combinações de build claramente
  diferentes (por exemplo, puramente combatente, puramente arcanista, e uma híbrida) que
  resultam em capacidades de combate mensuravelmente distintas entre si.
- **SC-003**: Negligenciar completamente a alimentação do personagem por um período de jogo
  definido resulta, em 100% dos casos, em penalidades de desempenho perceptíveis e
  consistentes, revertidas após a necessidade ser suprida.
- **SC-004**: Uma escolha de impacto (salvar/não salvar um NPC, mover um recurso) produz uma
  mudança observável de reputação e/ou estado do mundo em até um período de jogo definido em
  100% dos casos testados.
- **SC-005**: Remover o suprimento essencial de uma vila sem repô-lo resulta em declínio
  mensurável de sua população e economia dentro do período de simulação definido, e repor o
  recurso interrompe declínios futuros (a perda populacional já ocorrida não é revertida; uma
  vila que chega a 0% de população permanece permanentemente inativa, conforme FR-019).
- **SC-006**: Jogadores relatam, em testes de validação de experiência, que suas escolhas de
  personalização de personagem e suas decisões morais/logísticas "importam" e alteram
  visivelmente sua experiência de jogo (meta qualitativa a ser validada por playtesting).

## Assumptions

- Trata-se de uma experiência single-player; suporte multiplayer/cooperativo está fora do escopo
  desta especificação.
- O escopo inicial (MVP) cobre um recorte do mundo (por exemplo, uma região com um pequeno
  número de vilas interligadas) suficiente para demonstrar a simulação de reputação/economia,
  não o mundo sandbox completo definitivo.
- "Período de jogo definido" (usado nos critérios de sucesso e casos de aceitação) refere-se a
  uma janela de tempo simulado dentro do jogo (por exemplo, dias/ciclos in-game), cujo valor
  exato será refinado durante o planejamento técnico.
- A simulação econômica das vilas é simplificada (estoque de recursos essenciais e população)
  e não pretende ser uma simulação econômica de mercado completa.
- Persistência de progresso assume salvamento local (não é necessário sincronização em nuvem
  nesta fase).
- A engine, linguagem e demais decisões técnicas de implementação serão definidas na fase de
  planejamento (`/speckit-plan`), respeitando a stack a ser registrada na constituição do
  projeto.
