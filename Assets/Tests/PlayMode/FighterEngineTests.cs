#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using Battlefield.Features.Fighter;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Battlefield.Tests.PlayMode
{
    public class FighterEngineTests
    {
        [UnityTest]
        public IEnumerator OccupancyStartsAndStopsEngineAndAudioButCursorReleaseDoesNot()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            FighterEngine engine = context.Seat.GetComponent<FighterEngine>();
            JetMovement movement = context.Seat.GetComponent<JetMovement>();
            FighterEngineAudio audio = context.Seat.GetComponent<FighterEngineAudio>();
            Assert.That(engine.IsRunning, Is.False);
            Assert.That(Read<bool>(audio, "_playing"), Is.False);
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            yield return null;
            Assert.That(engine.IsRunning, Is.True);
            Assert.That(Read<bool>(audio, "_playing"), Is.True);
            Write(context.Input, "_captured", false);
            yield return null;
            yield return null;
            Assert.That(engine.IsRunning, Is.True, "Releasing the pointer does not leave the seat.");
            Assert.That(context.JetInput.enabled, Is.False);
            Write(context.Input, "_captured", true);
            Write(context.Input, "_captureFrame", -1);
            context.Seat.GetComponent<Afterburner>().Tick(true, .01f);
            Assert.That(context.Seat.TryExit(), Is.True);
            yield return null;
            yield return null;
            Assert.That(movement.IsEngineRunning, Is.False);
            Assert.That(movement.IsUnpowered, Is.True);
            Assert.That(context.Seat.GetComponent<Afterburner>().IsActive, Is.False);
            Assert.That(Read<bool>(audio, "_playing"), Is.False);
            Assert.That(Read<AudioSource>(audio, "_engineSource").volume, Is.Zero);
            Assert.That(Read<AudioSource>(audio, "_afterburnerSource").volume, Is.Zero);
            Assert.That(Read<bool>(context.Seat.GetComponent<JetEngineVfx>(), "_isPlaying"), Is.False);
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            yield return null;
            Assert.That(engine.IsRunning, Is.True);
            Assert.That(Read<bool>(audio, "_playing"), Is.True);
        }

        [UnityTest]
        public IEnumerator ShutdownClearsThrustWithoutErasingFlightMomentum()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            JetMovement movement = context.Seat.GetComponent<JetMovement>();
            Rigidbody body = context.Seat.GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.None;
            Vector3 velocity = new Vector3(12f, -8f, 60f);
            body.linearVelocity = velocity;
            movement.SetEngineRunning(false);
            Assert.That(body.linearVelocity, Is.EqualTo(velocity));
            Assert.That(Read<float>(movement, "_targetSpeed"), Is.Zero);
            Assert.That(movement.IsUnpowered, Is.True);
            typeof(JetMovement).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(movement, null);
            Assert.That(Read<float>(movement, "_targetSpeed"), Is.Zero, "Minimum flight speed must not restart a stopped engine.");
        }

        [UnityTest]
        public IEnumerator DeathTurnsOffEngineEvenWhilePilotRemainsSeated()
        {
            yield return BoardingTestContext.Load();
            var context = new BoardingTestContext();
            context.PrepareEntry();
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            yield return null;
            context.Seat.GetComponent<Health>().TakeDamage(float.MaxValue);
            yield return null;
            yield return null;
            Assert.That(context.Occupant.IsSeated, Is.True);
            Assert.That(context.Seat.GetComponent<FighterEngine>().IsRunning, Is.False);
            Assert.That(Read<bool>(context.Seat.GetComponent<FighterEngineAudio>(), "_playing"), Is.False);
        }

        private static T Read<T>(object target, string field) =>
            (T)target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        private static void Write(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
#endif
