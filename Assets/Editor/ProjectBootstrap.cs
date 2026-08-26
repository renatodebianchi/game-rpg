using System.IO;
using System.Linq;
using GameRpg.Characters;
using GameRpg.Demo;
using GameRpg.NPCs;
using GameRpg.Skills;
using GameRpg.UI;
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
        private const string SkillTreeDemoScenePath = "Assets/Scenes/SkillTreeDemo.unity";
        private const string SurvivalDemoScenePath = "Assets/Scenes/SurvivalDemo.unity";
        private const string ReputationEconomyDemoScenePath = "Assets/Scenes/ReputationEconomyDemo.unity";
        private const string CharacterCreationDemoScenePath = "Assets/Scenes/CharacterCreationDemo.unity";

        private const string SkillContentDirectory = "Assets/Data/Skills";
        private const string WorldContentDirectory = "Assets/Data/World";
        private const string EquipmentContentDirectory = "Assets/Data/Equipment";

        [MenuItem("GameRpg/Bootstrap/Setup Project (URP + Scenes + Content)")]
        public static void SetupProject()
        {
            CreateUrpAsset();
            CreateExplorationScene();
            CreateCombatEncounterTestScene();
            CreateInitialSkillContent();
            CreateInitialWorldContent();
            CreateInitialEquipmentKitContent();
            WireCombatDemoIntoTestScene();
            WireSkillTreeDemoScene();
            WireSurvivalDemoScene();
            WireReputationEconomyDemoScene();
            WireCharacterCreationDemoScene();
            WireExplorationScene();
            RegisterScenesInBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Adds an ExplorationCharacterController to Exploration.unity (feature 003,
        /// FR-001/FR-002) so the scene shows and moves the player's created
        /// character — or a default one, per FR-004/scene-transition-contract.md —
        /// without any hand-authored scene content.
        /// </summary>
        private static void WireExplorationScene()
        {
            var scene = EditorSceneManager.OpenScene(ExplorationScenePath, OpenSceneMode.Single);

            if (GameObject.Find("ExplorationCharacterController") == null)
            {
                var controllerGameObject = new GameObject("ExplorationCharacterController");
                controllerGameObject.AddComponent<ExplorationCharacterController>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>
        /// SceneManager.LoadScene (used by CharacterCreationUI's "Finalizar" button
        /// to enter Exploration, contracts/scene-transition-contract.md) only finds
        /// scenes registered in Build Settings, even inside the Editor's Play mode.
        /// </summary>
        private static void RegisterScenesInBuildSettings()
        {
            var scenePaths = new[]
            {
                ExplorationScenePath,
                CombatEncounterTestScenePath,
                SkillTreeDemoScenePath,
                SurvivalDemoScenePath,
                ReputationEconomyDemoScenePath,
                CharacterCreationDemoScenePath,
            };

            EditorBuildSettings.scenes = scenePaths
                .Where(File.Exists)
                .Select(path => new EditorBuildSettingsScene(path, enabled: true))
                .ToArray();
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

        /// <summary>
        /// Creates SkillTreeDemo.unity (if missing) with a plain camera and a
        /// SkillTreeDemoController wired with the seeded skill node assets, so
        /// User Story 2 (including respec) is manually testable.
        ///
        /// Loads the node assets fresh, right here, instead of accepting them
        /// as a parameter: carrying ScriptableObject references across the
        /// EditorSceneManager.OpenScene/NewScene calls made by earlier steps in
        /// SetupProject() left them pointing at unloaded objects (every
        /// reference serialized as `{fileID: 0}`, i.e. null, even though the
        /// array itself had the right length) — reloading by path immediately
        /// before use avoids that.
        /// </summary>
        private static void WireSkillTreeDemoScene()
        {
            var scene = CreateUiDemoScene(SkillTreeDemoScenePath, "SkillTreeDemoController");

            var skillNodes = new[]
            {
                "combat_power_strike", "combat_cleave", "arcane_bolt", "arcane_shield", "hybrid_spellblade",
            }
                .Select(nodeId => AssetDatabase.LoadAssetAtPath<SkillNodeDefinition>($"{SkillContentDirectory}/{nodeId}.asset"))
                .Where(node => node != null)
                .ToArray();

            var controllerGameObject = GameObject.Find("SkillTreeDemoController");
            var controller = controllerGameObject.GetComponent<SkillTreeDemoController>();
            if (controller == null)
            {
                controller = controllerGameObject.AddComponent<SkillTreeDemoController>();
            }

            var serializedController = new SerializedObject(controller);
            var nodesProperty = serializedController.FindProperty("allNodes");
            nodesProperty.ClearArray();
            for (var i = 0; i < skillNodes.Length; i++)
            {
                nodesProperty.InsertArrayElementAtIndex(i);
                nodesProperty.GetArrayElementAtIndex(i).objectReferenceValue = skillNodes[i];
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>
        /// Creates SurvivalDemo.unity (if missing) with a plain camera and a
        /// SurvivalDemoController, so User Story 3 (hunger/sanity, including
        /// cumulative combat penalties) is manually testable.
        /// </summary>
        private static void WireSurvivalDemoScene()
        {
            var scene = CreateUiDemoScene(SurvivalDemoScenePath, "SurvivalDemoController");

            var controllerGameObject = GameObject.Find("SurvivalDemoController");
            if (controllerGameObject.GetComponent<SurvivalDemoController>() == null)
            {
                controllerGameObject.AddComponent<SurvivalDemoController>();
            }

            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>
        /// Creates ReputationEconomyDemo.unity (if missing) with a plain camera
        /// and a ReputationEconomyDemoController wired with the seeded village/
        /// NPC assets, so User Story 4 (reputation, village economy/population,
        /// permanent collapse) is manually testable.
        /// </summary>
        private static void WireReputationEconomyDemoScene()
        {
            var scene = CreateUiDemoScene(ReputationEconomyDemoScenePath, "ReputationEconomyDemoController");

            var communityIds = new[] { "village_oakhollow", "village_riverbend" };
            var npcIds = new[]
            {
                "npc_oakhollow_elder", "npc_oakhollow_merchant", "npc_oakhollow_guard",
                "npc_riverbend_healer", "npc_riverbend_farmer",
            };

            var communities = communityIds
                .Select(id => AssetDatabase.LoadAssetAtPath<CommunityDefinition>($"{WorldContentDirectory}/{id}.asset"))
                .Where(c => c != null)
                .ToArray();
            var npcs = npcIds
                .Select(id => AssetDatabase.LoadAssetAtPath<NpcDefinition>($"{WorldContentDirectory}/{id}.asset"))
                .Where(n => n != null)
                .ToArray();

            var controllerGameObject = GameObject.Find("ReputationEconomyDemoController");
            var controller = controllerGameObject.GetComponent<ReputationEconomyDemoController>();
            if (controller == null)
            {
                controller = controllerGameObject.AddComponent<ReputationEconomyDemoController>();
            }

            var serializedController = new SerializedObject(controller);
            AssignObjectArray(serializedController, "communityDefinitions", communities);
            AssignObjectArray(serializedController, "npcDefinitions", npcs);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AssignObjectArray(SerializedObject serializedObject, string propertyName, UnityEngine.Object[] values)
        {
            var property = serializedObject.FindProperty(propertyName);
            property.ClearArray();
            for (var i = 0; i < values.Length; i++)
            {
                property.InsertArrayElementAtIndex(i);
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        /// <summary>
        /// Shared helper: opens (creating if needed) a bare scene with a plain
        /// camera and a named, empty controller GameObject — the pattern used
        /// by every UI-only demo scene (skill tree, survival).
        /// </summary>
        private static UnityEngine.SceneManagement.Scene CreateUiDemoScene(string scenePath, string controllerGameObjectName)
        {
            UnityEngine.SceneManagement.Scene scene;

            if (File.Exists(scenePath))
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                var cameraGameObject = new GameObject("DemoCamera");
                var camera = cameraGameObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 6f;
                cameraGameObject.tag = "MainCamera";

                var directory = Path.GetDirectoryName(scenePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                EditorSceneManager.SaveScene(scene, scenePath);
            }

            if (GameObject.Find(controllerGameObjectName) == null)
            {
                new GameObject(controllerGameObjectName);
            }

            return scene;
        }

        /// <summary>
        /// Creates CharacterCreationDemo.unity (if missing) with a plain camera
        /// and a CharacterCreationUI wired with the seeded equipment kits, so
        /// the character-creation feature (attributes, orientation, appearance)
        /// is manually testable. Loads the kits fresh from their known paths,
        /// same reasoning as WireSkillTreeDemoScene: carrying references across
        /// the earlier OpenScene/NewScene calls in SetupProject() leaves them
        /// stale.
        /// </summary>
        private static void WireCharacterCreationDemoScene()
        {
            var scene = CreateUiDemoScene(CharacterCreationDemoScenePath, "CharacterCreationUI");

            var kits = new[] { "combatant_starter_kit", "arcanist_starter_kit" }
                .Select(id => AssetDatabase.LoadAssetAtPath<EquipmentKitDefinition>($"{EquipmentContentDirectory}/{id}.asset"))
                .Where(kit => kit != null)
                .ToArray();

            var controllerGameObject = GameObject.Find("CharacterCreationUI");
            var controller = controllerGameObject.GetComponent<CharacterCreationUI>();
            if (controller == null)
            {
                controller = controllerGameObject.AddComponent<CharacterCreationUI>();
            }

            var serializedController = new SerializedObject(controller);
            AssignObjectArray(serializedController, "equipmentKits", kits);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CreateInitialEquipmentKitContent()
        {
            // T016: fixed starting equipment kits per CharacterOrientation
            // (contracts/character-creation-finalization-contract.md).
            if (!AssetDatabase.IsValidFolder(EquipmentContentDirectory))
            {
                Directory.CreateDirectory(EquipmentContentDirectory);
                AssetDatabase.Refresh();
            }

            if (File.Exists($"{EquipmentContentDirectory}/combatant_starter_kit.asset"))
            {
                return; // Already seeded.
            }

            var sword = ResourceDefinition.CreateForTesting("sword_basic", "Espada Básica", isEssential: false);
            AssetDatabase.CreateAsset(sword, $"{EquipmentContentDirectory}/sword_basic.asset");
            var leatherArmor = ResourceDefinition.CreateForTesting("leather_armor", "Armadura de Couro", isEssential: false);
            AssetDatabase.CreateAsset(leatherArmor, $"{EquipmentContentDirectory}/leather_armor.asset");

            var staff = ResourceDefinition.CreateForTesting("staff_basic", "Cajado Básico", isEssential: false);
            AssetDatabase.CreateAsset(staff, $"{EquipmentContentDirectory}/staff_basic.asset");
            var spellbook = ResourceDefinition.CreateForTesting("spellbook_basic", "Grimório Básico", isEssential: false);
            AssetDatabase.CreateAsset(spellbook, $"{EquipmentContentDirectory}/spellbook_basic.asset");

            var combatantKit = EquipmentKitDefinition.CreateForTesting(
                CharacterOrientation.Combatant, new[] { (sword, 1), (leatherArmor, 1) });
            AssetDatabase.CreateAsset(combatantKit, $"{EquipmentContentDirectory}/combatant_starter_kit.asset");

            var arcanistKit = EquipmentKitDefinition.CreateForTesting(
                CharacterOrientation.Arcanist, new[] { (staff, 1), (spellbook, 1) });
            AssetDatabase.CreateAsset(arcanistKit, $"{EquipmentContentDirectory}/arcanist_starter_kit.asset");
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

        private static SkillNodeDefinition[] CreateInitialSkillContent()
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

            var knownNodeIds = new[]
            {
                "combat_power_strike", "combat_cleave", "arcane_bolt", "arcane_shield", "hybrid_spellblade",
            };

            if (File.Exists($"{SkillContentDirectory}/{knownNodeIds[0]}.asset"))
            {
                return knownNodeIds
                    .Select(nodeId => AssetDatabase.LoadAssetAtPath<SkillNodeDefinition>($"{SkillContentDirectory}/{nodeId}.asset"))
                    .Where(node => node != null)
                    .ToArray();
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

            var hybridSpellblade = CreateSkillNodeAsset(
                "hybrid_spellblade", SkillTrack.Hybrid, cost: 3,
                grantedCapabilityId: "capability.hybrid.spellblade", powerStrike, arcaneBolt);

            return new[] { powerStrike, cleave, arcaneBolt, arcaneShield, hybridSpellblade };
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
