# Phase 0 Research: Criação de Personagem (Atributos, Aparência e Equipamento Inicial)

**Feature**: [spec.md](./spec.md) | **Date**: 2026-08-26

Este documento resolve o Technical Context de [plan.md](./plan.md). A stack técnica do projeto
já foi decidida e validada na feature `001-isometric-sandbox-rpg` (ver
`specs/001-isometric-sandbox-rpg/research.md`); esta feature reutiliza essa stack sem alterações
e resolve apenas as decisões específicas de criação de personagem.

## Decision: Reutilizar a stack técnica da feature 001 sem alterações

- **Decision**: Unity 6000.5.9f1 (versão já instalada e em uso no projeto), C#, URP, UGUI,
  Unity Test Framework, persistência local em JSON — idênticos à feature 001.
- **Rationale**: É o mesmo projeto Unity; introduzir qualquer variação de stack para uma única
  feature violaria o Princípio V (Simplicidade) sem nenhum benefício concreto.
- **Alternatives considered**: N/A — não há motivo para reavaliar a stack.

## Decision: Tela de criação de personagem construída em runtime via UGUI

- **Decision**: A UI de criação de personagem é construída via código em uma `MonoBehaviour`
  (`CharacterCreationUI`), no mesmo padrão já validado pelos controladores de demo da feature
  001 (`CombatDemoController`, `SkillTreeDemoController`, etc.): Canvas, botões e textos criados
  em runtime, sem depender de hierarquia de cena desenhada manualmente no Editor.
- **Rationale**: Esse padrão já foi comprovado neste projeto — é confiável de gerar/validar via
  linha de comando (sem precisar de um humano desenhando a UI no Editor) e mantém consistência
  arquitetural com o restante do código de UI já existente.
- **Alternatives considered**: Hierarquia de UI desenhada manualmente no Editor Unity —
  rejeitada por exigir uma sessão interativa do Editor para autoria/validação, o que não é
  garantido neste fluxo de desenvolvimento (ver notas operacionais da feature 001).

## Decision: Reutilizar `Character.Inventory` para o kit de equipamento inicial

- **Decision**: O "kit de equipamento inicial" (FR-005) é representado como um conjunto de
  entradas `ResourceDefinition` + quantidade, adicionadas ao `Character.Inventory` já existente
  (feature 001) ao finalizar a criação — não é introduzido um sistema de slots de equipamento
  (arma equipada, armadura equipada, etc.).
- **Rationale**: A spec não pede um sistema de equipar/desequipar itens em slots — apenas que o
  personagem "receba" um kit inicial. O `Inventory` já existente (`Dictionary<string,int>`,
  `Character.cs`) é suficiente para representar isso sem introduzir uma abstração nova
  (Princípio V). Um sistema de slots de equipamento fica como evolução futura, fora de escopo
  aqui.
- **Alternatives considered**: Novo sistema `EquippedGear` com slots por tipo de item —
  rejeitado por escopo: nada na spec exige efeitos mecânicos de "equipar" versus apenas
  "possuir" o item no inventário nesta primeira versão.

## Decision: Conteúdo do kit de equipamento como `ScriptableObject` por orientação

- **Decision**: Um novo tipo de conteúdo `EquipmentKitDefinition` (ScriptableObject), com um
  campo de orientação (`Combatant` ou `Arcanist`) e uma lista de entradas
  (`ResourceDefinition`, quantidade). Dois assets são semeados inicialmente (um kit por
  orientação), seguindo o mesmo padrão de autoria de conteúdo já usado para `SkillNodeDefinition`
  e `CommunityDefinition` (feature 001).
- **Rationale**: Mantém consistência com o Princípio II (arquitetura orientada a dados): equipe
  de design pode editar/balancear os kits iniciais sem tocar em código.
- **Alternatives considered**: Kits hard-coded em C# — rejeitado por violar o Princípio II.

## Decision: Tabela de custo do Point Buy como constantes estáticas em código

- **Decision**: A curva de custo do Point Buy (8→0, 9→1, 10→2, 11→3, 12→4, 13→5, 14→7, 15→9) é
  implementada como uma tabela estática somente-leitura em C#
  (`PointBuyCostTable`), não como conteúdo editável em asset.
- **Rationale**: É uma regra de balanceamento central e rara de mudar (replica uma tabela oficial
  de D&D 5e, apenas com orçamento total reescalado — ver Assumptions em `spec.md`); mantê-la em
  código simplifica a validação e os testes automatizados exigidos pelo Princípio III, sem
  necessidade de editá-la via Editor no dia a dia.
- **Alternatives considered**: Expor a tabela como asset `ScriptableObject` editável — avaliado e
  rejeitado por ora (Princípio V): não há necessidade concreta hoje de balancear essa curva sem
  tocar em código; pode ser promovida a asset depois, se necessário.

## Decision: Características visuais como dados puros (sem pipeline de arte)

- **Decision**: `VisualCharacteristics` é uma struct com três enums pequenos —
  `BodyType` (2 valores), `SkinTone` (3 valores), `HairStyle` (3 valores) — mais uma cor de
  cabelo (`Color`, valor livre). Não há troca de malha/textura associada nesta versão; os
  valores são persistidos e expostos para uma futura camada de renderização de personagem.
- **Rationale**: O projeto não tem hoje nenhum pipeline de customização visual 3D (modelos,
  texturas por parte do corpo). Modelar isso como dados simples atende ao pedido da spec
  ("características visuais básicas") sem introduzir trabalho de arte/rigging fora do escopo
  desta feature (Princípio V). `SC-004` (3 aparências distintas) é satisfeito pela combinatória
  desses enums (2×3×3 = 18 combinações possíveis).
- **Alternatives considered**: Integração com um sistema de customização de avatar em runtime —
  rejeitada por escopo; não há personagem 3D visual customizável no projeto ainda (os
  combatentes nas demos da feature 001 são cápsulas coloridas simples).

## Resumo das resoluções de Technical Context

| Campo | Resolução |
|---|---|
| Language/Version | C# (Unity 6000.5.9f1 — mesma versão da feature 001) |
| Primary Dependencies | Unity UGUI (mesmas dependências da feature 001; nenhuma nova) |
| Storage | Arquivos locais em JSON via `Application.persistentDataPath` (estende `SaveData` da feature 001) |
| Testing | Unity Test Framework — EditMode (validação de Point Buy, atribuição de kit, finalização) |
| Target Platform | PC desktop (mesmo alvo da feature 001) |
| Project Type | Extensão do mesmo projeto Unity single-player da feature 001 |
| Performance Goals | Mesmos da feature 001 (60 fps, <100ms de input) — tela de menu, sem risco de regressão |
| Constraints | Offline-capable; sem sistema de slots de equipamento (reutiliza Inventory) |
| Scale/Scope | 4 atributos, 2 orientações (kits fixos), ~18 combinações visuais básicas |
