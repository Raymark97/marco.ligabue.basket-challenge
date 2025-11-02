using Core;
using Gameplay;
using UnityEngine;

namespace Systems {
	[RequireComponent(typeof(Collider))]
	public class Detector : MonoBehaviour {
		[SerializeField] private GameEvents gameEvents;

		private void OnTriggerEnter(Collider other) {
			var ball = other.GetComponentInParent<BallController>();
			if (ball == null) return;

			var playerId = -1;
			if (other.CompareTag("PlayerBall"))
				playerId = 0;
			else if (other.CompareTag("NPCBall"))
				playerId = 1;

			if (playerId == -1) return;

			// Evita doppi trigger
			other.tag = "Untagged";

			// Comunica l’evento globale
			gameEvents.OnScoreAdded.Invoke(playerId, ball.perfectShot, ball.bankShot);

			Debug.Log($"[Detector] Score detected for Player {playerId} (Perfect: {ball.perfectShot}, Bank: {ball.bankShot})");
		}
	}
}
