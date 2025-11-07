using Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	/// <summary>
	/// Central manager for all in-game UI elements.
	/// Subscribes to global <see cref="GameEvents"/> and updates score,
	/// timer, fireball charge, and backboard bonus indicators in real time.
	/// Controls the transition between gameplay and reward screens.
	/// </summary>
	public class UIManager : MonoBehaviour {
		[Header("References")]
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private TextMeshProUGUI playerScoreText;
		[SerializeField] private TextMeshProUGUI npcScoreText;
		[SerializeField] private TextMeshProUGUI backboardText;
		[SerializeField] private Slider fireballChargeSlider;
		[SerializeField] private TextMeshProUGUI timer;
		[SerializeField] private GameObject mainCanvas;
		[SerializeField] private GameObject rewardCanvas;
		[SerializeField] private RewardManager rewardManager;

		[Header("Settings")]
		[SerializeField] private Color fireballActiveColor = Color.red;
		[SerializeField] private Color fireballChargingColor = Color.yellow;

		private void OnEnable() {
			gameEvents.OnScoreUpdated.AddListener(UpdateScore);
			gameEvents.OnBackboardBonusUpdated.AddListener(UpdateBackboardBonus);
			gameEvents.OnFireChargeChanged.AddListener(UpdateFireballChargeSlider);
			gameEvents.OnFireStateChanged.AddListener(UpdateFireballChargeEffect);
			gameEvents.OnTimerChanged.AddListener(ChangeTimer);
			gameEvents.OnMatchEnded.AddListener(ShowRewards);

			// Initialize fireball charge color
			fireballChargeSlider.fillRect.GetComponent<Image>().color = fireballChargingColor;
		}
		
		private void OnDisable() {
			gameEvents.OnScoreUpdated.RemoveListener(UpdateScore);
			gameEvents.OnBackboardBonusUpdated.RemoveListener(UpdateBackboardBonus);
			gameEvents.OnFireChargeChanged.RemoveListener(UpdateFireballChargeSlider);
			gameEvents.OnFireStateChanged.RemoveListener(UpdateFireballChargeEffect);
			gameEvents.OnTimerChanged.RemoveListener(ChangeTimer);
			gameEvents.OnMatchEnded.RemoveListener(ShowRewards);
		}

		/// <summary>
		/// Displays the reward screen at the end of the match,
		/// hiding the main canvas and showing the winner.
		/// </summary>
		private void ShowRewards() {
			mainCanvas.SetActive(false);
			rewardManager.UpdateScore(
				int.Parse(playerScoreText.text),
				int.Parse(npcScoreText.text)
			);
			rewardCanvas.SetActive(true);
		}

		/// <summary>
		/// Updates the countdown timer text in <c>mm:ss</c> format.
		/// </summary>
		/// <param name="timerValue">Remaining time in seconds.</param>
		private void ChangeTimer(int timerValue) {
			timer.text = TimeSpan.FromSeconds(timerValue).ToString(@"mm\:ss");
		}

		/// <summary>
		/// Changes the fill color of the fireball charge slider
		/// based on whether the fireball mode is active.
		/// </summary>
		/// <param name="playerId">Player identifier (0 for player, 1 for NPC).</param>
		/// <param name="active">Whether the fireball mode is currently active.</param>
		private void UpdateFireballChargeEffect(int playerId, bool active) {
			if (playerId != 0) return;
			fireballChargeSlider.fillRect.GetComponent<Image>().color =
				active ? fireballActiveColor : fireballChargingColor;
		}

		/// <summary>
		/// Updates the value of the fireball charge slider for the player.
		/// </summary>
		/// <param name="playerId">Player identifier (0 for player, 1 for NPC).</param>
		/// <param name="charge">Normalized fireball charge value (0–1).</param>
		private void UpdateFireballChargeSlider(int playerId, float charge) {
			if (playerId != 0) return;
			fireballChargeSlider.value = charge;
		}

		/// <summary>
		/// Updates the displayed bonus value for the backboard.
		/// </summary>
		/// <param name="bonus">Active backboard bonus value. Zero hides the text.</param>
		private void UpdateBackboardBonus(int bonus) {
			backboardText.text = bonus == 0 ? "" : $"+{bonus}";
		}

		/// <summary>
		/// Updates the score display for either player or NPC.
		/// </summary>
		/// <param name="playerId">Player identifier (0 for player, 1 for NPC).</param>
		/// <param name="newScore">The new score value to display.</param>
		private void UpdateScore(int playerId, int newScore) {
			if (playerId == 0)
				playerScoreText.text = newScore.ToString();
			else
				npcScoreText.text = newScore.ToString();
		}
	}
}
