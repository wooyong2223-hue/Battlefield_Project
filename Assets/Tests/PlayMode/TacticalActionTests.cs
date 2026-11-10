#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Projectile;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Battlefield.Tests.PlayMode
{
    [DefaultExecutionOrder(200)]
    public sealed class MantleGroundContactProbe:MonoBehaviour
    {
        public readonly System.Collections.Generic.List<float> Heights=new();
        private SkinnedMeshRenderer _skin;private PlayerCharacterMantle _mantle;private CharacterController _body;private Mesh _mesh;
        public void Initialize(SkinnedMeshRenderer skin,PlayerCharacterMantle mantle){_skin=skin;_mantle=mantle;_body=mantle.GetComponent<CharacterController>();_mesh=new Mesh();}
        private void LateUpdate(){if(_mantle==null || !_mantle.IsBusy)return;_skin.BakeMesh(_mesh);float low=float.MaxValue;foreach(var vertex in _mesh.vertices)low=Mathf.Min(low,_skin.transform.TransformPoint(vertex).y);float ground=_body.transform.TransformPoint(_body.center).y-_body.height*Mathf.Abs(_body.transform.lossyScale.y)*.5f;Heights.Add(low-ground);}
        private void OnDestroy(){if(_mesh!=null)Object.Destroy(_mesh);}
    }
    public sealed class TacticalActionTests
    {
        sealed class Idle:IWeaponInput {public bool FireWeapon=>false;public int WeaponSlotSelection=>0;}
        static GameObject Box(Vector3 position,Vector3 size){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.transform.position=position;o.transform.localScale=size;Physics.SyncTransforms();return o;}
        static PlayerCharacterMovement Motor(CharacterViewTestContext c)
        {CharacterViewTestContext.SetField(c.Shooter,"_input",new Idle());c.Shooter.enabled=true;var motor=c.Actor.GetComponent<PlayerCharacterMovement>();CharacterViewTestContext.SetField(motor,"_input",null);motor.enabled=true;return motor;}
        [UnityTest]public IEnumerator HighLedgeClimbLandsOnTopCrouchedAndBlocksCompetingActions()
        {
            foreach(int slot in new[]{1,2,3})
            {
                yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(slot);
                var mantle=c.Actor.GetComponent<PlayerCharacterMantle>();Assert.That(mantle,Is.Not.Null);
                var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));
                var platform=Box(c.Actor.position+new Vector3(0,-.5f,4),new Vector3(6,3,5));
                float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
                try
                {
                    for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                    Vector3 start=c.Actor.position;Assert.That(mantle.TryBegin(),Is.True);
                    var actions=c.Actor.GetComponent<PlayerCharacterActionPolicy>();Assert.That(actions.CanAttack,Is.False);Assert.That(actions.CanChangeStance,Is.False);
                    Assert.That(c.Actor.GetComponent<PlayerCharacterGrenadeThrow>().TryThrow(),Is.False);
                    var animator=c.Actor.GetComponentInChildren<Animator>();bool sawPose=false;float lastTime=0f;
                    var contact=c.Actor.gameObject.AddComponent<MantleGroundContactProbe>();contact.Initialize(c.Renderer,mantle);
                    for(int n=0;n<80;n++)
                    {
                        motor.Tick(Vector2.up,true,true,.02f);yield return null;
                        if(mantle.IsBusy)
                        {
                            float time=animator.GetFloat("MantleTime");
                            Assert.That(time,Is.GreaterThanOrEqualTo(lastTime-.0001f),"Recovery must not rewind the outgoing native mantle pose.");lastTime=time;
                        }
                        if(mantle.IsClimbing && mantle.AnimationProgress>.25f)
                        {sawPose=true;Assert.That(animator.GetInteger("TraversalMode"),Is.EqualTo(5));Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Mantle"),Is.True);}
                    }
                    Assert.That(contact.Heights.Count,Is.GreaterThan(20));
                    foreach(float height in contact.Heights)Assert.That(height,Is.InRange(-.15f,.15f),"Native mantle mesh must follow the collision feet through recovery.");
                    Assert.That(sawPose,Is.True);Assert.That(mantle.IsClimbing,Is.False);Assert.That(c.Actor.GetComponent<PlayerCharacterStance>().IsCrouched,Is.True);
                    Assert.That(c.Actor.position.y-start.y,Is.InRange(2.9f,3.3f));Assert.That(c.Actor.position.z-start.z,Is.GreaterThan(1.9f));
                    Assert.That(actions.CanAttack,Is.True);Assert.That(animator.applyRootMotion,Is.False);
                }
                finally{Time.captureDeltaTime=old;Object.Destroy(floor);Object.Destroy(platform);}
            }
        }
        [UnityTest]public IEnumerator MantleRejectsLowTallAndBlockedLandingsAndCancelsOnDisable()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);var mantle=c.Actor.GetComponent<PlayerCharacterMantle>();
            var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));var platform=Box(c.Actor.position+new Vector3(0,-.5f,4),new Vector3(6,3,5));
            var ceiling=Box(c.Actor.position+new Vector3(0,3.2f,4),new Vector3(6,.3f,5));
            try
            {
                for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                Assert.That(mantle.TryBegin(),Is.False,"Standing clearance is required on top.");ceiling.SetActive(false);Physics.SyncTransforms();
                platform.transform.localScale=new Vector3(6,1,5);platform.transform.position=c.Actor.position+new Vector3(0,-1.5f,4);Physics.SyncTransforms();
                Assert.That(mantle.TryBegin(),Is.False,"Low obstacles belong to the existing vault.");
                platform.transform.localScale=new Vector3(6,5,5);platform.transform.position=c.Actor.position+new Vector3(0,.5f,4);Physics.SyncTransforms();
                Assert.That(mantle.TryBegin(),Is.False,"Do not climb beyond the authored height range.");
                platform.transform.localScale=new Vector3(6,3,5);platform.transform.position=c.Actor.position+new Vector3(0,-.5f,4);Physics.SyncTransforms();
                Assert.That(mantle.TryBegin(),Is.True);
                for(int n=0;n<10;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                var presentation=c.Actor.GetComponentInChildren<PlayerCharacterTacticalAnimationView>();
                mantle.enabled=false;presentation.enabled=false;Vector3 position=c.Actor.position;motor.Tick(Vector2.zero,false,false,.02f);
                Assert.That(mantle.IsClimbing,Is.False);Assert.That(Vector3.Distance(position,c.Actor.position),Is.LessThan(.1f));
                mantle.enabled=true;presentation.enabled=true;
                for(int n=0;n<40;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                Assert.That(c.Actor.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Mantle"),Is.False,"Interrupted pose must not resume after suspension.");
            }
            finally{Object.Destroy(floor);Object.Destroy(platform);Object.Destroy(ceiling);}
        }
        [UnityTest]public IEnumerator SerializedTacticalBindingsSurviveActorInstantiation()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);
            var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));GameObject clone=null;
            try
            {
                for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                clone=Object.Instantiate(c.Actor.gameObject);clone.GetComponent<PlayerCharacterInput>().enabled=false;
                var body=clone.GetComponent<CharacterController>();body.enabled=false;clone.transform.position+=Vector3.right*30;body.enabled=true;Physics.SyncTransforms();
                for(int n=0;n<20;n++)yield return null;
                var grenade=clone.GetComponent<PlayerCharacterGrenadeThrow>();var mantle=clone.GetComponent<PlayerCharacterMantle>();
                Assert.That(grenade.Remaining,Is.EqualTo(3));Assert.That(mantle,Is.Not.Null);
                Assert.That(grenade.TryEquip(),Is.True,"Serialized dependencies must rebuild during Awake without editor Initialize.");
                Assert.That(grenade.IsEquipped,Is.True);Assert.That(grenade.IsThrowing,Is.False);
                grenade.enabled=false;Assert.That(grenade.IsThrowing,Is.False);Assert.That(grenade.Remaining,Is.EqualTo(3));
            }
            finally{if(clone!=null)Object.Destroy(clone);Object.Destroy(floor);}
        }
        private sealed class GrenadeInput:IPlayerCharacterThrowInput,IWeaponInput
        {
            public bool EquipGrenadePressed{get;set;}
            public bool ThrowGrenadePressed{get;set;}
            public bool ThrowGrenadeReleased{get;set;}
            public bool ThrowGrenadeHeld=>PrimaryHeld;
            public bool CanControlGrenade{get;set;}=true;
            public bool ToggleGrenadeThrowModePressed{get;set;}
            public int WeaponSlotSelection{get;set;}
            public bool PrimaryHeld{get;set;}
            private bool _consumed;
            public bool FireWeapon=>PrimaryHeld&&!_consumed;
            public void ConsumePrimaryUntilRelease()=>_consumed=true;
            public void ConsumeAimUntilRelease(){}
            public void PressPrimary(){PrimaryHeld=ThrowGrenadePressed=true;ThrowGrenadeReleased=false;}
            public void ReleasePrimary(){PrimaryHeld=false;ThrowGrenadePressed=false;ThrowGrenadeReleased=true;_consumed=false;}
        }
        [UnityTest]public IEnumerator GrenadeEquipmentWaitsForClickAndReturnsToThePreviousWeapon()
        {
            foreach(bool firstPerson in new[]{false,true})
            {
                yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);
                var grenade=c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();var input=new GrenadeInput();CharacterViewTestContext.SetField(grenade,"_input",input);
                var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
                try
                {
                    for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                    if(c.View.IsFirstPersonView!=firstPerson){c.TogglePerspective();for(int n=0;n<15;n++)yield return null;}
                    int count=grenade.Remaining;int slot=c.Actor.GetComponent<PlayerCharacterLoadout>().CurrentSlot;
                    input.EquipGrenadePressed=true;yield return null;input.EquipGrenadePressed=false;
                    for(int n=0;n<220;n++)yield return null;
                    Assert.That(grenade.IsEquipped,Is.True);Assert.That(grenade.IsAiming,Is.False);Assert.That(grenade.IsThrowing,Is.False);
                    Assert.That(grenade.Remaining,Is.EqualTo(count));Assert.That(grenade.Held,Is.Not.Null);Assert.That(grenade.Held.IsArmed,Is.False);
                    var presentation=c.Actor.GetComponentInChildren<PlayerCharacterCombatView>();
                    Assert.That(presentation.ActiveWeapon.gameObject.activeSelf,Is.False);
                    Assert.That(c.Actor.GetComponent<PlayerCharacterActionPolicy>().CanAttack,Is.False);
                    Assert.That(c.Actor.GetComponent<PlayerCharacterActionPolicy>().CanJump,Is.True);
                    Assert.That(c.Actor.GetComponent<PlayerCharacterActionPolicy>().CanChangeStance,Is.True);
                    c.TapAim();Assert.That(c.View.IsAds||c.View.IsAiming,Is.False,"Grenade equipment must not enter weapon ADS.");
                    if(firstPerson)
                    {
                        var visible=c.Camera.WorldToViewportPoint(grenade.Held.GetComponentInChildren<Renderer>().bounds.center);
                        Assert.That(visible.z,Is.GreaterThan(.05f));Assert.That(visible.x,Is.InRange(0f,1f));Assert.That(visible.y,Is.InRange(0f,1f));
                    }
                    input.PressPrimary();yield return null;input.ThrowGrenadePressed=false;
                    for(int n=0;n<180;n++)yield return null;
                    Assert.That(grenade.IsAiming,Is.True);Assert.That(grenade.IsThrowing,Is.False);
                    Assert.That(grenade.Remaining,Is.EqualTo(count));Assert.That(grenade.Held.IsArmed,Is.False,"Holding aim must not cook the grenade.");
                    var line=c.Actor.GetComponent<LineRenderer>();Assert.That(line.enabled,Is.True);Assert.That(line.positionCount,Is.GreaterThan(2));
                    Assert.That(grenade.TryGetAimTrajectory(out Vector3 origin,out Vector3 velocity),Is.True);
                    Assert.That(Vector3.Distance(line.GetPosition(0),origin),Is.LessThan(.001f));
                    float step=Time.fixedDeltaTime;
                    Assert.That(Vector3.Distance(line.GetPosition(1),origin+(velocity+Physics.gravity*step)*step),Is.LessThan(.001f));
                    input.ReleasePrimary();yield return null;yield return null;
                    Assert.That(grenade.IsThrowing,Is.False);Assert.That(line.enabled,Is.False);
                    Assert.That(grenade.Remaining,Is.EqualTo(count-1),"Release must launch immediately without a motion timer.");
                    for(int n=0;n<30;n++)yield return null;
                    Assert.That(grenade.IsEquipped,Is.False);Assert.That(grenade.Remaining,Is.EqualTo(count-1));
                    Assert.That(Object.FindObjectsByType<GrenadeProjectile>().Count(x=>x.IsArmed),Is.EqualTo(1));
                    Assert.That(c.Actor.GetComponent<PlayerCharacterLoadout>().CurrentSlot,Is.EqualTo(slot));
                    Assert.That(presentation.ActiveWeapon.gameObject.activeSelf,Is.True);
                }
                finally{Time.captureDeltaTime=old;Object.Destroy(floor);}
            }
        }
        [UnityTest]public IEnumerator GrenadeSelectionCanBeCancelledByEveryWeaponSlotWithoutConsuming()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);
            var grenade=c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();var input=new GrenadeInput();
            CharacterViewTestContext.SetField(grenade,"_input",input);CharacterViewTestContext.SetField(c.Shooter,"_input",input);
            var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));
            float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
            try
            {
                for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                int count=grenade.Remaining;
                foreach(int slot in new[]{1,2,3})
                {
                    Assert.That(grenade.TryEquip(),Is.True);for(int n=0;n<100;n++)yield return null;
                    input.WeaponSlotSelection=slot;yield return null;input.WeaponSlotSelection=0;yield return null;
                    Assert.That(grenade.IsEquipped,Is.False);Assert.That(grenade.Held==null,Is.True);Assert.That(grenade.Remaining,Is.EqualTo(count));
                    Assert.That(c.Actor.GetComponent<PlayerCharacterLoadout>().CurrentSlot,Is.EqualTo(slot));
                }
                Assert.That(grenade.TryEquip(),Is.True);c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(2);yield return null;
                Assert.That(grenade.IsEquipped,Is.False,"External loadout changes must cancel grenade equipment too.");
                Assert.That(grenade.Remaining,Is.EqualTo(count));
            }
            finally{Time.captureDeltaTime=old;Object.Destroy(floor);}
        }
        [UnityTest]public IEnumerator GrenadeClickCannotFireTheGunAfterReturningButANewClickCan()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);
            var grenade=c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();var input=new GrenadeInput{PrimaryHeld=true};
            CharacterViewTestContext.SetField(grenade,"_input",input);CharacterViewTestContext.SetField(c.Shooter,"_input",input);
            int shots=0;c.Shooter.Fired+=()=>shots++;
            var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
            try
            {
                input.PrimaryHeld=false;for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                input.PrimaryHeld=true;Assert.That(grenade.TryEquip(),Is.True);
                for(int n=0;n<120;n++)yield return null;
                Assert.That(grenade.IsThrowing,Is.False,"A mouse button held before equipment must not start a throw.");Assert.That(shots,Is.Zero);
                input.ReleasePrimary();input.PressPrimary();yield return null;input.ThrowGrenadePressed=false;
                for(int n=0;n<10;n++)yield return null;
                Assert.That(grenade.IsAiming,Is.True);input.ReleasePrimary();yield return null;
                for(int n=0;n<220;n++)yield return null;
                Assert.That(grenade.IsEquipped,Is.False);Assert.That(shots,Is.Zero,"The grenade click must not spill into gun fire.");
                input.ReleasePrimary();input.PrimaryHeld=true;yield return null;yield return null;
                Assert.That(shots,Is.GreaterThan(0),"A fresh click after returning must fire normally.");
            }
            finally{Time.captureDeltaTime=old;Object.Destroy(floor);}
        }
        [UnityTest]public IEnumerator GrenadeReleaseIsImmediateOnceAndThinCoverCancelsRelease()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);var grenade=c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();
            var input=new GrenadeInput();CharacterViewTestContext.SetField(grenade,"_input",input);
            var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));GameObject wall=null;float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
            try
            {
                for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                int count=grenade.Remaining;Assert.That(grenade.TryThrow(),Is.False,"Throw requires equipped grenade.");
                Assert.That(grenade.TryEquip(),Is.True);input.PressPrimary();yield return null;input.ThrowGrenadePressed=false;
                Assert.That(grenade.IsAiming,Is.True);Assert.That(grenade.IsThrowing,Is.False);
                input.ReleasePrimary();yield return null;Assert.That(grenade.IsEquipped,Is.False);Assert.That(grenade.TryThrow(),Is.False);
                Assert.That(Object.FindObjectsByType<GrenadeProjectile>().Count(x=>x.IsArmed),Is.EqualTo(1));
                Assert.That(grenade.IsEquipped,Is.False);Assert.That(grenade.Remaining,Is.EqualTo(count-1));
                foreach(var projectile in Object.FindObjectsByType<GrenadeProjectile>())Object.Destroy(projectile.gameObject);
                wall=Box(c.Actor.position+Vector3.up*1.2f+Vector3.forward*.3f,new Vector3(3,3,.05f));
                yield return null;
                Assert.That(grenade.TryEquip(),Is.True);Assert.That(grenade.TryThrow(),Is.False);yield return null;
                Assert.That(grenade.IsEquipped,Is.False);Assert.That(grenade.Remaining,Is.EqualTo(count-1));
                Assert.That(Object.FindObjectsByType<GrenadeProjectile>().Count(x=>x.IsArmed),Is.Zero);
            }
            finally{Time.captureDeltaTime=old;Object.Destroy(floor);if(wall!=null)Object.Destroy(wall);}
        }
        [UnityTest]public IEnumerator GrenadeAimCancelsOnFourEveryWeaponAndLostControlEvenOnReleaseFrame()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);
            var grenade=c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();var input=new GrenadeInput();
            CharacterViewTestContext.SetField(grenade,"_input",input);CharacterViewTestContext.SetField(c.Shooter,"_input",input);
            int shots=0;c.Shooter.Fired+=()=>shots++;
            var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));
            float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
            try
            {
                for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                int count=grenade.Remaining;
                foreach(bool releaseSameFrame in new[]{false,true})
                foreach(int cancel in new[]{1,2,3,4,5})
                {
                    input.ReleasePrimary();input.EquipGrenadePressed=false;input.WeaponSlotSelection=0;input.CanControlGrenade=true;
                    Assert.That(grenade.TryEquip(),Is.True);
                    for(int n=0;n<40;n++)yield return null;
                    input.PressPrimary();yield return null;input.ThrowGrenadePressed=false;yield return null;
                    Assert.That(grenade.IsAiming,Is.True,"Hold must enter aim.");Assert.That(c.Actor.GetComponent<LineRenderer>().enabled,Is.True,"LateUpdate must show the trajectory.");
                    if(releaseSameFrame)input.ReleasePrimary();
                    if(cancel<=3)input.WeaponSlotSelection=cancel;
                    else if(cancel==4)input.EquipGrenadePressed=true;
                    else input.CanControlGrenade=false;
                    yield return null;
                    Assert.That(grenade.IsEquipped,Is.False);Assert.That(grenade.IsThrowing,Is.False);
                    input.EquipGrenadePressed=false;input.WeaponSlotSelection=0;yield return null;
                    Assert.That(c.Actor.GetComponent<LineRenderer>().enabled,Is.False);
                    Assert.That(grenade.Remaining,Is.EqualTo(count));Assert.That(shots,Is.Zero);
                    input.ReleasePrimary();input.EquipGrenadePressed=false;input.WeaponSlotSelection=0;
                    for(int n=0;n<3;n++)yield return null;
                    Assert.That(Object.FindObjectsByType<GrenadeProjectile>().Count(x=>x.IsArmed),Is.Zero);
                }
            }
            finally{Time.captureDeltaTime=old;Object.Destroy(floor);}
        }
        [UnityTest]public IEnumerator RightClickSwitchesThrowModeAndTheActualLaunchMatchesTheSelectedPreview()
        {
            foreach(bool firstPerson in new[]{false,true})
            {
                yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);
                if(c.View.IsFirstPersonView!=firstPerson)c.TogglePerspective();
                var grenade=c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();var input=new GrenadeInput();
                CharacterViewTestContext.SetField(grenade,"_input",input);
                var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));
                float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
                try
                {
                    for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                    Assert.That(grenade.TryToggleThrowMode(),Is.False);
                    Assert.That(grenade.TryEquip(),Is.True);Assert.That(grenade.ThrowMode,Is.EqualTo(GrenadeThrowMode.Overhand));
                    input.PressPrimary();yield return null;input.ThrowGrenadePressed=false;yield return null;
                    Assert.That(grenade.TryGetAimTrajectory(out Vector3 overOrigin,out Vector3 overVelocity),Is.True);
                    input.ToggleGrenadeThrowModePressed=true;yield return null;input.ToggleGrenadeThrowModePressed=false;yield return null;
                    Assert.That(grenade.ThrowMode,Is.EqualTo(GrenadeThrowMode.Underhand));
                    Assert.That(grenade.TryGetAimTrajectory(out Vector3 underOrigin,out Vector3 underVelocity),Is.True);
                    Assert.That(underOrigin.y,Is.LessThan(overOrigin.y));Assert.That(underVelocity.magnitude,Is.LessThan(overVelocity.magnitude));
                    var line=c.Actor.GetComponent<LineRenderer>();
                    Assert.That(Vector3.Distance(line.GetPosition(0),underOrigin),Is.LessThan(.001f));
                    float step=Time.fixedDeltaTime;
                    Assert.That(Vector3.Distance(line.GetPosition(1),underOrigin+(underVelocity+Physics.gravity*step)*step),Is.LessThan(.001f));
                    var projectile=grenade.Held;int before=grenade.Remaining;
                    input.ReleasePrimary();yield return null;
                    Assert.That(projectile.IsArmed,Is.True);Assert.That(grenade.Remaining,Is.EqualTo(before-1));
                    Assert.That(Vector3.Distance(projectile.GetComponent<Rigidbody>().linearVelocity,underVelocity+Physics.gravity*step),Is.LessThan(.3f));

                    Assert.That(grenade.TryEquip(),Is.True);Assert.That(grenade.ThrowMode,Is.EqualTo(GrenadeThrowMode.Overhand),"A fresh grenade starts overhand.");
                    input.PressPrimary();yield return null;input.ThrowGrenadePressed=false;
                    Assert.That(grenade.TryGetAimTrajectory(out _,out Vector3 expected),Is.True);
                    projectile=grenade.Held;input.ToggleGrenadeThrowModePressed=true;input.ReleasePrimary();yield return null;input.ToggleGrenadeThrowModePressed=false;
                    Assert.That(projectile.IsArmed,Is.True);
                    Assert.That(projectile.GetComponent<Rigidbody>().linearVelocity.magnitude,Is.LessThan(expected.magnitude),"A toggle on the release frame must affect the actual throw.");
                    Assert.That(grenade.TryEquip(),Is.True);input.PressPrimary();yield return null;input.ThrowGrenadePressed=false;
                    before=grenade.Remaining;input.ToggleGrenadeThrowModePressed=input.EquipGrenadePressed=true;input.ReleasePrimary();yield return null;
                    Assert.That(grenade.IsEquipped,Is.False);Assert.That(grenade.Remaining,Is.EqualTo(before),"Cancel wins over toggle and release.");
                    input.ToggleGrenadeThrowModePressed=input.EquipGrenadePressed=false;
                }
                finally{Time.captureDeltaTime=old;Object.Destroy(floor);}
            }
        }
        [UnityTest]public IEnumerator GrenadeTrajectoryStopsAtCoverAndTracksTheCurrentAim()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var motor=Motor(c);
            var grenade=c.Actor.GetComponent<PlayerCharacterGrenadeThrow>();var input=new GrenadeInput();
            CharacterViewTestContext.SetField(grenade,"_input",input);
            var floor=Box(c.Actor.position+Vector3.down*2.25f,new Vector3(100,.5f,100));GameObject wall=null;
            float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
            try
            {
                for(int n=0;n<20;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                Assert.That(grenade.TryEquip(),Is.True);for(int n=0;n<40;n++)yield return null;
                input.PressPrimary();yield return null;input.ThrowGrenadePressed=false;
                Assert.That(grenade.TryGetAimTrajectory(out Vector3 origin,out Vector3 velocity),Is.True);
                wall=Box(origin+Vector3.forward*3f,new Vector3(20,20,.1f));yield return null;
                var line=c.Actor.GetComponent<LineRenderer>();Assert.That(line.enabled,Is.True);
                Vector3 end=line.GetPosition(line.positionCount-1);
                Assert.That(end.z,Is.EqualTo(wall.transform.position.z-.05f-.12f).Within(.03f));
                c.Actor.rotation=Quaternion.Euler(0f,25f,0f);
                // The test view owns the aim rotation; normal mouse look changes this same state.
                CharacterViewTestContext.SetField(c.View,"_pitch",-25f);
                yield return null;
                Assert.That(grenade.TryGetAimTrajectory(out Vector3 nextOrigin,out Vector3 nextVelocity),Is.True);
                Assert.That(Vector3.Distance(velocity,nextVelocity),Is.GreaterThan(1f));
                Assert.That(Vector3.Distance(line.GetPosition(0),nextOrigin),Is.LessThan(.001f));
                grenade.enabled=false;yield return null;Assert.That(line.enabled,Is.False);Assert.That(grenade.Remaining,Is.EqualTo(3));
            }
            finally{Time.captureDeltaTime=old;Object.Destroy(floor);if(wall!=null)Object.Destroy(wall);}
        }
    }
}
#endif
