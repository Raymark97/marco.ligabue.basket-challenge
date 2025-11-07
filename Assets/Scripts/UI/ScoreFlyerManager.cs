using Core;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages and animates score flyers for both player and NPC.
/// Displays the points gained when a shot is scored and optionally highlights perfect shots.
/// </summary>
public class ScoreFlyerManager : MonoBehaviour {
	[Header("References")]
	[SerializeField] private GameEvents gameEvents;
	[SerializeField] private TextMeshProUGUI playerFlyer;
	[SerializeField] private TextMeshProUGUI npcFlyer;

	[Header("Settings")]
	[SerializeField] private float flyDuration = 1f;
	[SerializeField] private Color shotColor = new(1f, 0.53f, 0.24f);
	[SerializeField] private Color perfectShotColor = Color.green;
	[SerializeField] private Vector3 playerFlyOffset = new(0, 50, 0);
	[SerializeField] private Vector3 npcFlyOffset = new(0, 25, 0);


	private void OnEnable() {
		gameEvents.OnScoreAdded.AddListener(OnScoreAdded);
	}
	private void OnDisable() {
		gameEvents.OnScoreAdded.RemoveListener(OnScoreAdded);
	}

	/// <summary>
	/// Called when a score is updated for either the player or NPC.
	/// Updates the corresponding flyer text, sets the appropriate color, and starts the animation.
	/// </summary>
	/// <param name="player">Identifier of the scoring player (0 = player, 1 = NPC).</param>
	/// <param name="score">Points scored by the player.</param>
	/// <param name="perfect">Indicates whether the shot was perfect, used to change flyer text and color.</param>
	private void OnScoreAdded(int player, int score, bool perfect) {
		switch (player) {
			case 0:
				playerFlyer.text = "+" + score;
				if (perfect) {
					playerFlyer.text += "\n Perfect Shot!";
				}
				playerFlyer.color = perfect ? perfectShotColor : shotColor;
				StartCoroutine(AnimateFlyer(playerFlyer, playerFlyOffset));
				break;
			case 1:
				npcFlyer.text = "+" + score;
				npcFlyer.color = shotColor;
				StartCoroutine(AnimateFlyer(npcFlyer, npcFlyOffset));
				break;
		}
	}
	
	/// <summary>
	/// Animates a flyer by moving it upward and fading it out over a specified duration.
	/// Resets the flyer position and deactivates it after the animation completes.
	/// </summary>
	/// <param name="flyer">The TextMeshProUGUI component to animate.</param>
	/// <param name="offset">The local offset by which the flyer moves during the animation.</param>
	private IEnumerator AnimateFlyer(TextMeshProUGUI flyer, Vector3 offset) {
		flyer.gameObject.SetActive(true);
		flyer.alpha = 1f;

		var startPos = flyer.rectTransform.localPosition;
		var endPos = startPos + offset;

		var timer = 0f;
		while (timer < flyDuration) {
			timer += Time.deltaTime;
			var t = timer / flyDuration;

			flyer.rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
			if (t > 0.7f)
				flyer.alpha = Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);

			yield return null;
		}

		flyer.alpha = 0f;
		flyer.rectTransform.localPosition = startPos;
		flyer.gameObject.SetActive(false);
	}
}
