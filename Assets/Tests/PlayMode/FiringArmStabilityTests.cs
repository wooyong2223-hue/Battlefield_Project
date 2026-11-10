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
    public class FiringArmStabilityTests
    {
        private sealed class Idle : IWeaponInput
        { public bool FireWeapon => false; public int WeaponSlotSelection => 0; }

        [UnityTest]
        public IEnumerator RepeatedShotsKeepElbowsStableInBothViewsAndPreserveRecoil()
        {
            yield return CharacterViewTestContext.Load();
            var c = new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter, "_input", new Idle());
            c.Shooter.enabled = true;
            var animator = c.Actor.GetComponentInChildren<Animator>();
            var presentation = animator.GetComponent<PlayerCharacterCombatView>();
            var loadout = c.Actor.GetComponent<PlayerCharacterLoadout>();
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
            floor.transform.localScale = new Vector3(100, .5f, 100);
            Physics.SyncTransforms();
            var motor = c.Actor.GetComponent<PlayerCharacterMovement>();
            CharacterViewTestContext.SetField(motor, "_input", null);
            motor.enabled = true;
            float previousTime = Time.captureDeltaTime;
            Time.captureDeltaTime = .02f;
            try
            {
                float idleMaximum = 0f;
                foreach (string weapon in new[] { "AK47", "M4A1", "Pistol" })
                foreach (bool firstPerson in new[] { false, true })
                foreach (bool firing in new[] { false, true })
                {
                    if (weapon == "Pistol") loadout.TryEquip(PlayerCharacterLoadout.PistolSlot);
                    else RealRifleModelTests.Select(loadout, AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>(
                        "Assets/Features/Weapon/Definitions/" + weapon + ".asset"));
                    if (c.View.IsFirstPersonView != firstPerson) c.TogglePerspective();
                    c.Input.AimHeld = true;
                    c.View.Tick(c.Input, .02f);
                    for (int i = 0; i < 60; i++) yield return null;
                    var right = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    var left = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                    if (firstPerson)
                    {
                        right = presentation.FirstPersonArms.bones.Single(b => b.name == right.name);
                        left = presentation.FirstPersonArms.bones.Single(b => b.name == left.name);
                    }
                    Vector3 previousRight = c.Camera.transform.InverseTransformPoint(right.position);
                    Vector3 previousLeft = c.Camera.transform.InverseTransformPoint(left.position);
                    float maximum = 0f;
                    for (int frame = 0; frame < 50; frame++)
                    {
                        if (firing && frame % 5 == 0)
                        {
                            // Isolate presentation from camera recoil and projectile physics.
                            typeof(PlayerCharacterCombatView).GetMethod("OnFired", BindingFlags.Instance | BindingFlags.NonPublic)
                                .Invoke(presentation, null);
                            Assert.That(presentation.RecoilAmount, Is.EqualTo(1f));
                        }
                        yield return null;
                        Vector3 currentRight = c.Camera.transform.InverseTransformPoint(right.position);
                        Vector3 currentLeft = c.Camera.transform.InverseTransformPoint(left.position);
                        maximum = Mathf.Max(maximum, Vector3.Distance(previousRight, currentRight), Vector3.Distance(previousLeft, currentLeft));
                        previousRight = currentRight; previousLeft = currentLeft;
                    }
                    Assert.That(maximum, Is.LessThan(firstPerson ? .025f : .08f), weapon + " elbow step FP=" + firstPerson);
                    if (!firing) idleMaximum = maximum;
                    else Assert.That(maximum, Is.LessThan(idleMaximum + .025f), weapon + " firing must not introduce an elbow snap");
                    Assert.That(animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("UpperCombat")).IsName("UpperCombat.PistolHold"), Is.True);
                    
                    
                    yield return new WaitForSeconds(.5f);
                    Assert.That(presentation.RecoilAmount, Is.Zero);
                }
            }
            finally { Time.captureDeltaTime = previousTime; }
        }
    }
}
#endif
