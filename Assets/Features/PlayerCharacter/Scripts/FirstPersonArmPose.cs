using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Retarget the view-only arm chain without moving the animated hand or world skeleton.
    internal sealed class FirstPersonArmPose
    {
        private readonly Transform _upper, _lower, _hand;
        private readonly Transform[] _upperBones, _lowerBones;

        internal FirstPersonArmPose(Animator animator, Transform[] source, Transform[] copies, bool right)
        {
            Transform upper = animator.GetBoneTransform(right ? HumanBodyBones.RightUpperArm : HumanBodyBones.LeftUpperArm);
            Transform lower = animator.GetBoneTransform(right ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm);
            Transform hand = animator.GetBoneTransform(right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
            Transform shoulder = animator.GetBoneTransform(right ? HumanBodyBones.RightShoulder : HumanBodyBones.LeftShoulder);
            _upper = copies[Array.IndexOf(source, upper)];
            _lower = copies[Array.IndexOf(source, lower)];
            _hand = copies[Array.IndexOf(source, hand)];
            var top = new List<Transform>(); var bottom = new List<Transform>();
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == shoulder || (source[i].IsChildOf(upper) && !source[i].IsChildOf(lower))) top.Add(copies[i]);
                else if (source[i].IsChildOf(lower) && !source[i].IsChildOf(hand)) bottom.Add(copies[i]);
            }
            _upperBones = top.ToArray(); _lowerBones = bottom.ToArray();
        }

        internal void Apply(Vector3 shoulder, Vector3 elbowHint, float weight)
        {
            if (weight <= .0001f) return;
            Vector3 oldUpper = _upper.position, oldLower = _lower.position, hand = _hand.position;
            float upperLength = Vector3.Distance(oldUpper, oldLower), lowerLength = Vector3.Distance(oldLower, hand);
            if (upperLength < .0001f || lowerLength < .0001f) return;
            shoulder = Vector3.Lerp(oldUpper, shoulder, weight);
            // Preserve the authored hand even when it would put the new shoulder beyond reach.
            shoulder = hand + Vector3.ClampMagnitude(shoulder - hand, (upperLength + lowerLength) * .995f);
            Vector3 direction = hand - shoulder;
            float distance = Mathf.Max(direction.magnitude, .0001f); direction /= distance;
            if (distance < Mathf.Abs(upperLength - lowerLength) + .0001f)
            {
                distance = Mathf.Abs(upperLength - lowerLength) + .0001f;
                shoulder = hand - direction * distance;
            }
            Vector3 bend = Vector3.ProjectOnPlane(Vector3.Lerp(oldLower, elbowHint, weight) - shoulder, direction);
            if (bend.sqrMagnitude < .000001f) bend = Vector3.ProjectOnPlane(Vector3.down, direction);
            if (bend.sqrMagnitude < .000001f) bend = Vector3.ProjectOnPlane(Vector3.right, direction);
            float along = (upperLength * upperLength - lowerLength * lowerLength + distance * distance) / (2f * distance);
            Vector3 elbow = shoulder + direction * along + bend.normalized *
                Mathf.Sqrt(Mathf.Max(0f, upperLength * upperLength - along * along));
            Quaternion topRotation = Quaternion.FromToRotation(oldLower - oldUpper, elbow - shoulder);
            Quaternion bottomRotation = Quaternion.FromToRotation(hand - oldLower, hand - elbow);
            foreach (Transform bone in _upperBones)
                bone.SetPositionAndRotation(shoulder + topRotation * (bone.position - oldUpper), topRotation * bone.rotation);
            foreach (Transform bone in _lowerBones)
                bone.SetPositionAndRotation(elbow + bottomRotation * (bone.position - oldLower), bottomRotation * bone.rotation);
        }
    }
}
