using UnityEngine;
using UnityEngine.Events;

namespace Core {
	[CreateAssetMenu(fileName = "GameEvents", menuName = "Game/GameEvents")]
	public class GameEvents : ScriptableObject {
		// ─────────────────────────────
		//  SCORING & POINTS
		// ─────────────────────────────
		public UnityEvent<int, bool, bool> OnScoreAdded = new();        // (playerId, perfect, bankShot)
		public UnityEvent<int, int> OnScoreUpdated = new();              // (playerId, totalScore)
		public UnityEvent<int> OnBackboardBonusUpdated = new();          // (bonus amount)

		// ─────────────────────────────
		//  FIREBALL MODE
		// ─────────────────────────────
		public UnityEvent<int, float> OnFireChargeChanged = new();       // (playerId, normalized charge)
		public UnityEvent<int, bool> OnFireStateChanged = new();         // (playerId, isActive)

		// ─────────────────────────────
		//  SHOOTING / TRAJECTORY
		// ─────────────────────────────
		public UnityEvent<float, float, float> OnPerfectZonesChanged = new(); // (direct, bank, threshold)
		public UnityEvent<int, GameObject> OnBallThrown = new();         // (playerId, ballInstance)
		public UnityEvent<int> OnShotMiss = new();                       // (playerId)

		// ─────────────────────────────
		//  PROGRESSION / COMPETITION
		// ─────────────────────────────
		public UnityEvent<int> OnPlayerFinished = new();                 // (playerId)
	}
}
