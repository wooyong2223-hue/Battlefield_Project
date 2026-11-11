using System.Collections.Generic;
using Battlefield.Features.Weapon;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    internal sealed class PlayerCharacterWeaponModelPresentation
    {
        private readonly Transform _worldHand;
        private readonly Dictionary<InfantryWeaponDefinition, WeaponModel> _models = new();

        internal PlayerCharacterWeaponModelPresentation(WeaponModel pistolPrefab, Transform worldHand)
        {
            _worldHand = worldHand;
            ActiveWeapon = Pistol = Object.Instantiate(pistolPrefab, worldHand);
        }

        internal WeaponModel Pistol { get; }
        internal WeaponModel ActiveWeapon { get; private set; }
        internal Transform WorldHand => _worldHand;

        internal void Select(bool armed, InfantryWeaponDefinition definition)
        {
            HideAll();
            ActiveWeapon = null;
            if (!armed) return;
            if (definition == null || definition.ModelPrefab == null)
            {
                ActiveWeapon = Pistol;
                return;
            }

            if (!_models.TryGetValue(definition, out var model))
            {
                model = Object.Instantiate(definition.ModelPrefab, _worldHand);
                _models.Add(definition, model);
            }
            ActiveWeapon = model;
        }

        internal void Attach(Transform hand, Vector3 position, Quaternion rotation)
        {
            if (ActiveWeapon == null) return;
            Transform model = ActiveWeapon.transform;
            if (model.parent != hand) model.SetParent(hand, false);
            // The copied hand inherits the view scale; normalize against the stable source scale.
            Vector3 scale = _worldHand.lossyScale;
            model.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
            model.localPosition = position;
            model.localRotation = rotation;
        }

        internal bool TryGetSupportHandPose(out Vector3 offset, out Quaternion rotation)
        {
            offset = Vector3.zero;
            rotation = Quaternion.identity;
            if (ActiveWeapon == null || ActiveWeapon.SupportHandGrip == null || ActiveWeapon.transform.parent == null)
                return false;
            Transform hand = ActiveWeapon.transform.parent;
            offset = hand.InverseTransformPoint(ActiveWeapon.SupportHandGrip.position);
            rotation = Quaternion.Inverse(hand.rotation) * ActiveWeapon.SupportHandGrip.rotation;
            return true;
        }

        internal void SetVisible(bool visible)
        {
            if (ActiveWeapon != null) ActiveWeapon.gameObject.SetActive(visible);
        }

        internal void ApplyRecoil(Vector3 position, Quaternion rotation, float recoil, InfantryWeaponDefinition definition)
        {
            if (ActiveWeapon == null) return;
            bool custom = definition != null && definition.CustomGrip;
            float distance = custom ? definition.RecoilDistance : .02f;
            float degrees = custom ? definition.RecoilDegrees : 5f;
            Transform model = ActiveWeapon.transform;
            model.localPosition = position + rotation * Vector3.back * (recoil * distance);
            model.localRotation = rotation * Quaternion.Euler(-recoil * degrees, 0f, 0f);
        }

        internal void HideAll()
        {
            if (Pistol != null) Pistol.gameObject.SetActive(false);
            foreach (var model in _models.Values)
                if (model != null) model.gameObject.SetActive(false);
        }

        internal void DestroyModels()
        {
            if (Pistol != null) Object.Destroy(Pistol.gameObject);
            foreach (var model in _models.Values)
                if (model != null) Object.Destroy(model.gameObject);
        }
    }
}
