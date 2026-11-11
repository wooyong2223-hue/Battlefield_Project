#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.Training;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Battlefield.Tests.PlayMode
{
    public class RifleSelectionStationTests : InputTestFixture
    {
        private sealed class InteractionInput:IPlayerCharacterInteractionInput
        {public bool HasControl {get;set;}=true;public bool InteractPressed=>false;}
        private static void Move(CharacterViewTestContext c,Vector3 position)
        {var body=c.Actor.GetComponent<CharacterController>();body.enabled=false;c.Actor.position=position;body.enabled=true;Physics.SyncTransforms();}
        [UnityTest]
        public IEnumerator ActualFKeySelectsTheDisplayedRifleAndPreservesPreviousAmmunition()
        {
            var previous=InputSystem.settings.updateMode;Keyboard keyboard=null;
            try
            {
                InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsManually;
                yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext(true);c.Shooter.enabled=true;
                keyboard=InputSystem.AddDevice<Keyboard>();var input=c.Actor.GetComponent<PlayerCharacterInput>();
                CharacterViewTestContext.SetField(input,"_captured",true);CharacterViewTestContext.SetField(input,"_captureFrame",-1);
                var stations=Object.FindObjectsByType<RifleSelectionStation>().OrderBy(s=>s.Definition.DisplayName).ToArray();Assert.That(stations.Length,Is.EqualTo(2));
                var interaction=c.Actor.GetComponent<PlayerRifleStationInteraction>();var equipment=c.Actor.GetComponent<PlayerCharacterLoadout>().GetEquipment(1);
                var akAmmo=equipment.Magazine;akAmmo.TryConsumeRound();akAmmo.TryConsumeRound();
                foreach(var station in stations.OrderByDescending(s=>s.Definition.DisplayName))
                {
                    Move(c,station.InteractionPoint-Vector3.forward*2);yield return null;yield return null;
                    Assert.That(c.Actor.GetComponent<PlayerVehicleInteraction>().NearbySeat,Is.Null,"Weapon stands must be clear of vehicle entry interaction.");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F));InputSystem.Update();yield return null;
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();yield return null;
                    Assert.That(interaction.NearbyStation,Is.SameAs(station));Assert.That(equipment.Definition,Is.SameAs(station.Definition));
                    Assert.That(c.Shooter.WeaponSlot,Is.EqualTo(1));Assert.That(interaction.IsEquipped,Is.True);
                }
                Assert.That(akAmmo.Rounds,Is.EqualTo(28));
            }
            finally{if(keyboard!=null)InputSystem.RemoveDevice(keyboard);InputSystem.settings.updateMode=previous;}
        }
        [UnityTest]
        public IEnumerator RangeWallsAndMissingWalkingControlPreventStationUse()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();c.Shooter.enabled=true;
            var interaction=c.Actor.GetComponent<PlayerRifleStationInteraction>();var input=new InteractionInput();
            CharacterViewTestContext.SetField(interaction,"_input",input);
            var station=Object.FindObjectsByType<RifleSelectionStation>().Single(s=>s.Definition.DisplayName=="M4A1");
            Move(c,station.InteractionPoint-Vector3.forward*6);Assert.That(interaction.TryUseNearest(),Is.False);
            Move(c,station.InteractionPoint-Vector3.forward*2);yield return null;
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=station.InteractionPoint-Vector3.forward;wall.transform.localScale=new Vector3(2,4,.2f);Physics.SyncTransforms();
            Assert.That(interaction.TryUseNearest(),Is.False);Object.Destroy(wall);yield return null;
            input.HasControl=false;Assert.That(interaction.TryUseNearest(),Is.False);input.HasControl=true;
            c.Shooter.enabled=false;Assert.That(interaction.TryUseNearest(),Is.False);c.Shooter.enabled=true;
            Assert.That(interaction.TryUseNearest(),Is.True);
        }
    }
}
#endif
