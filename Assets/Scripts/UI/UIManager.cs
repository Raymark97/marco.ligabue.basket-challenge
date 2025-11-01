using Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	public class UIManager : MonoBehaviour {
		[Header("References")]
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private TextMeshProUGUI playerScoreText;
		[SerializeField] private TextMeshProUGUI npcScoreText;
		[SerializeField] private TextMeshProUGUI backboardText;
		[SerializeField] private Slider fireballChargeSlider;
		[SerializeField] private TextMeshProUGUI timer;
		
		[Header("Settings")]
		[SerializeField] private Color fireballActiveColor = Color.red;
		[SerializeField] private Color fireballChargingColor = Color.yellow;

		private void OnEnable() {
			gameEvents.OnScoreUpdated.AddListener(UpdateScore);
			gameEvents.OnBackboardBonusUpdated.AddListener(UpdateBackboardBonus);
			gameEvents.OnFireChargeChanged.AddListener(UpdateFireballChargeSlider);
			gameEvents.OnFireStateChanged.AddListener(UpdateFireballChargeEffect);
			gameEvents.OnTimerChanged.AddListener(ChangeTimer);
			fireballChargeSlider.fillRect.GetComponent<Image>().color = fireballChargingColor;
		}
		private void ChangeTimer(int timerValue) {
			timer.text = TimeSpan.FromSeconds(timerValue).ToString(@"mm\:ss");
		}
		private void UpdateFireballChargeEffect(int playerId, bool active) {
			if(playerId != 0) return;
			fireballChargeSlider.fillRect.GetComponent<Image>().color = active ? fireballActiveColor : fireballChargingColor;
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
