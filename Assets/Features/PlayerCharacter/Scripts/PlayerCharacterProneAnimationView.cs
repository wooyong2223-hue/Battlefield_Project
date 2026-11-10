using Battlefield.Features.Weapon;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Battlefield.Features.PlayerCharacter
{
    // Full-body authored prone playback. Movement, stance, damage and ammo remain with gameplay.
    [DefaultExecutionOrder(42)]
    public sealed class PlayerCharacterProneAnimationView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerCharacterMovement _movement;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterController _combat;
        [SerializeField] private InfantryLifeState _life;
        [SerializeField] private ProneAnimationSet _animations;
        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer, _meleeMixer;
        private AnimationLayerMixerPlayable _layers;
        private readonly AnimationClipPlayable[] _meleeClips = new AnimationClipPlayable[3];
        private readonly AnimationClip[] _meleeSources = new AnimationClip[3];
        private float _meleeRemaining, _meleeDuration, _meleeBlend;
        private int _meleeSlot;
        private bool _cross;
        private readonly AnimationClip[] _sourceClips = new AnimationClip[10];
        private readonly AnimationClipPlayable[] _clips = new AnimationClipPlayable[10];
        private readonly float[] _weights = new float[10];
        private readonly float[] _targets = new float[10];
        private float _elapsed, _shotTime, _deathTime, _reloadTime;
        private IWeaponMagazine _magazine;
        private bool _entering, _exiting, _wasEliminated, _subscribed;
        private AnimatorCullingMode _previousCulling;
        private bool CanPresent => _animator != null && _animator.isHuman && _movement != null && _stance != null && _animations != null && _animations.IsValid;
        public bool IsPresenting => isActiveAndEnabled && CanPresent &&
            (_graph.IsValid() || (_stance != null && _stance.IsProne));
        public float MeleePoseWeight => IsPresenting ? _meleeBlend : 0f;
        public bool RetargetStandingMelee => _animations != null && _animations.RetargetStandingMelee;
        public bool IsReloading => IsPresenting && _magazine != null && _magazine.IsReloading;
        // Firing keeps the calibrated grip; only actions that release the weapon relinquish it.
        public bool CanUseItemGrip => IsPresenting && !_entering && !_exiting && !_wasEliminated && !IsReloading && _meleeBlend <= .001f && (_combat == null || !_combat.IsAttacking);
        public ProneAnimationSet Animations => _animations;

        public void Initialize(Animator animator, PlayerCharacterMovement movement, PlayerCharacterStance stance,
            PlayerCharacterController combat, InfantryLifeState life, ProneAnimationSet animations)
        {
            Unsubscribe(); Release();
            _animator = animator; _movement = movement; _stance = stance; _combat = combat; _life = life; _animations = animations;
            Subscribe();
        }
        private void OnEnable() => Subscribe();
        private void Subscribe()
        {
            if (!isActiveAndEnabled || _combat == null || _subscribed) return;
            _combat.Fired += Fired; _combat.MeleeStarted += MeleeStarted; _combat.EquipmentChanged += EquipmentChanged; _subscribed = true; BindMagazine();
        }
        private void Unsubscribe()
        {
            if (_subscribed && _combat != null) { _combat.Fired -= Fired; _combat.MeleeStarted -= MeleeStarted; _combat.EquipmentChanged -= EquipmentChanged; }
            if (_magazine != null) _magazine.ReloadStarted -= ReloadStarted;
            _magazine = null;
            _subscribed = false;
        }
        private void EquipmentChanged(bool armed) => BindMagazine();
        private void BindMagazine()
        {
            if (_magazine != null) _magazine.ReloadStarted -= ReloadStarted;
            _magazine = _combat.Magazine;
            if (_magazine != null) _magazine.ReloadStarted += ReloadStarted;
        }
        private void ReloadStarted() => _reloadTime = 0f;
        private void MeleeStarted(bool armed)
        {
            if (!IsPresenting) return;
            _cross = !_cross; _meleeSlot = armed ? 0 : _cross ? 1 : 2;
            _meleeDuration = _combat.MeleeDuration; _meleeRemaining = _meleeDuration;
        }
        private void Fired() { if (IsPresenting) _shotTime = _combat.ShotInterval; }
        private void Begin()
        {
            _previousCulling = _animator.cullingMode;
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            _graph = PlayableGraph.Create("Authored prone animations");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            _mixer = AnimationMixerPlayable.Create(_graph, _clips.Length);
            for (int i = 0; i < _clips.Length; i++)
            {
                _weights[i] = 0f;
                var clip = _sourceClips[i] = _animations.GetClip(i, _combat != null && _combat.IsArmed); if (clip == null) continue;
                _clips[i] = AnimationClipPlayable.Create(_graph, clip);
                _clips[i].SetApplyFootIK(false); _clips[i].SetApplyPlayableIK(false);
                _graph.Connect(_clips[i], 0, _mixer, i);
            }
            var output = AnimationPlayableOutput.Create(_graph, "Full body prone", _animator);
            _layers = AnimationLayerMixerPlayable.Create(_graph, 2);
            _graph.Connect(_mixer, 0, _layers, 0); _layers.SetInputWeight(0, 1f);
            _meleeMixer = AnimationMixerPlayable.Create(_graph, 3);
            _meleeSources[0] = _animations.MeleeWeapon; _meleeSources[1] = _animations.MeleeJab; _meleeSources[2] = _animations.MeleeCross;
            for (int i = 0; i < _meleeClips.Length; i++)
            {
                if (_meleeSources[i] == null) continue;
                _meleeClips[i] = AnimationClipPlayable.Create(_graph, _meleeSources[i]);
                _meleeClips[i].SetApplyFootIK(false); _meleeClips[i].SetApplyPlayableIK(false);
                _graph.Connect(_meleeClips[i], 0, _meleeMixer, i);
            }
            _graph.Connect(_meleeMixer, 0, _layers, 1);
            if (_animations.MeleeMask != null) _layers.SetLayerMaskFromAvatarMask(1, _animations.MeleeMask);
            output.SetSourcePlayable(_layers); _graph.Play();
            _entering = _sourceClips[5] != null; _exiting = _wasEliminated = false;
            _elapsed = _shotTime = _deathTime = 0f;
            _weights[_entering ? 5 : 0] = 1f;
        }
        private void LateUpdate()
        {
            if (!CanPresent) return;
            if (!_graph.IsValid()) { if (!_stance.IsProne) return; Begin(); }
            float dt = Time.deltaTime; _elapsed += dt;
            if (_magazine != null && _magazine.IsReloading) _reloadTime += dt;
            bool eliminated = _life != null && _life.IsEliminated;
            if (eliminated && !_wasEliminated) _deathTime = 0f;
            _wasEliminated = eliminated; if (eliminated) _deathTime += dt;
            if (!_stance.IsProne && !_exiting) { _exiting = true; _entering = false; }
            else if (_stance.IsProne && _exiting) { _exiting = false; _entering = _sourceClips[5] != null; }
            if (_exiting && (_sourceClips[6] == null || !_stance.IsProneTransitioning))
            { Release(); return; }
            System.Array.Clear(_targets, 0, _targets.Length);
            if (eliminated && _sourceClips[9] != null) _targets[9] = 1f;
            else if (_exiting) _targets[6] = 1f;
            else if (_entering && _stance.IsProneTransitioning) _targets[5] = 1f;
            else
            {
                _entering = false;
                var magazine = _combat != null ? _combat.Magazine : null;
                if (magazine != null && magazine.IsReloading && _sourceClips[8] != null) _targets[8] = 1f;
                else if (_shotTime > 0f && _sourceClips[7] != null) _targets[7] = 1f;
                else
                {
                    Vector3 velocity = _movement.transform.InverseTransformDirection(_movement.Velocity);
                    float speed = new Vector2(velocity.x, velocity.z).magnitude;
                    float moving = Mathf.Clamp01(speed / Mathf.Max(.01f, _stance.MoveSpeed));
                    float sum = Mathf.Abs(velocity.x) + Mathf.Abs(velocity.z);
                    _targets[0] = 1f - moving;
                    if (sum > .01f)
                    {
                        _targets[velocity.z >= 0f ? 1 : 2] += moving * Mathf.Abs(velocity.z) / sum;
                        _targets[velocity.x >= 0f ? 4 : 3] += moving * Mathf.Abs(velocity.x) / sum;
                    }
                    for (int i = 1; i <= 4; i++) if (_sourceClips[i] == null) { _targets[0] += _targets[i]; _targets[i] = 0f; }
                }
            }
            float total = 0f;
            for (int i = 0; i < _clips.Length; i++)
            { _weights[i] = Mathf.MoveTowards(_weights[i], _targets[i], dt * 12.5f); total += _weights[i]; }
            for (int i = 0; i < _clips.Length; i++)
            {
                var clip = _sourceClips[i]; if (clip == null) continue;
                float rate = i > 0 && i < 5 ? _animations.CrawlPlaybackSpeed : 1f;
                float time = i < 5 ? Mathf.Repeat(_elapsed * rate, clip.length) : 0f;
                if (i == 5 || i == 6) time = _stance.ProneTransitionProgress * clip.length;
                if (i == 7) time = (1f - _shotTime / Mathf.Max(.01f, _combat.ShotInterval)) * clip.length;
                if (i == 8 && _combat.Magazine != null)
                {
                    // Normalized reload progress is presentation-only; the magazine owns completion.
                    time = Mathf.Clamp01(_reloadTime / _combat.Magazine.ReloadDuration) * clip.length;
                }
                if (i == 9) time = Mathf.Min(_deathTime, clip.length);
                _clips[i].SetTime(time); _mixer.SetInputWeight(i, total > 0f ? _weights[i] / total : 0f);
            }
            bool meleeAllowed = !eliminated && _combat != null && _combat.CanUseCombat && _animations.MeleeMask != null && _meleeSources[_meleeSlot] != null;
            if (!meleeAllowed) { _meleeRemaining = 0f; _meleeBlend = 0f; }
            else _meleeBlend = Mathf.MoveTowards(_meleeBlend, _meleeRemaining > 0f ? 1f : 0f, dt * 12.5f);
            for (int i = 0; i < _meleeClips.Length; i++)
            {
                if (_meleeSources[i] == null) continue;
                _meleeMixer.SetInputWeight(i, i == _meleeSlot ? 1f : 0f);
                _meleeClips[i].SetTime(Mathf.Clamp01(1f - _meleeRemaining / Mathf.Max(.01f, _meleeDuration)) * _meleeSources[i].length);
            }
            _layers.SetInputWeight(1, _meleeBlend);
            _graph.Evaluate(0f); _shotTime = Mathf.Max(0f, _shotTime - dt); _meleeRemaining = Mathf.Max(0f, _meleeRemaining - dt);
        }
        private void Release()
        {
            if (_graph.IsValid()) { _graph.Destroy(); if (_animator != null) _animator.cullingMode = _previousCulling; }
            _graph = default; _entering = _exiting = false; _meleeRemaining = _meleeBlend = 0f;
        }
        private void OnDisable() { Unsubscribe(); Release(); }
        private void OnDestroy() { Unsubscribe(); Release(); }
    }
}
