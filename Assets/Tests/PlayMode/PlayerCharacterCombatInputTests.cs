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
    public class PlayerCharacterCombatInputTests : InputTestFixture
    {
        private Keyboard _keyboard;
        private Mouse _mouse;
        private PlayerCharacterInput _input;
        private InputSettings.UpdateMode _previous;
        [SetUp] public void ConfigureInput()
        {_previous=InputSystem.settings.updateMode;InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsManually;}
        [TearDown] public void RestoreInput()
        {if(_input!=null)_input.enabled=false;if(_keyboard!=null)InputSystem.RemoveDevice(_keyboard);if(_mouse!=null)InputSystem.RemoveDevice(_mouse);InputSystem.settings.updateMode=_previous;}
        [UnityTest]
        public IEnumerator CombatKeysAreMappedAndVehicleControlSuppressesAllOnFootCommands()
        {
            yield return CharacterViewTestContext.Load();var context=new CharacterViewTestContext(true);
            _input=context.Shooter.GetComponent<PlayerCharacterInput>();_keyboard=InputSystem.AddDevice<Keyboard>();_mouse=InputSystem.AddDevice<Mouse>();
            yield return null;yield return null;
            CharacterViewTestContext.SetField(_input,"_captured",true);CharacterViewTestContext.SetField(_input,"_captureFrame",-1);
            Press(Key.Digit1);Assert.That(_input.WeaponSlotSelection,Is.EqualTo(1));
            Press();Press(Key.Digit2);Assert.That(_input.WeaponSlotSelection,Is.EqualTo(2));
            Press();Press(Key.Digit3);Assert.That(_input.WeaponSlotSelection,Is.EqualTo(3));
            Press();Press(Key.R);Assert.That(_input.ReloadPressed,Is.True);
            Press();Press(Key.G);Assert.That(_input.MeleePressed,Is.True);Assert.That(_input.EquipGrenadePressed,Is.False);
            Press();Press(Key.Digit4);Assert.That(_input.EquipGrenadePressed,Is.True);Assert.That(_input.MeleePressed,Is.False);
            InputSystem.QueueStateEvent(_mouse,new MouseState().WithButton(MouseButton.Left));InputSystem.Update();
            Assert.That(_input.ThrowGrenadePressed,Is.True);_input.ConsumePrimaryUntilRelease();
            Assert.That(_input.FireWeapon||_input.ThrowGrenadePressed,Is.False);
            Assert.That(_input.ThrowGrenadeHeld,Is.True,"Consumed weapon fire must not hide grenade hold.");
            InputSystem.QueueStateEvent(_mouse,new MouseState());InputSystem.Update();
            Assert.That(_input.ThrowGrenadeReleased,Is.True,"Consumed weapon fire must not hide grenade release.");yield return null;
            InputSystem.QueueStateEvent(_mouse,new MouseState().WithButton(MouseButton.Right));InputSystem.Update();
            Assert.That(_input.ToggleGrenadeThrowModePressed,Is.True);_input.ConsumeAimUntilRelease();
            Assert.That(_input.AimPressed||_input.AimHeld||_input.AimReleased,Is.False);
            InputSystem.QueueStateEvent(_mouse,new MouseState());InputSystem.Update();yield return null;
            Press();Press(Key.C);Assert.That(_input.SlidePressed,Is.True);Assert.That(_input.CrouchPressed,Is.False);
            Press();Press(Key.LeftCtrl);Assert.That(_input.CrouchPressed,Is.True);Assert.That(_input.SlidePressed,Is.False);
            _input.SetGameplayEnabled(false);
            Press();Press(Key.Digit1,Key.R,Key.G,Key.C,Key.Digit4);Assert.That(_input.WeaponSlotSelection,Is.Zero);
            Assert.That(_input.ReloadPressed||_input.MeleePressed||_input.SlidePressed||_input.EquipGrenadePressed,Is.False);
        }
        private void Press(params Key[] keys){InputSystem.QueueStateEvent(_keyboard,new KeyboardState(keys));InputSystem.Update();}
        [UnityTest]
        public IEnumerator GrenadeRightClickCannotBecomeWeaponAdsAfterRelease()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext(true);
            _input=c.Shooter.GetComponent<PlayerCharacterInput>();_keyboard=InputSystem.AddDevice<Keyboard>();_mouse=InputSystem.AddDevice<Mouse>();
            var motor=c.Actor.GetComponent<PlayerCharacterMovement>();CharacterViewTestContext.SetField(motor,"_input",null);motor.enabled=true;c.Shooter.enabled=true;
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=c.Actor.position+Vector3.down*2.25f;floor.transform.localScale=new Vector3(100,.5f,100);Physics.SyncTransforms();
            float previous=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
            try
            {
                yield return null;yield return null;CharacterViewTestContext.SetField(_input,"_captured",true);CharacterViewTestContext.SetField(_input,"_captureFrame",-1);
                for(int i=0;i<20;i++){motor.Tick(Vector2.zero,false,false,.02f);InputSystem.Update();yield return null;}
                var grenade=c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();Press(Key.Digit4);yield return null;Press();yield return null;
                Assert.That(grenade.IsEquipped,Is.True);
                InputSystem.QueueStateEvent(_mouse,new MouseState().WithButton(MouseButton.Left));InputSystem.Update();yield return null;
                InputSystem.QueueStateEvent(_mouse,new MouseState().WithButton(MouseButton.Left).WithButton(MouseButton.Right));InputSystem.Update();yield return null;
                Assert.That(grenade.ThrowMode,Is.EqualTo(GrenadeThrowMode.Underhand));
                InputSystem.QueueStateEvent(_mouse,new MouseState().WithButton(MouseButton.Right));InputSystem.Update();yield return null;
                Assert.That(grenade.IsEquipped,Is.False);Assert.That(c.View.IsAds||c.View.IsAiming,Is.False);
                InputSystem.Update();yield return null;Assert.That(c.View.IsAds||c.View.IsAiming,Is.False,"Held RMB must not aim the restored gun.");
                InputSystem.QueueStateEvent(_mouse,new MouseState());InputSystem.Update();yield return null;
                Assert.That(c.View.IsAds,Is.False,"RMB release must not toggle ADS.");
                InputSystem.QueueStateEvent(_mouse,new MouseState().WithButton(MouseButton.Right));InputSystem.Update();yield return null;
                InputSystem.QueueStateEvent(_mouse,new MouseState());InputSystem.Update();yield return null;
                Assert.That(c.View.IsAds,Is.True,"A fresh RMB tap after release must work normally.");
            }
            finally{Time.captureDeltaTime=previous;Object.Destroy(floor);}
        }
    }
}
#endif
