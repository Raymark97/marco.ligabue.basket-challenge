using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	public class UIManager : MonoBehaviour {
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private TextMeshProUGUI playerScoreText;
		[SerializeField] private TextMeshProUGUI npcScoreText;
		[SerializeField] private TextMeshProUGUI backboardText;
		[SerializeField] private Slider fireballChargeSlider;

		private void OnEnable() {
			gameEvents.OnScoreUpdated.AddListener(UpdateScore);
			gameEvents.OnBackboardBonusUpdated.AddListener(UpdateBackboardBonus);
			gameEvents.OnFireChargeChanged.AddListener(UpdateFireballChargeSlider);
		}
		private void UpdateFireballChargeSlider(int playerId, float charge) {
			if (playerId != 0) return;
			fireballChargeSlider.value = charge;
		}

		private void OnDisable() {
			gameEvents.OnScoreUpdated.RemoveListener(UpdateScore);
			gameEvents.OnBackboardBonusUpdated.RemoveListener(UpdateBackboardBonus);
		}

		private void UpdateBackboardBonus(int bonus) {
			backboardText.text = bonus == 0 ? "" : $"+{bonus}";
		}

		public void UpdateScore(int playerId, int newScore) {
			if (playerId == 0) playerScoreText.text = newScore.ToString();
			else npcScoreText.text = newScore.ToString();
		}
	}
}
