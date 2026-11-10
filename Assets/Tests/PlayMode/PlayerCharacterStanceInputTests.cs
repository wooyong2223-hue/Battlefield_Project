#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using Battlefield.Features.Fighter;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterStanceInputTests : InputTestFixture
    {
        [SetUp]
        public void ManualInput() => InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;

        [TearDown]
        public void StopInputBeforeReset()
        {
            foreach (PlayerCharacterInput input in Object.FindObjectsByType<PlayerCharacterInput>()) input.enabled = false;
            foreach (KeyboardJetInput input in Object.FindObjectsByType<KeyboardJetInput>()) input.enabled = false;
        }

        [UnityTest]
        public IEnumerator ZAndLeanKeysUseWalkingControlAndAreSuppressedAfterEscape()
        {
            yield return BoardingTestContext.Load(); var context = new BoardingTestContext();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>(); yield return new WaitForSeconds(.8f);
            typeof(PlayerCharacterInput).GetField("_captured", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(context.Input, true);
            typeof(PlayerCharacterInput).GetField("_captureFrame", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(context.Input, -1);
            yield return State(keyboard, Key.Z); Assert.That(context.Actor.GetComponent<PlayerCharacterStance>().IsProne, Is.True);
            yield return State(keyboard); yield return State(keyboard, Key.Z);
            Assert.That(context.Actor.GetComponent<PlayerCharacterStance>().IsLowered, Is.False);
            yield return State(keyboard, Key.Q); Assert.That(context.Input.Lean, Is.LessThan(0f));
            yield return State(keyboard, Key.E); Assert.That(context.Input.Lean, Is.GreaterThan(0f));
            yield return State(keyboard, Key.Escape); yield return State(keyboard, Key.Z, Key.Q, Key.B, Key.Enter);
            Assert.That(context.Input.PronePressed, Is.False); Assert.That(context.Input.Lean, Is.Zero);
            Assert.That(context.Input.MountHeld || context.Input.RespawnPressed, Is.False);
        }

        [UnityTest]
        public IEnumerator LeftControlTogglesCrouchOnlyWhileWalkingInputIsCaptured()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            yield return new WaitForSeconds(.8f);
            typeof(PlayerCharacterInput).GetField("_captured", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(context.Input, true);
            typeof(PlayerCharacterInput).GetField("_captureFrame", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(context.Input, -1);
            PlayerCharacterStance stance = context.Actor.GetComponent<PlayerCharacterStance>();
            yield return State(keyboard, Key.LeftCtrl);
            Assert.That(stance.IsCrouched, Is.True);
            yield return State(keyboard);
            yield return State(keyboard, Key.LeftCtrl);
            Assert.That(stance.IsCrouched, Is.False);
            yield return State(keyboard, Key.Escape);
            yield return State(keyboard, Key.LeftCtrl);
            Assert.That(stance.IsCrouched, Is.False);
        }
        private static IEnumerator State(Keyboard keyboard, params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));InputSystem.Update();yield return null;
            InputSystem.Update();yield return null;
        }
    }
}
#endif
