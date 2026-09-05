using System;

namespace GameRpg.Combat
{
    /// <summary>
    /// A continuous 2D side-view combat space with horizontal bounds (research.md,
    /// "Decision: Arena de combate como espaço contínuo"). Replaces the
    /// turn-based Combat.Grid.GridMap — there are no cells, occupancy, or
    /// terrain; a combatant's position is any float within [MinX, MaxX]
    /// (FR-002).
    /// </summary>
    public class BattleArena
    {
        public float MinX { get; }
        public float MaxX { get; }

        public BattleArena(float minX, float maxX)
        {
            if (maxX <= minX)
            {
                throw new ArgumentOutOfRangeException(nameof(maxX), "BattleArena requires maxX > minX.");
            }

            MinX = minX;
            MaxX = maxX;
        }

        public float Clamp(float positionX) => Math.Clamp(positionX, MinX, MaxX);

        public bool IsWithinBounds(float positionX) => positionX >= MinX && positionX <= MaxX;
    }
}
