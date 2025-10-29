using Gameplay;
using System.Collections;
using UnityEngine;
namespace Core {
	public class GameManager : MonoBehaviour {
		public static GameManager Instance;

		[Header("References")]
		[SerializeField] private GameEvents gameEvents;

		[Header("Gameplay")]
		[SerializeField] private Transform hoop;
		[SerializeField] private Transform backboard;
		[SerializeField] private float maxHeight = 4.5f;

		public Vector3 DirectShotVelocity { get; private set; }
		public Vector3 BankShotVelocity { get; private set; }

		private void Awake() {
			if (Instance == null) Instance = this;
			else { Destroy(gameObject); }
		}

		private void Start() {
			StartCoroutine(StartRace());
		}

		private IEnumerator StartRace() {
			var player = FindObjectOfType<PlayerController>().transform;
			if (!ShotCalculator.CalculateDirectShot(player.position, hoop.position, maxHeight, out var direct)) {
				Debug.LogWarning($"Impossible to calculate direct shot for {player.gameObject.name}");
			}
			if (!ShotCalculator.CalculateBankShot(player.position, hoop.position, backboard, maxHeight, out var bank)) {
				Debug.LogWarning($"Impossible to calculate bank shot for  {player.gameObject.name}");
			}

			DirectShotVelocity = direct;
			BankShotVelocity = bank;

			yield return new WaitForSeconds(0.5f);

			gameEvents.OnBackboardBonusUpdated.Invoke(0);
			
			yield return new WaitForSeconds(2f);
			
			gameEvents.OnBackboardBonusUpdated.Invoke(6);
		}
	}
}
