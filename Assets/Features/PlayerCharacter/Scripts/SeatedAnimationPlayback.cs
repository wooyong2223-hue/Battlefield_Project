using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Battlefield.Features.PlayerCharacter
{
    // Owns a temporary animation graph; seat placement remains with SeatedCharacterPose.
    internal sealed class SeatedAnimationPlayback : IDisposable
    {
        private readonly Animator _animator;
        private readonly bool _wasEnabled, _hadRootMotion;
        private readonly AnimatorCullingMode _cullingMode;
        private PlayableGraph _graph;

        internal bool IsValid => _graph.IsValid();

        internal SeatedAnimationPlayback(Animator animator, AnimationClip clip)
        {
            _animator = animator;
            _wasEnabled = animator.enabled;
            _hadRootMotion = animator.applyRootMotion;
            _cullingMode = animator.cullingMode;
            animator.enabled = true;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            try
            {
                _graph = PlayableGraph.Create("Seated character animation");
                _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var motion = AnimationClipPlayable.Create(_graph, clip);
                motion.SetApplyFootIK(false);
                motion.SetApplyPlayableIK(false);
                var output = AnimationPlayableOutput.Create(_graph, "Seated pose", animator);
                output.SetSourcePlayable(motion);
                _graph.Play();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        internal void Sample(float deltaTime)
        {
            if (!IsValid || _animator == null) return;
            _graph.Evaluate(Mathf.Max(0f, deltaTime * _animator.speed));
        }

        public void Dispose()
        {
            if (_graph.IsValid()) _graph.Destroy();
            _graph = default;
            if (_animator == null) return;
            _animator.applyRootMotion = _hadRootMotion;
            _animator.cullingMode = _cullingMode;
            _animator.enabled = _wasEnabled;
        }
    }
}
