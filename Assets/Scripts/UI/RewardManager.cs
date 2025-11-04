using TMPro;
using UnityEngine;
namespace UI {
	public class RewardManager : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI playerScoreText;
		[SerializeField] private TextMeshProUGUI npcScoreText;
		[SerializeField] private GameObject playerWinnerText;
		[SerializeField] private GameObject npcWinnerText;
		
		public void UpdateScore(int playerScore, int npcScore) {
			playerScoreText.text = playerScore.ToString();
			npcScoreText.text = npcScore.ToString();
			if (playerScore > npcScore) {
				playerWinnerText.SetActive(true);
				npcWinnerText.SetActive(false);
			} else if (npcScore > playerScore) {
				playerWinnerText.SetActive(false);
				npcWinnerText.SetActive(true);
			} else {
				playerWinnerText.SetActive(false);
				npcWinnerText.SetActive(false);
			}
			
		}
	}
}
