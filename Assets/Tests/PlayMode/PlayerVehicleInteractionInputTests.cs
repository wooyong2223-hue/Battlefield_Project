#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Fighter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerVehicleInteractionInputTests : InputTestFixture
    {
        [SetUp]
        public void UseManualInput() => InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;

        [TearDown]
        public void StopSceneInputBeforeDeviceReset()
        {
            foreach (KeyboardJetInput input in Object.FindObjectsByType<KeyboardJetInput>()) input.enabled = false;
            foreach (PlayerCharacterInput input in Object.FindObjectsByType<PlayerCharacterInput>()) input.enabled = false;
        }

        [UnityTest]
        public IEnumerator FBoardsAndExitsWhileEscapeAndRecaptureCannotFireTheFighter()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return KeyboardState(keyboard);
            context.PrepareEntry();
            yield return KeyboardState(keyboard, Key.F);
            Assert.That(context.Occupant.IsSeated, Is.True);
            Assert.That(context.Input.HasControl, Is.False);
            yield return KeyboardState(keyboard, Key.W);
            Assert.That(context.JetInput.Throttle, Is.EqualTo(1f));
            yield return KeyboardState(keyboard, Key.Digit2);
            Assert.That(context.Seat.GetComponent<WeaponController>().SelectedIndex, Is.EqualTo(1));
            yield return KeyboardState(keyboard, Key.Digit1);
            Assert.That(context.Seat.GetComponent<WeaponController>().SelectedIndex, Is.Zero);
            yield return KeyboardState(keyboard, Key.W, Key.Escape);
            Assert.That(context.Input.HasPointerCapture, Is.False);
            Assert.That(context.JetInput.enabled, Is.False);
            Assert.That(context.JetInput.FireWeapon, Is.False);
            yield return KeyboardState(keyboard);
            yield return MouseState(mouse, 1);
            Assert.That(context.Input.HasPointerCapture, Is.True);
            Assert.That(context.JetInput.FireWeapon, Is.False, "The recapture click must not fire aircraft weapons.");
            yield return MouseState(mouse, 0);
            yield return MouseState(mouse, 1);
            Assert.That(context.JetInput.FireWeapon, Is.True);
            yield return MouseState(mouse, 0);
            yield return KeyboardState(keyboard, Key.F);
            Assert.That(context.Occupant.IsSeated, Is.False);
            Assert.That(context.Input.HasControl, Is.True);
            Assert.That(context.FootCamera.enabled, Is.True);
            Assert.That(context.JetCamera.enabled, Is.False);
        }

        private static IEnumerator KeyboardState(Keyboard keyboard, params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
            yield return null;
            InputSystem.Update();
            yield return null;
            InputSystem.Update();
            yield return null;
        }

        private static IEnumerator MouseState(Mouse mouse, ushort buttons)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState {buttons = buttons});
            InputSystem.Update();
            yield return null;
            InputSystem.Update();
            yield return null;
        }
    }
}
#endif
