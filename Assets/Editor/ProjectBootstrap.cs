using System.IO;
using GameRpg.Demo;
using GameRpg.NPCs;
using GameRpg.Skills;
using GameRpg.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameRpg.Editor
{
    /// <summary>
    /// One-off setup for Editor-only assets that are far more reliable to
    /// generate through the Editor API than to hand-author as raw YAML
    /// (tasks.md T003: URP asset + base isometric camera in Exploration.unity).
    /// Run headlessly via:
    ///   Unity -batchmode -quit -projectPath . -executeMethod GameRpg.Editor.ProjectBootstrap.SetupProject
    /// </summary>
    public static class ProjectBootstrap
    {
        private const string UrpAssetPath = "Assets/Settings/GameRpgUrpAsset.asset";
        private const string RendererDataPath = "Assets/Settings/GameRpgUrpRenderer.asset";
        private const string ExplorationScenePath = "Assets/Scenes/Exploration.unity";
        private const string CombatEncounterTestScenePath = "Assets/Scenes/CombatEncounterTest.unity";

        private const string SkillContentDirectory = "Assets/Data/Skills";
        private const string WorldContentDirectory = "Assets/Data/World";

        [MenuItem("GameRpg/Bootstrap/Setup Project (URP + Scenes + Content)")]
        public static void SetupProject()
        {
            CreateUrpAsset();
            CreateExplorationScene();
            CreateCombatEncounterTestScene();
            CreateInitialSkillContent();
            CreateInitialWorldContent();
            WireCombatDemoIntoTestScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Adds a CombatDemoController to CombatEncounterTest.unity so the scene
        /// is playable/visual for manual testing (see quickstart.md's combat
        /// validation section) without any hand-authored scene content.
        /// </summary>
        private static void WireCombatDemoIntoTestScene()
        {
            var scene = EditorSceneManager.OpenScene(CombatEncounterTestScenePath, OpenSceneMode.Single);

            if (GameObject.Find("CombatDemoController") == null)
            {
                var demoGameObject = new GameObject("CombatDemoController");
                demoGameObject.AddComponent<CombatDemoController>();
            }

            EditorSceneManager.SaveScene(scene);
        }

        private static void CreateInitialWorldContent()
        {
            // T059: MVP region content — a couple of villages, a handful of NPCs,
            // and the essential resource (food) their survival simulation depends
            // on (contracts/village-economy-simulation-contract.md).
            if (!AssetDatabase.IsValidFolder(WorldContentDirectory))
            {
                Directory.CreateDirectory(WorldContentDirectory);
                AssetDatabase.Refresh();
            }

            if (AssetDatabase.FindAssets("t:CommunityDefinition", new[] { WorldContentDirectory }).Length > 0)
            {
                return; // Already seeded.
            }

            var food = ResourceDefinition.CreateForTesting("food", "Alimento", isEssential: true);
            AssetDatabase.CreateAsset(food, $"{WorldContentDirectory}/food.asset");

            CreateVillageWithNpcs("village_oakhollow", "Oakhollow", population: 6, startingFoodStock: 60,
                npcIds: new[] { "npc_oakhollow_elder", "npc_oakhollow_merchant", "npc_oakhollow_guard" });

            CreateVillageWithNpcs("village_riverbend", "Riverbend", population: 5, startingFoodStock: 45,
                npcIds: new[] { "npc_riverbend_healer", "npc_riverbend_farmer" });
        }

        private static void CreateVillageWithNpcs(
            string communityId, string displayName, int population, int startingFoodStock, string[] npcIds)
        {
            var community = CommunityDefinition.CreateForTesting(communityId, displayName, population, startingFoodStock);
            AssetDatabase.CreateAsset(community, $"{WorldContentDirectory}/{communityId}.asset");

            foreach (var npcId in npcIds)
            {
                var npc = NpcDefinition.CreateForTesting(npcId, npcId, communityId);
                AssetDatabase.CreateAsset(npc, $"{WorldContentDirectory}/{npcId}.asset");
            }
        }

        private static void CreateInitialSkillContent()
        {
            // T037: a small starter set spanning Combatant, Arcanist, and one
            // Hybrid node, demonstrating the prerequisite/track rules from
            // contracts/skill-node-data-contract.md. Designers extend this set
            // directly in the Editor afterwards; this method only seeds it.
            if (!AssetDatabase.IsValidFolder(SkillContentDirectory))
            {
                Directory.CreateDirectory(SkillContentDirectory);
                AssetDatabase.Refresh();
            }

            if (AssetDatabase.FindAssets("t:SkillNodeDefinition", new[] { SkillContentDirectory }).Length > 0)
            {
                return; // Already seeded.
            }

            var powerStrike = CreateSkillNodeAsset(
                "combat_power_strike", SkillTrack.Combatant, cost: 1,
                grantedCapabilityId: "capability.combat.power_strike");
            var cleave = CreateSkillNodeAsset(
                "combat_cleave", SkillTrack.Combatant, cost: 2,
                grantedCapabilityId: "capability.combat.cleave", powerStrike);

            var arcaneBolt = CreateSkillNodeAsset(
                "arcane_bolt", SkillTrack.Arcanist, cost: 1,
                grantedCapabilityId: "capability.arcanist.arcane_bolt");
            var arcaneShield = CreateSkillNodeAsset(
                "arcane_shield", SkillTrack.Arcanist, cost: 2,
                grantedCapabilityId: "capability.arcanist.arcane_shield", arcaneBolt);

            CreateSkillNodeAsset(
                "hybrid_spellblade", SkillTrack.Hybrid, cost: 3,
                grantedCapabilityId: "capability.hybrid.spellblade", powerStrike, arcaneBolt);

            // Reference cleave/arcaneShield too, so they are not reported as unused locals.
            _ = cleave;
            _ = arcaneShield;
        }

        private static SkillNodeDefinition CreateSkillNodeAsset(
            string nodeId,
            SkillTrack track,
            int cost,
            string grantedCapabilityId,
            params SkillNodeDefinition[] prerequisites)
        {
            var node = SkillNodeDefinition.CreateForTesting(nodeId, track, prerequisites, cost, grantedCapabilityId);
            AssetDatabase.CreateAsset(node, $"{SkillContentDirectory}/{nodeId}.asset");
            return node;
        }

        private static void CreateUrpAsset()
        {
            var directory = Path.GetDirectoryName(UrpAssetPath);
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererDataPath);
            }

            var existing = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (existing == null)
            {
                var urpAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(urpAsset, UrpAssetPath);
                existing = urpAsset;
            }

            GraphicsSettings.defaultRenderPipeline = existing;
            QualitySettings.renderPipeline = existing;
        }

        private static void CreateExplorationScene()
        {
            if (File.Exists(ExplorationScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGameObject = new GameObject("IsometricExplorationCamera");
            var camera = cameraGameObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            // Classic isometric look: 45 degrees around Y, ~35.264 degrees down (arctan(1/sqrt(2))).
            cameraGameObject.transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
            cameraGameObject.transform.position = new Vector3(10f, 10f, -10f);
            cameraGameObject.tag = "MainCamera";

            var directory = Path.GetDirectoryName(ExplorationScenePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            EditorSceneManager.SaveScene(scene, ExplorationScenePath);
        }

        private static void CreateCombatEncounterTestScene()
        {
            // T028: a bare isometric-camera scene used as the harness for manual
            // combat validation (see quickstart.md). Spawning combatants/UI into
            // it is done by MonoBehaviour bootstrap code, not baked into the
            // scene file, so it stays trivial to keep in sync with CombatEncounter.
            if (File.Exists(CombatEncounterTestScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGameObject = new GameObject("CombatCamera");
            var camera = cameraGameObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            cameraGameObject.transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
            cameraGameObject.transform.position = new Vector3(10f, 10f, -10f);
            cameraGameObject.tag = "MainCamera";

            var directory = Path.GetDirectoryName(CombatEncounterTestScenePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            EditorSceneManager.SaveScene(scene, CombatEncounterTestScenePath);
        }
    }
}
