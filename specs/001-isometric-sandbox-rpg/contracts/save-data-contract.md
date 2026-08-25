# Contrato: Save Data (persistência local)

Contrato do arquivo de save local (JSON, gravado em `Application.persistentDataPath` — ver
[research.md](../research.md#decision-persistência-saveload)). Define o que DEVE ser persistido
para que uma sessão possa ser restaurada de forma fiel (FR-017).

## Estrutura conceitual

```text
SaveData
├── saveVersion            # inteiro, para permitir migração de saves entre versões do jogo
├── character
│   ├── attributes
│   ├── currentHitPoints / maxHitPoints
│   ├── hunger
│   ├── sanity
│   ├── skillPoints (disponíveis)
│   ├── acquiredSkillNodeIds[]
│   └── inventory[] (resourceId, quantity)
├── reputationByCommunity[] # (communityId, reputationValue)
├── communities[]
│   ├── communityId
│   ├── essentialResourceStock[] (resourceId, quantity)
│   └── populationNpcIds[]
├── npcs[]
│   ├── npcId
│   └── lifeState
├── impactfulChoicesLog[]  # histórico mínimo necessário para auditoria/consequências futuras
└── worldSimulatedTime      # timestamp in-game usado pela simulação de economia/vilas
```

## Regras do contrato

1. **Nunca** persistir definições de conteúdo (texto de habilidades, stats base de NPCs,
   layout de grid de combate) — apenas *estado* e referências por `id` a conteúdo definido em
   `ScriptableObject`s. Isso mantém saves pequenos e resistentes a rebalanceamentos de conteúdo.
2. Todo campo que referencia conteúdo (`acquiredSkillNodeIds`, `resourceId`, `communityId`,
   `npcId`) DEVE ser resolvido contra os assets de conteúdo carregados; uma referência não
   resolvida é tratada como erro de carregamento de save, não como estado padrão silencioso.
3. `saveVersion` DEVE ser incrementado sempre que a estrutura do save mudar de forma
   incompatível; o carregador de save DEVE rejeitar ou migrar explicitamente versões
   desconhecidas — nunca assumir compatibilidade silenciosa.
4. O ciclo salvar → carregar DEVE ser idempotente: carregar um save imediatamente após salvá-lo
   deve produzir um estado de jogo observacionalmente idêntico (mesma vida, fome, sanidade,
   habilidades adquiridas, reputações e estado de comunidades/NPCs).

## Consumidores deste contrato

- Sistema de save/load (serialização/desserialização).
- Testes de PlayMode que validam o ciclo completo salvar → recarregar → verificar estado
  (exigido pelo Princípio III da constituição — cobertura de testes para save/load).
