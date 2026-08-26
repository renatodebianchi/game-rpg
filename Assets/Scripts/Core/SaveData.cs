using System;
using System.Collections.Generic;
using GameRpg.Characters;
using UnityEngine;

namespace GameRpg.Core
{
    // NOTE: All types below use JsonUtility-friendly shapes (public fields, no
    // Dictionary<,>) per contracts/save-data-contract.md. Only *state* is
    // persisted here — never content definitions (skill descriptions, NPC stats,
    // grid layouts), which are resolved from ScriptableObject assets by id.

    [Serializable]
    public struct InventoryEntry
    {
        public string resourceId;
        public int quantity;
    }

    [Serializable]
    public struct ReputationEntry
    {
        public string communityId;
        public int reputationValue;
    }

    [Serializable]
    public class CharacterSaveData
    {
        public int strength;
        public int dexterity;
        public int intellect;
        public int willpower;
        public int currentHitPoints;
        public int maxHitPoints;
        public float hunger;
        public float sanity;
        public int availableSkillPoints;
        public List<string> acquiredSkillNodeIds = new List<string>();
        public List<InventoryEntry> inventory = new List<InventoryEntry>();
        public BodyType bodyType;
        public SkinTone skinTone;
        public HairStyle hairStyle;
        public Color hairColor;
    }

    [Serializable]
    public class CommunitySaveData
    {
        public string communityId;
        public List<InventoryEntry> essentialResourceStock = new List<InventoryEntry>();
        public List<string> populationNpcIds = new List<string>();
        public bool isPermanentlyInactive;
    }

    [Serializable]
    public class NpcSaveData
    {
        public string npcId;
        public string lifeState;
    }

    [Serializable]
    public class ImpactfulChoiceSaveEntry
    {
        public string choiceId;
        public string type;
        public string targetCommunityId;
        public string relatedNpcId;
        public string relatedResourceId;
        public int quantity;
        public double timestampSimulatedSeconds;
    }

    /// <summary>
    /// Root save document. Bump CurrentSaveVersion and add explicit migration
    /// logic in SaveSystem whenever this shape changes incompatibly
    /// (contracts/save-data-contract.md, rule 3).
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentSaveVersion = 1;

        public int saveVersion = CurrentSaveVersion;
        public CharacterSaveData character = new CharacterSaveData();
        public List<ReputationEntry> reputationByCommunity = new List<ReputationEntry>();
        public List<CommunitySaveData> communities = new List<CommunitySaveData>();
        public List<NpcSaveData> npcs = new List<NpcSaveData>();
        public List<ImpactfulChoiceSaveEntry> impactfulChoicesLog = new List<ImpactfulChoiceSaveEntry>();
        public double worldSimulatedTimeSeconds;
    }
}
