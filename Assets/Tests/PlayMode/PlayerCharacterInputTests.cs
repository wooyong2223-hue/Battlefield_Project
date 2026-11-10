#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterInputTests : InputTestFixture
    {
        private Keyboard _keyboard;
        private Mouse _mouse;
        private InputSettings.UpdateMode _previousUpdateMode;

        [SetUp]
        public void SetUp()
        {
            _previousUpdateMode = InputSystem.settings.updateMode;
            // Batch-mode Editor updates otherwise consume queued keyboard events before the player loop.
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
        }

        [TearDown]
        public void CleanUp()
        {
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            if (_mouse != null) InputSystem.RemoveDevice(_mouse);
            InputSystem.settings.updateMode = _previousUpdateMode;
        }

        [UnityTest]
        public IEnumerator KeyboardAndMouseBindingsTogglePerspectiveAndFreeLook()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext(true);
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();
            yield return MouseState(1);
            yield return MouseState(0);
            PlayerCharacterInput input = context.Shooter.GetComponent<PlayerCharacterInput>();
            Assert.That(input.HasControl, Is.True);

            yield return KeyboardState(Key.V);
            Assert.That(context.View.IsFirstPersonView, Is.True);
            yield return KeyboardState();
            yield return KeyboardState(Key.V);
            Assert.That(context.View.IsFirstPersonView, Is.False);
            yield return KeyboardState(Key.LeftAlt);
            Quaternion heading = context.Actor.rotation;
            InputSystem.QueueStateEvent(_mouse, new MouseState { delta = new Vector2(100f, 0f) });
            InputSystem.Update();
            yield return null;
            InputSystem.Update();
            yield return null;
            Assert.That(context.View.IsFreeLooking, Is.True);
            Assert.That(Quaternion.Angle(context.Actor.rotation, heading), Is.LessThan(0.01f));
            Assert.That(Quaternion.Angle(context.View.LookRotation, context.View.AimRotation), Is.GreaterThan(20f));
        }

        [UnityTest]
        public IEnumerator EscapeReleasesControlAndTheRecaptureClickDoesNotFire()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext(true);
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();
            yield return MouseState(1);
            yield return MouseState(0);
            PlayerCharacterInput input = context.Shooter.GetComponent<PlayerCharacterInput>();
            yield return KeyboardState(Key.Escape);
            Assert.That(input.HasControl, Is.False);
            Assert.That(input.FireWeapon, Is.False);
            yield return KeyboardState();
            yield return MouseState(1);
            Assert.That(input.HasControl, Is.True);
            Assert.That(input.FireWeapon, Is.False, "Recapturing the pointer must not fire a shot.");
            yield return MouseState(0);
            yield return MouseState(1);
            Assert.That(input.FireWeapon, Is.True);
            yield return MouseState(0);
            input.enabled = false;
            Assert.That(input.HasControl, Is.False);
            input.enabled = true;
            yield return null;
            yield return null;
            Assert.That(input.HasControl, Is.True);
            Assert.That(input.FireWeapon, Is.False);
        }

        private IEnumerator KeyboardState(params Key[] keys)
        {
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(keys));
            InputSystem.Update();
            yield return null;
            InputSystem.Update();
            yield return null;
        }

        private IEnumerator MouseState(ushort buttons)
        {
            InputSystem.QueueStateEvent(_mouse, new MouseState { buttons = buttons });
            InputSystem.Update();
            yield return null;
            InputSystem.Update();
            yield return null;
        }
    }
}
#endif
