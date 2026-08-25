using GameRpg.World;
using UnityEngine;
using UnityEngine.UI;

namespace GameRpg.UI
{
    /// <summary>
    /// Shows a community's reputation and economic status, including a
    /// permanent-collapse indicator (FR-019, FR-020). Presentation-only;
    /// exempt from automated test coverage per tasks.md's testing scope note.
    /// </summary>
    public class ReputationEconomyUI : MonoBehaviour
    {
        [SerializeField] private Text communityNameLabel;
        [SerializeField] private Text reputationLabel;
        [SerializeField] private Text economyStateLabel;
        [SerializeField] private GameObject permanentCollapseBanner;

        public void Display(Community community, string communityDisplayName, CommunityEconomyState economyState)
        {
            if (communityNameLabel != null)
            {
                communityNameLabel.text = communityDisplayName;
            }

            if (reputationLabel != null)
            {
                reputationLabel.text = community.ReputationWithPlayer.ToString();
            }

            if (economyStateLabel != null)
            {
                economyStateLabel.text = economyState.ToString();
            }

            if (permanentCollapseBanner != null)
            {
                permanentCollapseBanner.SetActive(community.IsPermanentlyInactive);
            }
        }
    }
}
