# Phase 1 Data Model: Assets Visuais do Personagem e da Interface + Exploração com Personagem Criado

**Feature**: [spec.md](./spec.md) | **Research**: [research.md](./research.md)

Modelo conceitual das entidades/estruturas introduzidas por esta feature. Reutiliza
`Characters.Character`, `Characters.VisualCharacteristics` e `Characters.CharacterCreationProfile`
já existentes (feature 002) sem alterá-los estruturalmente.

## CharacterSpriteMapping (dados estáticos)

Tabela estática, análoga em espírito a `PointBuyCostTable` (feature 002), que traduz
`VisualCharacteristics` em uma aparência de sprite (research.md, "Decision: Mapeamento de
características visuais para o sprite").

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `TintBySkinTone` | Cor de tingimento (`Color`) para cada valor de `SkinTone` (Light/Medium/Dark) | 3 entradas fixas, definidas em código |
| `SpriteFrameByBodyType` | Índice/nome do frame do spritesheet do Kenney para cada `BodyType`, quando houver frame distinto disponível | Se não houver frame distinto, usa o frame padrão (fallback documentado em research.md) |
| `HairColorOverride` | A cor livre já existente em `VisualCharacteristics.HairColor` (feature 002) é aplicada como tingimento adicional sobre a região de cabelo do sprite, quando o pacote permitir essa separação; caso não permita, é ignorada nesta versão (melhor esforço, ver spec.md Assumptions) |

## PendingPlayerCharacter (transferência entre cenas)

Objeto simples que carrega o `Character` finalizado da cena de Criação de Personagem para a
cena de Exploração (research.md, "Decision: Transição... via carregamento de cena"). Não é
persistido em disco — vive apenas na memória durante a sessão de jogo, entre o momento da
finalização e o `Start()` da cena de Exploração.

| Campo | Descrição |
|---|---|
| `Character` | Referência ao `Characters.Character` finalizado por `CharacterCreationProfile.Finalize()` |

**Regra**: se a cena de Exploração inicia e nenhum `PendingPlayerCharacter` foi definido (ex.
cena aberta diretamente, sem passar pela criação), a Exploração cria um `Character` com
atributos e `VisualCharacteristics` padrão (FR-004; mesmo padrão de auto-inicialização já usado
pelas demais demos das features 001/002).

## ExplorationCharacterController (comportamento, não dado)

Não é uma entidade de dados, mas o componente responsável por: posicionar o sprite do
personagem na cena, aplicar `CharacterSpriteMapping` para refletir `Character.Visuals`, e ler
input de teclado para mover o personagem (FR-001, FR-002, FR-005). Vive em
`Assets/Scripts/Demo/ExplorationCharacterController.cs`, seguindo o mesmo padrão de MonoBehaviour
runtime-construído das demais demos.

## DemoUiKit (componentes de UI compartilhados)

Não é uma entidade de dados, mas a extração arquitetural descrita em research.md ("Decision:
Extrair os componentes de UI duplicados..."). Consolida os métodos `CreateText`, `CreateButton`
(e variantes) hoje duplicados em cada controller de demo em um único ponto
(`Assets/Scripts/UI/DemoUiKit.cs`), que passa a carregar e aplicar os assets do Kenney UI Pack
(botão, painel, fonte). É o mecanismo que torna FR-007 (propagação automática do novo visual)
verdadeiro.

## Registro de Créditos de Assets (artefato de documentação, não de código)

Não é uma entidade em tempo de execução — é um arquivo de texto do projeto (ex.
`Assets/Art/CREDITS.md`) listando, para cada asset de terceiros introduzido: nome do pacote,
fonte (URL), autor/criador, e licença (FR-008). Verificado manualmente, não por código.

## Diagrama de relações (conceitual)

```
Characters.Character 1---1 Characters.VisualCharacteristics (já existente, feature 002)
CharacterSpriteMapping --resolve(VisualCharacteristics)--> aparência do sprite (tint + frame)
CharacterCreationUI --Finalize() + LoadScene--> PendingPlayerCharacter --lido por--> ExplorationCharacterController
ExplorationCharacterController --usa--> CharacterSpriteMapping
Todas as demos (Combate, Árvore de Habilidades, Sobrevivência, Reputação/Economia,
Criação de Personagem, Exploração) --usam--> DemoUiKit
```
