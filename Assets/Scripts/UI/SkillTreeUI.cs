using System.Collections.Generic;
using System.Linq;
using GameRpg.Characters;
using GameRpg.Skills;
using UnityEngine;
using UnityEngine.UI;

namespace GameRpg.UI
{
    /// <summary>
    /// Minimal skill-tree UI: lists nodes with an invest/respec button each,
    /// reflecting availability from SkillTreeService (FR-004, FR-005, FR-018).
    /// Presentation-only; no automated tests required for this class per
    /// tasks.md's testing scope note (rendering/UI is exempt from Principle III).
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        [SerializeField] private Transform nodeListContainer;
        [SerializeField] private Button nodeButtonPrefab;

        private SkillTreeService _skillTreeService;
        private Character _character;
        private IReadOnlyList<SkillNodeDefinition> _allNodes;

        public void Initialize(SkillTreeService skillTreeService, Character character, IReadOnlyList<SkillNodeDefinition> allNodes)
        {
            _skillTreeService = skillTreeService;
            _character = character;
            _allNodes = allNodes;
            Refresh();
        }

        public void Refresh()
        {
            if (nodeListContainer == null || nodeButtonPrefab == null || _allNodes == null)
            {
                return;
            }

            for (var i = nodeListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(nodeListContainer.GetChild(i).gameObject);
            }

            foreach (var node in _allNodes)
            {
                var button = Instantiate(nodeButtonPrefab, nodeListContainer);
                var label = button.GetComponentInChildren<Text>();
                var acquired = _character.AcquiredSkillNodeIds.Contains(node.NodeId);
                var available = _skillTreeService.IsAvailableForInvestment(_character, node);

                if (label != null)
                {
                    label.text = acquired ? $"{node.DisplayName} (adquirido)" : node.DisplayName;
                }

                button.interactable = acquired || available;
                var capturedNode = node;
                button.onClick.AddListener(() => OnNodeClicked(capturedNode, acquired));
            }
        }

        private void OnNodeClicked(SkillNodeDefinition node, bool alreadyAcquired)
        {
            if (alreadyAcquired)
            {
                _skillTreeService.Respec(_character, node);
            }
            else
            {
                _skillTreeService.AcquireNode(_character, node);
            }

            Refresh();
        }
    }
}
