using System;

namespace GameRpg.Mobility
{
    /// <summary>
    /// Pure decision logic for the mobility test scene's platformer movement
    /// (feature 005) — ground/aerial/wall jump, wall-slide fall-speed decision,
    /// and the charge-jump resource. Knows nothing about Rigidbody2D, Transform,
    /// or sprites; the MonoBehaviour (PlatformerMovementController) only feeds
    /// physics results in and applies the decisions this class returns. See
    /// contracts/movement-state-contract.md and contracts/charge-jump-contract.md.
    ///
    /// Isolated from Combat/Character (FR-014) — this is a standalone technical
    /// test harness, not part of the shipped RPG systems.
    /// </summary>
    public class PlatformerMovementState
    {
        /// <summary>Set by the controller each frame from a ground raycast.</summary>
        public bool IsGrounded { get; set; }

        /// <summary>-1 = wall to the left, 0 = none, 1 = wall to the right. Set by the controller.</summary>
        public int WallContactDirection { get; set; }

        /// <summary>Set by the controller from the crouch input. Only relevant on the ground.</summary>
        public bool IsCrouching { get; set; }

        public int JumpsUsed { get; private set; }

        /// <summary>How many aerial jumps are available beyond the initial ground jump (FR-005).
        /// Defaults to 1 — the standard double jump (ground jump + one jump in the air).
        /// Settable — not just a constructor value — so a future special ability can raise it
        /// further at runtime (e.g. <c>State.MaxAerialJumps += 1;</c>) without changing this class.</summary>
        public int MaxAerialJumps { get; set; }

        public float CurrentChargeSeconds { get; private set; }
        public float MaxChargeSeconds { get; }
        public float MinChargeSecondsToLeap { get; }

        /// <summary>Fraction of free-fall speed applied while wall-sliding (FR-006). 0 &lt; value &lt; 1.</summary>
        public float WallSlideFallSpeedMultiplier { get; }

        /// <summary>Derived condition (contract): sliding whenever airborne and touching a wall.</summary>
        public bool IsWallSliding => !IsGrounded && WallContactDirection != 0;

        public PlatformerMovementState(
            int maxAerialJumps = 1,
            float maxChargeSeconds = 1.5f,
            float minChargeSecondsToLeap = 0.15f,
            float wallSlideFallSpeedMultiplier = 0.5f)
        {
            if (maxAerialJumps < 0) throw new ArgumentOutOfRangeException(nameof(maxAerialJumps));
            if (maxChargeSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(maxChargeSeconds));
            if (minChargeSecondsToLeap < 0 || minChargeSecondsToLeap >= maxChargeSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(minChargeSecondsToLeap));
            }

            if (wallSlideFallSpeedMultiplier <= 0 || wallSlideFallSpeedMultiplier >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(wallSlideFallSpeedMultiplier));
            }

            MaxAerialJumps = maxAerialJumps;
            MaxChargeSeconds = maxChargeSeconds;
            MinChargeSecondsToLeap = minChargeSecondsToLeap;
            WallSlideFallSpeedMultiplier = wallSlideFallSpeedMultiplier;
        }

        /// <summary>Called by the controller when a ground raycast detects contact; always resets the aerial-jump count.</summary>
        public void NotifyGrounded()
        {
            JumpsUsed = 0;
        }

        /// <summary>FR-004: only takes effect while grounded. Does not count toward MaxAerialJumps.</summary>
        public bool TryGroundJump()
        {
            return IsGrounded;
        }

        /// <summary>FR-005: only takes effect in the air, up to MaxAerialJumps (1 by default —
        /// the standard double jump; a further jump attempt has no effect unless MaxAerialJumps
        /// is raised by a future ability).</summary>
        public bool TryAerialJump()
        {
            if (IsGrounded || JumpsUsed >= MaxAerialJumps)
            {
                return false;
            }

            JumpsUsed++;
            return true;
        }

        /// <summary>FR-007: only takes effect while airborne and touching a wall. Resets JumpsUsed
        /// so wall jumps can be chained indefinitely across different walls.</summary>
        public bool TryWallJump()
        {
            if (IsGrounded || WallContactDirection == 0)
            {
                return false;
            }

            JumpsUsed = 0;
            return true;
        }

        /// <summary>FR-006: the pure class decides the fall-speed value, not just the condition.</summary>
        public float GetFallSpeedMultiplier() => IsWallSliding ? WallSlideFallSpeedMultiplier : 1f;

        /// <summary>FR-008/FR-009: internally guarded — has no effect unless grounded and crouching.
        /// The controller may call this unconditionally every frame.</summary>
        public void AdvanceCharge(float deltaSeconds)
        {
            if (!IsGrounded || !IsCrouching)
            {
                return;
            }

            CurrentChargeSeconds = Math.Min(MaxChargeSeconds, CurrentChargeSeconds + Math.Max(0f, deltaSeconds));
        }

        /// <summary>FR-010: returns the charge fraction (0 if below the minimum threshold — no
        /// leap occurs), and always resets the accumulated charge.</summary>
        public float ReleaseCharge()
        {
            var charge = CurrentChargeSeconds;
            CurrentChargeSeconds = 0f;

            return charge < MinChargeSecondsToLeap ? 0f : charge / MaxChargeSeconds;
        }
    }
}
