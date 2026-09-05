using GameRpg.Mobility;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class PlatformerMovementStateTests
    {
        // --- Ground / aerial jump (FR-004, FR-005) ---

        [Test]
        public void TryGroundJump_WhenGrounded_ReturnsTrue_AndDoesNotIncrementJumpsUsed()
        {
            var state = new PlatformerMovementState();
            state.IsGrounded = true;

            var jumped = state.TryGroundJump();

            Assert.IsTrue(jumped);
            Assert.AreEqual(0, state.JumpsUsed);
        }

        [Test]
        public void TryGroundJump_WhenAirborne_ReturnsFalse()
        {
            var state = new PlatformerMovementState();
            state.IsGrounded = false;

            Assert.IsFalse(state.TryGroundJump());
        }

        [Test]
        public void TryAerialJump_FirstTimeInAir_ReturnsTrue_AndIncrementsJumpsUsed()
        {
            var state = new PlatformerMovementState(maxAerialJumps: 1);
            state.IsGrounded = false;

            var jumped = state.TryAerialJump();

            Assert.IsTrue(jumped);
            Assert.AreEqual(1, state.JumpsUsed);
        }

        [Test]
        public void TryAerialJump_AfterLimitReached_ReturnsFalse()
        {
            var state = new PlatformerMovementState(maxAerialJumps: 1);
            state.IsGrounded = false;
            state.TryAerialJump();

            var secondAttempt = state.TryAerialJump();

            Assert.IsFalse(secondAttempt);
            Assert.AreEqual(1, state.JumpsUsed);
        }

        [Test]
        public void Constructor_Default_AllowsOneAerialJump()
        {
            // FR-005: double-jump defaults to 1 aerial jump (ground jump + 1 in the air).
            var state = new PlatformerMovementState();

            Assert.AreEqual(1, state.MaxAerialJumps);
        }

        [Test]
        public void TryAerialJump_WithDefaultLimit_AllowsOneJump_ButNotASecond()
        {
            var state = new PlatformerMovementState();
            state.IsGrounded = false;

            var first = state.TryAerialJump();
            var second = state.TryAerialJump();

            Assert.IsTrue(first);
            Assert.IsFalse(second);
            Assert.AreEqual(1, state.JumpsUsed);
        }

        [Test]
        public void MaxAerialJumps_IncreasedAtRuntime_AllowsAnExtraJump()
        {
            // A future special ability can grant more aerial jumps without changing this class.
            var state = new PlatformerMovementState(maxAerialJumps: 1);
            state.IsGrounded = false;
            state.TryAerialJump();
            Assert.IsFalse(state.TryAerialJump(), "Sanity check: limit reached at 1.");

            state.MaxAerialJumps += 1;

            Assert.IsTrue(state.TryAerialJump(), "Raising MaxAerialJumps at runtime unlocks another jump immediately.");
        }

        [Test]
        public void TryAerialJump_WhileGrounded_ReturnsFalse()
        {
            var state = new PlatformerMovementState();
            state.IsGrounded = true;

            Assert.IsFalse(state.TryAerialJump());
        }

        [Test]
        public void NotifyGrounded_ResetsJumpsUsed()
        {
            var state = new PlatformerMovementState(maxAerialJumps: 1);
            state.IsGrounded = false;
            state.TryAerialJump();

            state.NotifyGrounded();

            Assert.AreEqual(0, state.JumpsUsed);
        }

        // --- Wall contact / wall jump (FR-006, FR-007) ---

        [Test]
        public void TryWallJump_NotTouchingWall_ReturnsFalse()
        {
            var state = new PlatformerMovementState();
            state.IsGrounded = false;
            state.WallContactDirection = 0;

            Assert.IsFalse(state.TryWallJump());
        }

        [Test]
        public void TryWallJump_WhileGrounded_ReturnsFalse()
        {
            var state = new PlatformerMovementState();
            state.IsGrounded = true;
            state.WallContactDirection = 1;

            Assert.IsFalse(state.TryWallJump());
        }

        [Test]
        public void TryWallJump_TouchingWallInAir_ReturnsTrue_AndResetsJumpsUsed()
        {
            var state = new PlatformerMovementState(maxAerialJumps: 1);
            state.IsGrounded = false;
            state.TryAerialJump();
            Assert.AreEqual(1, state.JumpsUsed);

            state.WallContactDirection = 1;
            var jumped = state.TryWallJump();

            Assert.IsTrue(jumped);
            Assert.AreEqual(0, state.JumpsUsed, "Wall jump resets the aerial-jump count, allowing chaining across walls.");
        }

        [Test]
        public void IsWallSliding_OnlyTrueWhenAirborneAndTouchingWall()
        {
            var state = new PlatformerMovementState();

            state.IsGrounded = false;
            state.WallContactDirection = -1;
            Assert.IsTrue(state.IsWallSliding);

            state.IsGrounded = true;
            Assert.IsFalse(state.IsWallSliding, "Touching the ground exits the wall-sliding condition immediately.");

            state.IsGrounded = false;
            state.WallContactDirection = 0;
            Assert.IsFalse(state.IsWallSliding);
        }

        [Test]
        public void GetFallSpeedMultiplier_WhileWallSliding_ReturnsConfiguredMultiplier()
        {
            var state = new PlatformerMovementState(wallSlideFallSpeedMultiplier: 0.4f);
            state.IsGrounded = false;
            state.WallContactDirection = 1;

            Assert.AreEqual(0.4f, state.GetFallSpeedMultiplier());
        }

        [Test]
        public void GetFallSpeedMultiplier_NotWallSliding_ReturnsOne()
        {
            var state = new PlatformerMovementState(wallSlideFallSpeedMultiplier: 0.4f);

            state.IsGrounded = true;
            Assert.AreEqual(1f, state.GetFallSpeedMultiplier());

            state.IsGrounded = false;
            state.WallContactDirection = 0;
            Assert.AreEqual(1f, state.GetFallSpeedMultiplier());
        }

        // --- Charge jump (FR-008, FR-009, FR-010) ---

        [Test]
        public void AdvanceCharge_WhileAirborne_HasNoEffect()
        {
            var state = new PlatformerMovementState();
            state.IsGrounded = false;
            state.IsCrouching = true;

            state.AdvanceCharge(1f);

            Assert.AreEqual(0f, state.CurrentChargeSeconds, "FR-008 Edge Case: crouching/charging has no effect in the air.");
        }

        [Test]
        public void AdvanceCharge_GroundedButNotCrouching_HasNoEffect()
        {
            var state = new PlatformerMovementState();
            state.IsGrounded = true;
            state.IsCrouching = false;

            state.AdvanceCharge(1f);

            Assert.AreEqual(0f, state.CurrentChargeSeconds);
        }

        [Test]
        public void AdvanceCharge_GroundedAndCrouching_Accumulates()
        {
            var state = new PlatformerMovementState(maxChargeSeconds: 2f);
            state.IsGrounded = true;
            state.IsCrouching = true;

            state.AdvanceCharge(0.5f);

            Assert.AreEqual(0.5f, state.CurrentChargeSeconds);
        }

        [Test]
        public void AdvanceCharge_NeverExceedsMaxChargeSeconds()
        {
            var state = new PlatformerMovementState(maxChargeSeconds: 1f);
            state.IsGrounded = true;
            state.IsCrouching = true;

            state.AdvanceCharge(5f);

            Assert.AreEqual(1f, state.CurrentChargeSeconds);
        }

        [Test]
        public void ReleaseCharge_BelowMinimumThreshold_ReturnsZero_AndResetsCharge()
        {
            var state = new PlatformerMovementState(maxChargeSeconds: 1f, minChargeSecondsToLeap: 0.2f);
            state.IsGrounded = true;
            state.IsCrouching = true;
            state.AdvanceCharge(0.1f);

            var fraction = state.ReleaseCharge();

            Assert.AreEqual(0f, fraction, "Edge Case: releasing without enough charge does not leap.");
            Assert.AreEqual(0f, state.CurrentChargeSeconds);
        }

        [Test]
        public void ReleaseCharge_AboveMinimumThreshold_ReturnsProportionalFraction_AndResetsCharge()
        {
            var state = new PlatformerMovementState(maxChargeSeconds: 2f, minChargeSecondsToLeap: 0.2f);
            state.IsGrounded = true;
            state.IsCrouching = true;
            state.AdvanceCharge(1f);

            var fraction = state.ReleaseCharge();

            Assert.AreEqual(0.5f, fraction);
            Assert.AreEqual(0f, state.CurrentChargeSeconds);
        }
    }
}
