using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    [CreateAssetMenu(menuName = "Battlefield/Animation/Prone Animation Set")]
    public sealed class ProneAnimationSet : ScriptableObject
    {
        [SerializeField] private AnimationClip _idle, _forward, _backward, _left, _right;
        [SerializeField] private AnimationClip _enter, _exit, _fire, _reload, _death;
        [SerializeField] private AnimationClip _armedEnter, _armedExit;
        [SerializeField] private AvatarMask _meleeMask;
        [SerializeField] private AnimationClip _meleeWeapon, _meleeJab, _meleeCross;
        [SerializeField] private bool _retargetStandingMelee = true;
        [SerializeField, Min(.01f)] private float _crawlPlaybackSpeed = 1.5f;
        public float CrawlPlaybackSpeed => Mathf.Max(.01f, _crawlPlaybackSpeed);
        public bool RetargetStandingMelee => _retargetStandingMelee;
        public AvatarMask MeleeMask => _meleeMask;
        public AnimationClip MeleeWeapon => _meleeWeapon;
        public AnimationClip MeleeJab => _meleeJab;
        public AnimationClip MeleeCross => _meleeCross;
        public bool IsValid => _idle != null && _idle.humanMotion;
        public AnimationClip GetClip(int slot, bool armed = false) => slot switch
        {
            0 => _idle, 1 => _forward, 2 => _backward, 3 => _left, 4 => _right,
            5 => armed && _armedEnter != null ? _armedEnter : _enter, 6 => armed && _armedExit != null ? _armedExit : _exit, 7 => _fire, 8 => _reload, 9 => _death, _ => null
        };
    }
}
