using GameRpg.Combat.Grid;
using UnityEngine;

namespace GameRpg.Demo
{
    /// <summary>Forwards a click on a grid tile primitive to the demo controller.</summary>
    public class GridTileClickHandler : MonoBehaviour
    {
        private CombatDemoController _controller;
        private GridCoordinate _coordinate;

        public void Initialize(CombatDemoController controller, GridCoordinate coordinate)
        {
            _controller = controller;
            _coordinate = coordinate;
        }

        private void OnMouseDown()
        {
            _controller.TryMovePlayerTo(_coordinate);
        }
    }
}
