using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameRpg.Characters;
using GameRpg.World;
using UnityEngine;

namespace GameRpg.Core
{
    /// <summary>
    /// Serializes/deserializes SaveData to local JSON, per
    /// contracts/save-data-contract.md. Full integration wiring every gameplay
    /// system into SaveData happens in the Polish phase (tasks.md T060); this
    /// class provides the serialization skeleton consumed by that integration.
    /// </summary>
    public class SaveSystem
    {
        private const string SaveFileName = "save.json";

        private readonly string _saveDirectory;

        public SaveSystem(string saveDirectory = null)
        {
            _saveDirectory = saveDirectory ?? Application.persistentDataPath;
        }

        public string SaveFilePath => Path.Combine(_saveDirectory, SaveFileName);

        public string Serialize(SaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return JsonUtility.ToJson(data, prettyPrint: true);
        }

        public SaveData Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("Save JSON cannot be null or empty.", nameof(json));
            }

            var data = JsonUtility.FromJson<SaveData>(json);

            if (data.saveVersion != SaveData.CurrentSaveVersion)
            {
                throw new NotSupportedException(
                    $"Save version {data.saveVersion} is not supported by this build " +
                    $"(expected {SaveData.CurrentSaveVersion}). Migration logic must be added " +
                    "explicitly before this version can be loaded (save-data-contract.md, rule 3).");
            }

            return data;
        }

        public void SaveToDisk(SaveData data)
        {
            Directory.CreateDirectory(_saveDirectory);
            File.WriteAllText(SaveFilePath, Serialize(data));
        }

        public bool TryLoadFromDisk(out SaveData data)
        {
            if (!File.Exists(SaveFilePath))
            {
                data = null;
                return false;
            }

            data = Deserialize(File.ReadAllText(SaveFilePath));
            return true;
        }

        /// <summary>
        /// Builds a SaveData snapshot from the live game state (T060): Character
        /// (including respec-adjusted skills, hunger/sanity, inventory), every
        /// Community's reputation/resources/population/isPermanentlyInactive, and
        /// the world clock.
        /// </summary>
        public SaveData CaptureGameState(Character character, IEnumerable<Community> communities, WorldClock worldClock)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));
            if (communities == null) throw new ArgumentNullException(nameof(communities));
            if (worldClock == null) throw new ArgumentNullException(nameof(worldClock));

            var data = new SaveData
            {
                worldSimulatedTimeSeconds = worldClock.ElapsedSimulatedTime.TotalSeconds,
                character = new CharacterSaveData
                {
                    strength = character.Attributes.Strength,
                    dexterity = character.Attributes.Dexterity,
                    intellect = character.Attributes.Intellect,
                    willpower = character.Attributes.Willpower,
                    currentHitPoints = character.CurrentHitPoints,
                    maxHitPoints = character.MaxHitPoints,
                    hunger = character.Hunger,
                    sanity = character.Sanity,
                    availableSkillPoints = character.AvailableSkillPoints,
                    acquiredSkillNodeIds = character.AcquiredSkillNodeIds.ToList(),
                    inventory = character.Inventory.AsReadOnly()
                        .Select(kv => new InventoryEntry { resourceId = kv.Key, quantity = kv.Value })
                        .ToList(),
                    bodyType = character.Visuals.BodyType,
                    skinTone = character.Visuals.SkinTone,
                    hairStyle = character.Visuals.HairStyle,
                    hairColor = character.Visuals.HairColor,
                },
            };

            foreach (var community in communities)
            {
                data.reputationByCommunity.Add(new ReputationEntry
                {
                    communityId = community.CommunityId,
                    reputationValue = community.ReputationWithPlayer,
                });

                data.communities.Add(new CommunitySaveData
                {
                    communityId = community.CommunityId,
                    essentialResourceStock = community.EnumerateResourceStock()
                        .Select(kv => new InventoryEntry { resourceId = kv.Key, quantity = Mathf.RoundToInt(kv.Value) })
                        .ToList(),
                    populationNpcIds = community.PopulationNpcIds.ToList(),
                    isPermanentlyInactive = community.IsPermanentlyInactive,
                });
            }

            return data;
        }

        /// <summary>
        /// Restores live game objects from a SaveData snapshot (T060). Communities
        /// not present in <paramref name="communitiesById"/> are skipped rather than
        /// created — content (which communities exist) is resolved from
        /// ScriptableObject assets, never invented from save data.
        /// </summary>
        public void ApplyGameState(
            SaveData data,
            Character character,
            IReadOnlyDictionary<string, Community> communitiesById,
            WorldClock worldClock)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (character == null) throw new ArgumentNullException(nameof(character));
            if (communitiesById == null) throw new ArgumentNullException(nameof(communitiesById));
            if (worldClock == null) throw new ArgumentNullException(nameof(worldClock));

            worldClock.RestoreFromSave(TimeSpan.FromSeconds(data.worldSimulatedTimeSeconds));

            character.Attributes = new CharacterAttributes(
                data.character.strength, data.character.dexterity, data.character.intellect, data.character.willpower);
            character.RestoreFromSave(
                data.character.currentHitPoints,
                data.character.availableSkillPoints,
                data.character.acquiredSkillNodeIds,
                data.character.hunger,
                data.character.sanity,
                data.character.inventory.Select(e => new KeyValuePair<string, int>(e.resourceId, e.quantity)),
                new VisualCharacteristics
                {
                    BodyType = data.character.bodyType,
                    SkinTone = data.character.skinTone,
                    HairStyle = data.character.hairStyle,
                    HairColor = data.character.hairColor,
                });

            foreach (var communitySave in data.communities)
            {
                if (!communitiesById.TryGetValue(communitySave.communityId, out var community))
                {
                    continue;
                }

                var reputationEntry = data.reputationByCommunity
                    .FirstOrDefault(r => r.communityId == communitySave.communityId);

                community.RestoreFromSave(
                    communitySave.essentialResourceStock.Select(e => new KeyValuePair<string, float>(e.resourceId, e.quantity)),
                    reputationEntry.reputationValue,
                    communitySave.isPermanentlyInactive,
                    communitySave.populationNpcIds);
            }
        }
    }
}
