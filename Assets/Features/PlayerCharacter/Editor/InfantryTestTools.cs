using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Battlefield.Editor.PlayerCharacter
{
    public static class InfantryTestTools
    {
        const string Menu = "Tools/Battlefield/Infantry/";
        static PlayerCharacterController Player => Object.FindAnyObjectByType<PlayerCharacterController>();
        static void Ref(Object target, string field, Object reference)
        { var data = new SerializedObject(target); data.FindProperty(field).objectReferenceValue = reference; data.ApplyModifiedPropertiesWithoutUndo(); }

        [MenuItem(Menu + "Down Player (Play Mode)")]
        static void DownPlayer()
        {
            var health = Player.GetComponent<Health>(); health.TakeDamage(health.MaxHealth);
        }
        [MenuItem(Menu + "Down Player (Play Mode)", true)]
        static bool Available() => Application.isPlaying && Player != null && Player.GetComponent<InfantryLifeState>() != null;
        [MenuItem(Menu + "Spawn Downed Ally (Play Mode)", true)]
        static bool SpawnAvailable() => Available() && Player.CanUseCombat && !Player.GetComponent<PlayerVehicleOccupant>().IsSeated;

        [MenuItem(Menu + "Spawn Downed Ally (Play Mode)")]
        static void SpawnAlly()
        {
            var player = Player;
            var source = player.GetComponentInChildren<Animator>();
            var root = new GameObject("Infantry_TrainingAlly"); root.SetActive(false);
            root.transform.SetPositionAndRotation(player.transform.position + player.transform.right * 2.25f, player.transform.rotation);
            var body = root.AddComponent<CharacterController>();
            var sourceBody = player.GetComponent<CharacterController>(); var sourceStance = player.GetComponent<PlayerCharacterStance>();
            body.height = sourceStance.StandingHeight; body.radius = sourceBody.radius; body.center = sourceStance.StandingCenter;
            body.skinWidth = sourceBody.skinWidth; body.stepOffset = sourceBody.stepOffset;
            var health = root.AddComponent<Health>(); var team = root.AddComponent<Team>();
            var teamData = new SerializedObject(team); teamData.FindProperty("_team").intValue = (int)player.GetComponent<Team>().CurrentTeam; teamData.ApplyModifiedPropertiesWithoutUndo();
            var stance = root.AddComponent<PlayerCharacterStance>(); var movement = root.AddComponent<PlayerCharacterMovement>();
            var life = root.AddComponent<InfantryLifeState>(); Ref(movement, "_stance", stance); Ref(life, "_stance", stance); Ref(life, "_movement", movement);
            var model = Object.Instantiate(source.gameObject, root.transform, false);
            foreach (var weapon in model.GetComponentsInChildren<WeaponModel>(true)) Object.DestroyImmediate(weapon.gameObject);
            foreach (var component in model.GetComponentsInChildren<MonoBehaviour>(true))
            {
                // No copied component may mutate a serialized reference back to the original player on destruction.
                var data = new SerializedObject(component); var combat = data.FindProperty("_combat");
                if (combat != null) { combat.objectReferenceValue = null; data.ApplyModifiedPropertiesWithoutUndo(); }
                Object.DestroyImmediate(component);
            }
            var animator = model.GetComponent<Animator>(); animator.applyRootMotion = false;
            foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            { renderer.enabled = true; renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; }
            var animation = model.AddComponent<PlayerCharacterAnimationView>(); Ref(animation, "_movement", movement); Ref(animation, "_animator", animator); Ref(animation, "_stance", stance); Ref(animation, "_life", life);
            var pose = model.AddComponent<PlayerCharacterPostureView>(); Ref(pose, "_stance", stance); Ref(pose, "_root", root.transform); Ref(pose, "_body", body); Ref(pose, "_life", life);
            model.AddComponent<InfantryTrainingDummy>().Initialize(movement, pose, animator);
            root.SetActive(true); animator.Rebind();
            int combatLayer = animator.GetLayerIndex("UpperCombat");
            if (combatLayer >= 0) animator.SetLayerWeight(combatLayer, 0f);
            health.TakeDamage(health.MaxHealth);
            Selection.activeGameObject = root;
        }
    }
}
