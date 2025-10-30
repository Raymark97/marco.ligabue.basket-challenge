using UnityEngine;
using UnityEngine.Events;

namespace Core {
	[CreateAssetMenu(fileName = "GameEvents", menuName = "Game/GameEvents")]
	public class GameEvents : ScriptableObject {
		public UnityEvent<int, bool, bool> OnScoreAdded = new();
		public UnityEvent<int, int> OnScoreUpdated = new();
		public UnityEvent<int> OnBackboardBonusUpdated = new();
		public UnityEvent<float, float, float> OnPerfectZonesChanged = new();

		public UnityEvent<int, float> OnFireChargeChanged = new();
		public UnityEvent<int, bool> OnFireStateChanged = new();
		public UnityEvent<int> OnShotMiss = new();
		public UnityEvent<int, GameObject> OnBallThrown = new();
	}
}
