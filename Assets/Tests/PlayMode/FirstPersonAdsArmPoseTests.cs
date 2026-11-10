#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode
{
    public class FirstPersonAdsArmPoseTests
    {
        private sealed class Idle:IWeaponInput{public bool FireWeapon=>false;public int WeaponSlotSelection=>0;}
        [UnityTest]
        public IEnumerator AdsKeepsTheRightElbowBelowTheSightAndBothHandsOnTheWeapon()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter,"_input",new Idle());c.Shooter.enabled=true;
            var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();var a=c.Actor.GetComponentInChildren<Animator>();
            var view=a.GetComponent<PlayerCharacterCombatView>();var rig=c.Actor.GetComponentInChildren<FirstPersonArmsView>();
            if(!c.View.IsFirstPersonView)c.TogglePerspective();
            foreach(string name in new[]{"AK47","M4A1"})
            {
                loadout.TryEquip(3);loadout.GetEquipment(1).TryEquip(AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/"+name+".asset"));loadout.TryEquip(1);
                c.TapAim();yield return new WaitForSeconds(.5f);
                foreach(float look in new[]{0f,40f,-80f,40f})
                {
                    c.Input.Look=new Vector2(0,look);c.View.Tick(c.Input,.02f);c.Input.Look=Vector2.zero;yield return new WaitForSeconds(.2f);
                    var upper=rig.FirstBone(view,a,HumanBodyBones.RightUpperArm);
                    var elbow=rig.FirstBone(view,a,HumanBodyBones.RightLowerArm);
                    Assert.That(c.Camera.transform.InverseTransformPoint(upper.position).x,Is.GreaterThan(.1f),name+" shoulder must stay on the right");
                    Assert.That(c.Camera.transform.InverseTransformPoint(elbow.position).y,Is.LessThan(-.1f),name+" elbow must stay below the aiming corridor");
                    var left=rig.FirstBone(view,a,HumanBodyBones.LeftHand);
                    Assert.That(Vector3.Distance(left.position,view.ActiveWeapon.SupportHandGrip.position),Is.LessThan(.05f));
                    Assert.That(Vector3.Angle(view.ActiveWeapon.Muzzle.forward,c.View.AimRotation*Vector3.forward),Is.LessThan(5));
                }
                Vector3 sourcePosition=a.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
                Quaternion sourceRotation=a.GetBoneTransform(HumanBodyBones.RightUpperArm).rotation;
                Vector3 eye=c.Camera.transform.position;Quaternion camera=c.Camera.transform.rotation;Vector3 hand=rig.RightHand.position;
                typeof(FirstPersonArmsView).GetMethod("CopyPose",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(rig,null);
                Assert.That(Vector3.Distance(hand,rig.RightHand.position),Is.LessThan(.001f));
                Assert.That(Vector3.Distance(sourcePosition,a.GetBoneTransform(HumanBodyBones.RightUpperArm).position),Is.LessThan(.00001f));
                Assert.That(Quaternion.Angle(sourceRotation,a.GetBoneTransform(HumanBodyBones.RightUpperArm).rotation),Is.LessThan(.001f));
                Assert.That(c.Camera.transform.position,Is.EqualTo(eye));Assert.That(c.Camera.transform.rotation,Is.EqualTo(camera));
                c.Shooter.Magazine.TryConsumeRound();Assert.That(c.Shooter.Magazine.TryReload(),Is.True);yield return new WaitForSeconds(.3f);
                Assert.That(view.UsesItemGrip,Is.False);yield return new WaitForSeconds(1.5f);Assert.That(view.UsesItemGrip,Is.True);
                c.TapAim();yield return new WaitForSeconds(.3f);
            }
        }
    }
    internal static class AdsArmTestBones
    {
        internal static Transform FirstBone(this FirstPersonArmsView rig,PlayerCharacterCombatView view,Animator animator,HumanBodyBones bone)
            =>view.FirstPersonArms.bones.Single(t=>t.name==animator.GetBoneTransform(bone).name);
    }
}
#endif
