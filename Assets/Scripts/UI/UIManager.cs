using Core;
using TMPro;
using UnityEngine;
namespace UI {
	public class UIManager : MonoBehaviour {
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private TextMeshProUGUI playerScoreText;
		[SerializeField] private TextMeshProUGUI npcScoreText;
		[SerializeField] private TextMeshProUGUI backboardText;

		private void OnEnable() {
			gameEvents.OnScoreUpdated.AddListener(UpdateScore);
			gameEvents.OnBackboardBonusUpdated.AddListener(UpdateBackboardBonus);
		}

		private void OnDisable() {
			gameEvents.OnScoreUpdated.RemoveListener(UpdateScore);
			gameEvents.OnBackboardBonusUpdated.RemoveListener(UpdateBackboardBonus);
		}

		public void UpdateBackboardBonus(int bonus) {
			backboardText.text = $"+{bonus}";
		}

		public void UpdateScore(int playerId, int newScore) {
			if (playerId == 0) playerScoreText.text = newScore.ToString();
			else npcScoreText.text = newScore.ToString();
		}
	}
}