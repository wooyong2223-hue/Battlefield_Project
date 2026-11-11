using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEngine;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterViewTests
    {
        private GameObject _actor;

        [TearDown]
        public void CleanUp()
        {
            if (_actor != null) Object.DestroyImmediate(_actor);
        }

        [Test]
        public void MouseTurnsTheCharacterAndPerspectiveTogglePreservesHeading()
        {
            PlayerCharacterLookController view = CreateView();
            InputState input = new() { Look = new Vector2(20f, 10f) };
            view.Tick(input, 0.02f);
            Assert.That(Mathf.DeltaAngle(0f, view.transform.eulerAngles.y), Is.EqualTo(60f).Within(0.01f));
            Assert.That(Mathf.DeltaAngle(0f, view.AimRotation.eulerAngles.x), Is.EqualTo(-30f).Within(0.01f));
            input.Look = Vector2.zero;
            input.TogglePerspectivePressed = true;
            view.Tick(input, 0.02f);
            Assert.That(view.IsFirstPersonView, Is.True);
            view.Tick(input, 0.02f);
            Assert.That(view.IsFirstPersonView, Is.False);
            Assert.That(Mathf.DeltaAngle(0f, view.transform.eulerAngles.y), Is.EqualTo(60f).Within(0.01f));
        }

        [Test]
        public void FreeLookKeepsMovementAndAimHeadingAndReturnsToTheCharacter()
        {
            PlayerCharacterLookController view = CreateView();
            InputState input = new() { FreeLookHeld = true, Look = new Vector2(100f, 100f) };
            Quaternion aim = view.AimRotation;
            view.Tick(input, 0.02f);
            Assert.That(view.IsFreeLooking, Is.True);
            Assert.That(Quaternion.Angle(view.AimRotation, aim), Is.LessThan(0.01f));
            Assert.That(view.transform.eulerAngles.y, Is.EqualTo(0f));
            Assert.That(Mathf.DeltaAngle(0f, view.LookRotation.eulerAngles.x), Is.EqualTo(-80f).Within(0.01f));
            input.FreeLookHeld = false;
            input.Look = Vector2.zero;
            for (int frame = 0; frame < 60; frame++) view.Tick(input, 0.02f);
            Assert.That(Quaternion.Angle(view.LookRotation, view.AimRotation), Is.LessThan(0.1f));
        }

        [Test]
        public void FreeLookCanLookBehindInThirdPersonAndLimitsFirstPersonHeadTurn()
        {
            PlayerCharacterLookController view = CreateView();
            InputState input = new() { FreeLookHeld = true, Look = new Vector2(100f, 0f) };
            view.Tick(input, 0.02f);
            Assert.That(Vector3.Angle(view.LookRotation * Vector3.forward, Vector3.back), Is.LessThan(0.01f));
            input.TogglePerspectivePressed = true;
            view.Tick(input, 0.02f);
            Assert.That(Vector3.Angle(view.LookRotation * Vector3.forward, Vector3.right), Is.LessThan(0.01f));
        }

        [Test]
        public void HoldAimsWhileTapTogglesAdsAndKeepsThePreferredPerspective()
        {
            PlayerCharacterLookController view = CreateView();
            InputState input = new() { AimHeld = true, AimPressed = true };
            view.Tick(input, 0.02f);
            input.AimPressed = false;
            for (int frame = 0; frame < 20; frame++) view.Tick(input, 0.02f);
            Assert.That(view.IsAiming, Is.True);
            Assert.That(view.IsFirstPersonView, Is.False);
            input.AimHeld = false;
            input.AimReleased = true;
            view.Tick(input, 0.02f);
            Assert.That(view.IsAds, Is.False);
            TapAim(view, input);
            Assert.That(view.IsAds && view.IsFirstPersonView, Is.True);
            Assert.That(view.IsFirstPerson, Is.False);
            TapAim(view, input);
            Assert.That(view.IsAds || view.IsFirstPersonView, Is.False);
            input.TogglePerspectivePressed = true;
            view.Tick(input, 0.02f);
            input.TogglePerspectivePressed = false;
            TapAim(view, input);
            TapAim(view, input);
            Assert.That(view.IsFirstPersonView, Is.True);
        }

        [Test]
        public void LosingControlClearsAimAndFreeLookWithoutChangingThePreferredPerspective()
        {
            PlayerCharacterLookController view = CreateView();
            InputState input = new() { TogglePerspectivePressed = true, FreeLookHeld = true };
            view.Tick(input, 0.02f);
            input.TogglePerspectivePressed = false;
            input.HasControl = false;
            view.Tick(input, 0.02f);
            Assert.That(view.IsFirstPerson, Is.True);
            Assert.That(view.IsFreeLooking || view.IsAiming || view.IsAds, Is.False);
        }

        private PlayerCharacterLookController CreateView()
        {
            _actor = new GameObject("View Test Character");
            return _actor.AddComponent<PlayerCharacterLookController>();
        }

        private static void TapAim(PlayerCharacterLookController view, InputState input)
        {
            input.AimReleased = false;
            input.AimHeld = input.AimPressed = true;
            view.Tick(input, 0.02f);
            input.AimHeld = input.AimPressed = false;
            input.AimReleased = true;
            view.Tick(input, 0.02f);
            input.AimReleased = false;
        }

        private sealed class InputState : IPlayerCharacterViewInput
        {
            public bool HasControl { get; set; } = true;
            public Vector2 Look { get; set; }
            public bool TogglePerspectivePressed { get; set; }
            public bool FreeLookHeld { get; set; }
            public bool AimHeld { get; set; }
            public bool AimPressed { get; set; }
            public bool AimReleased { get; set; }
        }
    }
}
