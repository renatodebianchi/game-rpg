# Contrato: Transição de Criação de Personagem para Exploração

Contrato de como o personagem finalizado chega à cena de Exploração (FR-003, FR-004). Ver
[data-model.md](../data-model.md#pendingplayercharacter-transferência-entre-cenas).

## Pré-condição

- `CharacterCreationProfile.Finalize()` (feature 002) foi chamado com sucesso — ou seja,
  `AttributeAllocationState.PointsRemaining == 0` e uma `CharacterOrientation` foi escolhida
  (contrato de finalização da feature 002).

## Efeito ao finalizar e prosseguir

1. O `Character` finalizado é atribuído a `PendingPlayerCharacter.Character`.
2. A cena `Exploration` é carregada (`SceneManager.LoadScene`).
3. No `Start()` da cena de Exploração, `ExplorationCharacterController` DEVE ler
   `PendingPlayerCharacter.Character`:
   - Se não for nulo, DEVE usá-lo como o personagem do jogador, e DEVE limpar
     `PendingPlayerCharacter.Character` em seguida (para que uma futura visita direta à cena
     não reutilize um personagem de uma sessão anterior por engano).
   - Se for nulo (cena aberta diretamente, sem passar pela criação), DEVE criar um `Character`
     com atributos e `VisualCharacteristics` padrão (FR-004), do mesmo modo que as demais
     demos das features 001/002 se auto-inicializam quando abertas isoladamente.

## Pós-condição

- O personagem exibido na Exploração tem exatamente os mesmos atributos, inventário
  (equipamento) e `VisualCharacteristics` que tinha ao final da criação — nenhum desses valores
  é recalculado ou resetado pela transição de cena.

## Consumidores deste contrato

- `CharacterCreationUI` (feature 002, botão "Finalizar" estendido por esta feature).
- `ExplorationCharacterController` (novo, desta feature).
- Testes de integração (PlayMode) que validam o fluxo completo: finalizar criação → carregar
  Exploração → verificar que o personagem exibido corresponde ao criado.
