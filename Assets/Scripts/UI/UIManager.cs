using Core;
using TMPro;
using UnityEngine;
public class UIManager : MonoBehaviour {
	[SerializeField] private GameEvents gameEvents;
	[SerializeField] private TextMeshProUGUI playerScoreText;
	[SerializeField] private TextMeshProUGUI npcScoreText;

	private void OnEnable() {
		gameEvents.OnScoreUpdated.AddListener(UpdateScore);
	}

	private void OnDisable() {
		gameEvents.OnScoreUpdated.RemoveListener(UpdateScore);
	}

	public void UpdateScore(int playerId, int newScore) {
		if (playerId == 0) playerScoreText.text = newScore.ToString();
		else npcScoreText.text = newScore.ToString();
	}
}