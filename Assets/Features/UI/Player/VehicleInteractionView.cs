using Battlefield.Features.PlayerCharacter;
using TMPro;
using UnityEngine;

namespace Battlefield.Features.UI
{
    public sealed class VehicleInteractionView : MonoBehaviour
    {
        [SerializeField] private PlayerVehicleInteraction _interaction;
        [SerializeField] private TMP_Text _label;

        private void Awake()
        {
            if (_interaction != null && _label != null) return;
            Debug.LogError("Vehicle interaction view references are missing.", this);
            enabled = false;
        }

        private void LateUpdate()
        {
            string text = _interaction.Occupant.IsSeated
                ? _interaction.ExitBlocked ? "F  하차 위치가 막혀 있습니다" : "F  하차"
                : _interaction.NearbySeat != null ? "F  탑승" : string.Empty;
            if (_label.text != text) _label.text = text;
        }
    }
}
