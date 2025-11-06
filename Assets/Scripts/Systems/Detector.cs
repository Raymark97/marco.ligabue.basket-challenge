using Audio;
using Core;
using Gameplay;
using UnityEngine;

namespace Systems {
	/// <summary>
	/// Detects when a ball enters the scoring area (hoop trigger)
	/// and triggers the appropriate <see cref="GameEvents"/> and audio feedback.
	/// </summary>
	[RequireComponent(typeof(Collider)), RequireComponent(typeof(AudioSource))]
	public class Detector : MonoBehaviour {
		
		[Header("References")]
		[SerializeField] private GameEvents gameEvents;

		private AudioSource _scoreSource;

		private void Awake() {
			_scoreSource  = GetComponent<AudioSource>();
		}

		/// <summary>
		/// Called when a ball collider enters the detector volume.
		/// Verifies the ball type, plays the score sound, and invokes the OnScoreAdded event.
		/// </summary>
		/// <param name="other">The collider entering the trigger area.</param>
		private void OnTriggerEnter(Collider other) {
			var ball = other.GetComponentInParent<BallController>();
			if (ball == null) return;

			var playerId = -1;
			if (other.CompareTag("PlayerBall")) {
				playerId = 0;
			} else if (other.CompareTag("NPCBall"))
				playerId = 1;

			if (playerId == -1) return;
			
			// Play the scoring sound effect
			AudioManager.Instance.PlaySFX("Score", _scoreSource);

			// Prevent multiple triggers from the same ball
			other.tag = "Untagged";

			// Notify the global event system
			gameEvents.OnScoreAdded.Invoke(playerId, ball.perfectShot, ball.bankShot);

			Debug.Log($"[Detector] Score detected for Player {playerId} (Perfect: {ball.perfectShot}, Bank: {ball.bankShot})");
		}
	}
}
