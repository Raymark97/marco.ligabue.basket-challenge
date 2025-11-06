using TMPro;
using UnityEngine;

namespace UI {
	/// <summary>
	/// Handles the display of the final scores and winner indicators
	/// at the end of the match. Updates both player and NPC scores and
	/// toggles the winner text based on the higher score.
	/// </summary>
	public class RewardManager : MonoBehaviour {
		[Header("References")]
		[SerializeField] private TextMeshProUGUI playerScoreText;
		[SerializeField] private TextMeshProUGUI npcScoreText;
		[SerializeField] private GameObject playerWinnerText;
		[SerializeField] private GameObject npcWinnerText;

		/// <summary>
		/// Updates the score display and shows the winner text
		/// depending on the comparison between player and NPC scores.
		/// </summary>
		/// <param name="playerScore">Final score of the player.</param>
		/// <param name="npcScore">Final score of the NPC opponent.</param>
		public void UpdateScore(int playerScore, int npcScore) {
			playerScoreText.text = playerScore.ToString();
			npcScoreText.text = npcScore.ToString();

			if (playerScore > npcScore) {
				playerWinnerText.SetActive(true);
				npcWinnerText.SetActive(false);
			} 
			else if (npcScore > playerScore) {
				playerWinnerText.SetActive(false);
				npcWinnerText.SetActive(true);
			} 
			else {
				playerWinnerText.SetActive(false);
				npcWinnerText.SetActive(false);
			}
		}
	}
}
