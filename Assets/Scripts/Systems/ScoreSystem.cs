using Core;
using UnityEngine;
namespace Systems {
	public class ScoreSystem : MonoBehaviour {
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private int normalPoints = 2;
		[SerializeField] private int perfectPoints = 3;
		[SerializeField] private int backboardBonus = 1;

		private int _playerPoints;
		private int _npcPoints;
		
		private void OnEnable() {
			gameEvents.OnScoreAdded.AddListener(AddPoints);
		}

		private void OnDisable() {
			gameEvents.OnScoreAdded.RemoveListener(AddPoints);
		}

		public void AddPoints(int playerId, bool perfect, bool backShot) {
			var points = perfect ? perfectPoints : normalPoints;
			if (backShot) points += backboardBonus;

			if (playerId == 0) _playerPoints += points;
			else _npcPoints += points;

			var newScore = playerId == 0 ? _playerPoints : _npcPoints;
			gameEvents.OnScoreUpdated.Invoke(playerId, newScore);
		}
	}
}
