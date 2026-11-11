#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.Fighter;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Battlefield.Tests.PlayMode
{
    public class FighterScaleTests
    {
        [UnityTest]
        public IEnumerator EnlargedFighterKeepsHumanSizedPilotSeatAndAuthoredCameraComposition()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            Assert.That(Vector3.Distance(context.Seat.transform.lossyScale, Vector3.one * 2.270328f), Is.LessThan(.0001f));
            MeshCollider hull = context.Seat.GetComponentInChildren<MeshCollider>();
            Assert.That(hull.bounds.size.x, Is.InRange(31f, 33f));
            Assert.That(hull.bounds.size.z, Is.InRange(46f, 48f));
            Vector3 modelScale = context.Animator.transform.localScale;
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            yield return null;
            Assert.That(context.Actor.localScale, Is.EqualTo(Vector3.one));
            Assert.That(context.Animator.transform.localScale, Is.EqualTo(modelScale));
            Assert.That(context.Occupant.CollisionBody.height, Is.EqualTo(4f));
            Assert.That(context.Occupant.CollisionBody.radius, Is.EqualTo(.5f));
            Assert.That(Vector3.Distance(context.Seat.SeatPoint.lossyScale, Vector3.one), Is.LessThan(.0001f));
            Assert.That(Vector3.Distance(context.Animator.GetBoneTransform(HumanBodyBones.Hips).position,
                context.Seat.SeatPoint.position), Is.LessThan(.002f));
            Transform eye = context.Seat.GetComponentsInChildren<Transform>().Single(t => t.name == "FirstPersonCameraPoint");
            Assert.That(Vector3.Distance(context.Animator.GetBoneTransform(HumanBodyBones.Head).position,
                eye.position), Is.LessThan(.6f));
            Transform authored = context.Seat.GetComponentsInChildren<Transform>().Single(t => t.name == "ThirdPersonCameraPoint");
            float expectedDistance = Vector3.Distance(authored.position, context.Seat.transform.position);
            float cameraDistance = Vector3.Distance(context.JetCamera.transform.position, context.Seat.transform.position);
            Assert.That(expectedDistance, Is.GreaterThan(50f));
            Assert.That(cameraDistance, Is.EqualTo(expectedDistance).Within(.05f), "The old 40m cap must not change the enlarged fighter's authored framing.");
            Assert.That(context.Seat.TryExit(), Is.True);
            yield return null;
            Assert.That(context.Occupant.CollisionBody.enabled, Is.True);
            Assert.That(context.Actor.IsChildOf(context.Seat.transform), Is.False);
        }

        [UnityTest]
        public IEnumerator EmptyEnlargedFighterSettlesOnRunwayWithExistingPhysicsTuning()
        {
            yield return BoardingTestContext.Load();
            FighterEngineActivation engine = Object.FindObjectsByType<FighterEngineActivation>().Single();
            Rigidbody body = engine.GetComponent<Rigidbody>();
            yield return new WaitForSeconds(1f);
            MeshCollider hull = engine.GetComponentInChildren<MeshCollider>();
            RaycastHit ground = Physics.RaycastAll(hull.bounds.center + Vector3.up * 30f,
                    Vector3.down, 100f, Physics.AllLayers, QueryTriggerInteraction.Ignore)
                .Where(hit => !hit.collider.transform.IsChildOf(engine.transform) && hit.normal.y > .7f)
                .OrderBy(hit => hit.distance).First();
            Assert.That(hull.bounds.min.y, Is.InRange(ground.point.y - .15f, ground.point.y + .5f));
            Assert.That(body.mass, Is.EqualTo(1000f));
            Assert.That(body.isKinematic, Is.False);
            Assert.That(engine.IsRunning, Is.False);
            Assert.That(engine.GetComponent<KeyboardJetInput>().enabled, Is.False);
            Assert.That(engine.GetComponent<Health>().IsDead, Is.False);
            Assert.That(engine.transform.parent, Is.Null);
        }
    }
}
#endif
