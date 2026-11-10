#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode
{
    public sealed class MixamoArmedLocomotionTests
    {
        const string ProfilePath = "Assets/ThirdParty/MixamoLocal/ArmedLocomotion/ArmedLocomotion.asset";
        sealed class Idle : IWeaponInput { public bool FireWeapon => false; public int WeaponSlotSelection => 0; }
        static readonly Vector2[] Directions = {Vector2.up,new Vector2(1,1).normalized,Vector2.right,new Vector2(1,-1).normalized,
            Vector2.down,new Vector2(-1,-1).normalized,Vector2.left,new Vector2(-1,1).normalized};
        [Test]
        public void LocalSourcesKeepCompleteHumanoidTakesWithBodyBasedRotation()
        {
            const string folder="Assets/ThirdParty/MixamoLocal/ArmedLocomotion/Sources";
            if(!AssetDatabase.IsValidFolder(folder))Assert.Ignore("Optional local source is not installed.");
            var paths=AssetDatabase.FindAssets("t:Model",new[]{folder}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            Assert.That(paths.Length,Is.EqualTo(16));
            foreach(string path in paths)
            {
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);Assert.That(importer.animationType,Is.EqualTo(ModelImporterAnimationType.Human));
                Assert.That(importer.clipAnimations.Length,Is.EqualTo(1));var clip=importer.clipAnimations[0];var take=importer.defaultClipAnimations[0];
                Assert.That(clip.firstFrame,Is.EqualTo(take.firstFrame));Assert.That(clip.lastFrame,Is.EqualTo(take.lastFrame));
                Assert.That(clip.loopTime,Is.True);Assert.That(clip.lockRootRotation,Is.True);Assert.That(clip.keepOriginalOrientation,Is.False);
            }
        }

        [UnityTest]
        public IEnumerator MovementOverrideTemplatesPreserveEquipmentAndRestoreTheOriginalProfile()
        {
            var profile=AssetDatabase.LoadAssetAtPath<ArmedLocomotionAnimationSet>(ProfilePath);
            if(profile==null)Assert.Ignore("Optional local source is not installed.");
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter,"_input",new Idle());c.Shooter.enabled=true;
            c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(1);
            var a=c.Actor.GetComponentInChildren<Animator>();var presentation=a.GetComponent<PlayerCharacterCombatView>();
            var equipment=AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/AK47.asset").Animations;
            var movement=new AnimatorOverrideController(profile.Find(equipment));
            var walk=AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/MixamoLocal/ArmedLocomotion/Sources/Basic Shooter Pack/walking.fbx").OfType<AnimationClip>().Single(v=>!v.name.StartsWith("__"));
            var alternative=AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/MixamoLocal/ArmedLocomotion/Sources/Pistol_Handgun Locomotion Pack/pistol walk.fbx").OfType<AnimationClip>().Single(v=>!v.name.StartsWith("__"));
            movement[walk]=alternative;var temporary=ScriptableObject.CreateInstance<ArmedLocomotionAnimationSet>();
            var data=new SerializedObject(temporary);var entries=data.FindProperty("_entries");entries.arraySize=1;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("_equipment").objectReferenceValue=equipment;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("_movement").objectReferenceValue=movement;data.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                presentation.SetLocomotionAnimations(temporary);yield return null;
                var current=(AnimatorOverrideController)a.runtimeAnimatorController;
                Assert.That(current[walk],Is.SameAs(alternative),"The movement template's overrides must survive composition.");
                Assert.That(current.animationClips,Does.Contain(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/ThirdParty/KayKit/Rifle/RifleHold.anim")),"Existing equipment grip overrides must survive.");
                presentation.SetLocomotionAnimations(profile);yield return null;
                Assert.That(((AnimatorOverrideController)a.runtimeAnimatorController)[walk],Is.SameAs(walk));
            }
            finally{presentation.SetLocomotionAnimations(profile);Object.Destroy(temporary);Object.Destroy(movement);}
        }

        [UnityTest]
        public IEnumerator ArmedWalkAndRunUseAllEightOriginalDirectionsAndKeepEquipmentGrips()
        {
            if(AssetDatabase.LoadAssetAtPath<ArmedLocomotionAnimationSet>(ProfilePath)==null) Assert.Ignore("Optional local source is not installed.");
            yield return CharacterViewTestContext.Load(); var c=new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter,"_input",new Idle()); c.Shooter.enabled=true;
            var motor=c.Actor.GetComponent<PlayerCharacterMovement>();CharacterViewTestContext.SetField(motor,"_input",null);motor.enabled=true;
            var a=c.Actor.GetComponentInChildren<Animator>(); var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();
            var presentation=a.GetComponent<PlayerCharacterCombatView>();
            CharacterViewTestContext.SetField(a.GetComponent<PlayerCharacterAnimationView>(),"_speedDamping",0f);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=c.Actor.position+Vector3.down*2.25f;floor.transform.localScale=new Vector3(1000,.5f,1000);Physics.SyncTransforms();
            float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
            try
            {
                foreach(int slot in new[]{1,2})
                {
                    loadout.TryEquip(slot);c.Input.AimHeld=true;c.View.Tick(c.Input,.02f);
                    for(int n=0;n<30;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
                    Quaternion rotation=a.transform.localRotation;Vector3 local=a.transform.localPosition;
                    foreach(bool walk in new[]{true,false}) for(int i=0;i<Directions.Length;i++)
                    {
                        for(int n=0;n<40;n++){motor.Tick(Directions[i]*(walk?.5f:1f),false,false,.02f);yield return null;}
                        string[] cardinal=slot==1 ? walk?new[]{"walking","walking backwards","strafe left","strafe right"}:new[]{"rifle run","run backwards","strafe (2)","strafe"}
                            :walk?new[]{"pistol walk","pistol walk backward","pistol strafe","pistol strafe (2)"}:new[]{"pistol run","pistol run backward","pistol strafe","pistol strafe (2)"};
                        Vector2 direction=Directions[i];var clips=a.GetCurrentAnimatorClipInfo(0);
                        var expected=new System.Collections.Generic.List<string>();
                        if(direction.y>.1f)expected.Add(cardinal[0]);if(direction.y<-.1f)expected.Add(cardinal[1]);if(direction.x<-.1f)expected.Add(cardinal[2]);if(direction.x>.1f)expected.Add(cardinal[3]);
                        foreach(string clip in expected)Assert.That(clips.Any(v=>v.clip.name==clip&&v.weight>(expected.Count==1?.85f:.15f)),Is.True,slot+" "+clip+": "+string.Join(",",clips.Select(v=>v.clip.name+"="+v.weight)));
                        Assert.That(a.applyRootMotion,Is.False);Assert.That(Vector3.Distance(local,a.transform.localPosition),Is.LessThan(.001f));
                        Assert.That(Quaternion.Angle(rotation,a.transform.localRotation),Is.LessThan(.01f));
                        Assert.That(presentation.UsesItemGrip,Is.True);
                        Assert.That(Vector3.Angle(c.Shooter.Muzzle.forward,c.View.AimRotation*Vector3.forward),Is.LessThan(2f));
                        if(walk && i==0) Debug.Log("ARMED_LOCOMOTION slot="+slot+" original="+string.Join("+",expected));
                    }
                    Assert.That(c.Shooter.Magazine.TryConsumeRound(),Is.True);Assert.That(c.Shooter.Magazine.TryReload(),Is.True);
                    yield return new WaitForSeconds(.25f);
                    Assert.That(a.GetCurrentAnimatorStateInfo(a.GetLayerIndex("ReloadUpperBody")).IsName("ReloadUpperBody.PistolReload"),Is.True);
                    c.Shooter.Magazine.CancelReload();yield return new WaitForSeconds(.3f);
                }
                loadout.TryEquip(3);for(int i=0;i<30;i++){motor.Tick(Vector2.up,false,false,.02f);yield return null;}
                Assert.That(a.GetCurrentAnimatorClipInfo(0).Any(v=>v.clip.name=="Jog_Fwd_Loop" && v.weight>.9f),Is.True,"Unarmed locomotion must retain the existing controller.");
                presentation.SetLocomotionAnimations(null);loadout.TryEquip(2);yield return null;
                Assert.That(a.runtimeAnimatorController.animationClips.Any(v=>AssetDatabase.GetAssetPath(v).Contains("MixamoLocal/ArmedLocomotion")),Is.False,"Missing optional local data must use the current equipment controller.");
            }
            finally {Time.captureDeltaTime=old;Object.Destroy(floor);}
        }
    }
}
#endif
