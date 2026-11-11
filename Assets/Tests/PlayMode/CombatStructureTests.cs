#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Training;
using Battlefield.Features.Fighter;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Projectile;
using Battlefield.Features.UI;
using Battlefield.Features.Vehicles;
using Battlefield.Features.VFX;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class CombatStructureTests
    {
        [UnityTest]
        public IEnumerator CombatRangeResolvesEffectPoolsAndHeatHud()
        {
            yield return LoadScene("SampleScene");

            HitEffectPool[] pools = FindInScene<HitEffectPool>();
            Assert.That(pools.Length, Is.EqualTo(5));
            foreach (HitEffectPool pool in pools)
            {
                HitEffect effect = pool.Get();
                Assert.That(effect, Is.Not.Null, pool.name);
                pool.Return(effect);
            }

            HeatDotsView view = FindInScene<HeatDotsView>().Single();
            Assert.That(GetField<WeaponBase>(view, "_weapon"), Is.TypeOf<MachineGun>());
        }

        [UnityTest]
        public IEnumerator MaterialPanelsSpawnParticlesAndBulletHoles()
        {
            yield return LoadScene("SampleScene");
            Transform owner = AlliedDummyShooter();
            owner.SetParent(null, true);
            MachineGun gun = CreateGun(owner, FindInScene<BulletPool>().Single());
            HitEffectDatabase database = FindInScene<HitEffectDatabase>().Single();
            string[] names = { "MaterialTarget_Concreat", "MaterialTarget_Dirt", "MaterialTarget_Metal", "MaterialTarget_Sand", "MaterialTarget_Wood", "Dummy_Enemy", "Dummy_Neutral" };

            for (int i = 0; i < names.Length; i++)
            {
                Collider target = FindInScene<Collider>().Single(collider => collider.name == names[i]);
                Collider surface = target;
                HitEffectDatabase.Entry entry = database.GetEntry(surface.sharedMaterial);
                Assert.That(entry, Is.Not.Null, names[i]);
                int effectsBefore = entry.EffectPool.GetComponentsInChildren<HitEffect>().Length;
                int holesBefore = entry.BulletHolePool.GetComponentsInChildren<BulletHole>().Length;
                Vector3 origin = new Vector3(10000f + i * 100f, 10000f, 10000f);
                owner.SetPositionAndRotation(origin, Quaternion.identity);
                target.transform.SetParent(null, true);
                target.transform.position = origin + Vector3.forward * 20f;
                Physics.SyncTransforms();
                gun.FirePoint.SetPositionAndRotation(
                    owner.GetComponent<Collider>().bounds.ClosestPoint(target.transform.position) + Vector3.forward * 0.2f,
                    Quaternion.identity);

                gun.TryFire();
                yield return new WaitForSeconds(0.15f);

                HitEffect[] effects = entry.EffectPool.GetComponentsInChildren<HitEffect>();
                Assert.That(effects.Length, Is.EqualTo(effectsBefore + 1), names[i] + " hit effect");
                Assert.That(effects.Any(effect => effect.GetComponentsInChildren<ParticleSystem>()
                    .Any(particle => particle.isPlaying)), Is.True, names[i] + " particles");
                BulletHole[] holes = entry.BulletHolePool.GetComponentsInChildren<BulletHole>();
                Assert.That(holes.Length, Is.EqualTo(holesBefore + 1), names[i] + " bullet hole");
                foreach (HitEffect effect in effects) entry.EffectPool.Return(effect);
                foreach (BulletHole hole in holes) entry.BulletHolePool.Return(hole);
            }
        }

        [UnityTest]
        public IEnumerator FailedFireDoesNotDelayNextSuccessfulShot()
        {
            yield return LoadScene("SampleScene");
            Transform owner = AlliedDummyShooter();
            owner.SetParent(null, true);
            owner.SetPositionAndRotation(new Vector3(10000f, 10000f, 10000f), Quaternion.identity);
            MachineGun gun = CreateGun(owner, null);
            int firedCount = 0;
            gun.Fired += () => firedCount++;

            LogAssert.Expect(LogType.Warning, "Machine gun requires a fire point and bullet pool.");
            gun.TryFire();
            gun.TryFire();
            Assert.That(firedCount, Is.Zero);
            Assert.That(gun.Heat, Is.Zero);

            SetField(gun, "_bulletPool", FindInScene<BulletPool>().Single());
            gun.TryFire();
            Assert.That(firedCount, Is.EqualTo(1), "A failed attempt must not consume the shot interval.");
            gun.TryFire();
            Assert.That(firedCount, Is.EqualTo(1), "Successful shots must still respect their interval.");
            yield return new WaitForSeconds(gun.ShotInterval + 0.03f);
            gun.TryFire();
            Assert.That(firedCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator AlliedDummyWeaponDamagesEnemyAndNeutralButNotAlliedDummy()
        {
            yield return LoadScene("SampleScene");
            Transform owner = AlliedDummyShooter();
            owner.SetParent(null, true);
            MachineGun gun = CreateGun(owner, FindInScene<BulletPool>().Single());
            string[] names = { "Dummy_Enemy", "Dummy_Neutral", "Ally_10m" };

            for (int i = 0; i < names.Length; i++)
            {
                Health target = FindInScene<Health>().Single(health => health.name == names[i]);
                Team team = target.GetComponent<Team>();
                if (i == 2)
                {
                    SetField(team, "_team", TeamType.Player);
                }

                Vector3 origin = new Vector3(10000f + i * 100f, 10000f, 10000f);
                owner.SetPositionAndRotation(origin, Quaternion.identity);
                target.transform.SetParent(null, true);
                target.transform.position = origin + Vector3.forward * 20f;
                Physics.SyncTransforms();
                gun.FirePoint.SetPositionAndRotation(
                    owner.GetComponent<Collider>().bounds.ClosestPoint(target.transform.position) + Vector3.forward * 0.2f,
                    Quaternion.identity);
                float previousHealth = target.CurrentHealth;

                gun.TryFire();
                yield return new WaitForSeconds(0.2f);

                float expectedDamage = i == 2 ? 0f : 10f;
                Assert.That(target.CurrentHealth, Is.EqualTo(previousHealth - expectedDamage), names[i]);
            }
        }

        [UnityTest]
        public IEnumerator FighterDeathStopsBoostAndOnlyChangesItsFollowingCamera()
        {
            yield return LoadScene("SampleScene");
            PlayerVehicleOccupant occupant = FindInScene<PlayerVehicleOccupant>().Single();
            VehicleSeat seat = FindInScene<VehicleSeat>().Single();
            CharacterController characterBody = occupant.GetComponent<CharacterController>();
            characterBody.enabled = false;
            occupant.transform.position = GetField<Transform>(seat, "_entryPoint").position;
            characterBody.enabled = true;
            PlayerCharacterInput input = occupant.GetComponent<PlayerCharacterInput>();
            SetField(input, "_captured", true);
            SetField(input, "_captureFrame", -1);
            yield return null;
            Assert.That(input.HasPointerCapture, Is.True);
            Assert.That(seat.TryEnter(occupant), Is.True);

            JetMovement movement = FindInScene<JetMovement>().Single();
            JetCamera camera = FindInScene<JetCamera>().Single();
            Assert.That(camera.IsFollowing(movement.transform), Is.True);
            Assert.That(movement.GetComponent<IWeaponInput>(), Is.Not.Null);
            HeatDotsView view = FindInScene<HeatDotsView>().Single();
            Assert.That(GetField<WeaponBase>(view, "_weapon"), Is.SameAs(movement.GetComponent<MachineGun>()));

            JetCamera unrelatedCamera = CreateUnrelatedCamera(movement.GetComponent<KeyboardJetInput>());
            yield return null;
            Assert.That(unrelatedCamera.IsThirdPersonView, Is.True);

            Rigidbody body = movement.GetComponent<Rigidbody>();
            body.linearVelocity = movement.transform.forward * 50f;
            Afterburner afterburner = movement.GetComponent<Afterburner>();
            afterburner.Tick(true, 0.01f);
            Assert.That(afterburner.IsActive, Is.True);

            Health health = movement.GetComponent<Health>();
            health.TakeDamage(health.MaxHealth);
            Assert.That(afterburner.IsActive, Is.False);
            Assert.That(body.linearVelocity.magnitude, Is.EqualTo(50f).Within(0.01f));
            Assert.That(camera.IsThirdPersonView, Is.False);
            Assert.That(unrelatedCamera.IsThirdPersonView, Is.True);
            yield return null;

            JetEngineVfx vfx = movement.GetComponent<JetEngineVfx>();
            foreach (Transform anchor in GetField<Transform[]>(vfx, "_engineAnchors"))
            {
                foreach (ParticleSystem particle in anchor.GetComponentsInChildren<ParticleSystem>())
                {
                    Assert.That(particle.isEmitting, Is.False, particle.name);
                }
            }
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                $"Assets/Scenes/{sceneName}.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            foreach (DummyShooter shooter in FindInScene<DummyShooter>()) shooter.enabled = false;
            foreach (DummyMissileShooter shooter in FindInScene<DummyMissileShooter>()) shooter.enabled = false;
            foreach (PlayerCharacterCombatController player in FindInScene<PlayerCharacterCombatController>())
            {
                player.enabled = false;
                player.GetComponent<PlayerCharacterMovement>().enabled = false;
            }
            yield return null;
        }

        private static Transform AlliedDummyShooter()
        {
            return FindInScene<DummyShooter>()
                .First(shooter => shooter.GetComponent<Team>().CurrentTeam == TeamType.Player).transform;
        }

        private static MachineGun CreateGun(Transform owner, BulletPool pool)
        {
            MachineGun gun = owner.gameObject.AddComponent<MachineGun>();
            SetField(gun, "_overheat", new Overheat());
            SetField(gun, "_fireRate", 10f);
            SetField(gun, "_damage", 10f);
            SetField(gun, "_bulletPool", pool);
            Transform firePoint = new GameObject("Validation Fire Point").transform;
            firePoint.SetParent(owner, false);
            firePoint.localPosition = Vector3.forward * 2f;
            SetField(gun, "_firePoint", firePoint);
            return gun;
        }

        private static JetCamera CreateUnrelatedCamera(KeyboardJetInput referenceInput)
        {
            GameObject inputObject = new GameObject("Unrelated Camera Input");
            inputObject.SetActive(false);
            KeyboardJetInput input = inputObject.AddComponent<KeyboardJetInput>();
            SetField(input, "_inputActions", GetField<Object>(referenceInput, "_inputActions"));
            Transform point = new GameObject("Camera Point").transform;
            point.SetParent(inputObject.transform, false);
            point.localPosition = new Vector3(0f, 2f, -10f);
            inputObject.SetActive(true);

            GameObject cameraObject = new GameObject("Unrelated Camera");
            cameraObject.SetActive(false);
            JetCamera camera = cameraObject.AddComponent<JetCamera>();
            SetField(camera, "_input", input);
            SetField(camera, "_firstPersonCameraPoint", point);
            SetField(camera, "_thirdPersonCameraPoint", point);
            SetField(camera, "_rearViewCameraPoint", point);
            cameraObject.SetActive(true);
            return camera;
        }

        private static T[] FindInScene<T>() where T : Component
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include)
                .Where(component => component.gameObject.scene == activeScene).ToArray();
        }

        private static T GetField<T>(object target, string name)
        {
            return (T)FindField(target.GetType(), name).GetValue(target);
        }

        private static void SetField(object target, string name, object value)
        {
            FindField(target.GetType(), name).SetValue(target, value);
        }

        private static FieldInfo FindField(System.Type type, string name)
        {
            for (; type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null) return field;
            }

            throw new System.MissingFieldException(name);
        }
    }
}
#endif
