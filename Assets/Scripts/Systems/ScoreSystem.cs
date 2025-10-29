using Core;
using System.Collections;
using UnityEngine;
namespace Systems {
	public class ScoreSystem : MonoBehaviour {
		[Header("Events")]
		[SerializeField] private GameEvents gameEvents;

		[Header("Scoring")]
		[SerializeField] private int normalPoints = 2;
		[SerializeField] private int perfectPoints = 3;
		[SerializeField] private int backboardBonus = 1;

		[Header("Fireball Settings")]
		[SerializeField] private float pointsToFillFire = 10f;
		[SerializeField] private float fireDuration = 5f;
		[SerializeField] private float fireDecayRate = 1f;
		[SerializeField] private float fireLossOnMiss = 1f;

		private int _playerPoints, _npcPoints;
		private float _fireChargePlayer, _fireChargeNPC;
		private bool _fireActivePlayer, _fireActiveNPC;
		private Coroutine _fireCoroutinePlayer, _fireCoroutineNPC;
		
		private void OnEnable() {
			gameEvents.OnScoreAdded.AddListener(AddPoints);
			gameEvents.OnBackboardBonusUpdated.AddListener(SetBackboardBonus);
		}

		private void OnDisable() {
			gameEvents.OnScoreAdded.RemoveListener(AddPoints);
			gameEvents.OnBackboardBonusUpdated.RemoveListener(SetBackboardBonus);
		}

		private void SetBackboardBonus(int bonus) {
			backboardBonus = bonus;
		}

		public void AddPoints(int playerId, bool perfect, bool backShot) {
			var points = perfect ? perfectPoints : normalPoints;
			if (backShot) points += backboardBonus;

			if (playerId == 0) _playerPoints += points;
			else _npcPoints += points;

			var newScore = playerId == 0 ? _playerPoints : _npcPoints;
			gameEvents.OnScoreUpdated.Invoke(playerId, newScore);
		}
		
		private void AddFireCharge(int playerId, int points) {
			ref var charge = ref playerId == 0 ? ref _fireChargePlayer : ref _fireChargeNPC;
			ref var active = ref playerId == 0 ? ref _fireActivePlayer : ref _fireActiveNPC;
			ref var routine = ref playerId == 0 ? ref _fireCoroutinePlayer : ref _fireCoroutineNPC;

			if (active) return;

			charge += points;
			var normalized = Mathf.Clamp01(charge / pointsToFillFire);
			gameEvents.OnFireChargeChanged.Invoke(playerId, normalized);

			if (charge >= pointsToFillFire) {
				active = true;
				gameEvents.OnFireStateChanged.Invoke(playerId, true);
				routine = StartCoroutine(FireModeRoutine(playerId));
			}
		}
		
		private IEnumerator FireModeRoutine(int playerId) {
			var charge = 1f;
			gameEvents.OnFireChargeChanged.Invoke(playerId, charge);

			while (charge > 0f) {
				charge -= Time.deltaTime / fireDuration * fireDecayRate;
				gameEvents.OnFireChargeChanged.Invoke(playerId, Mathf.Clamp01(charge));
				yield return null;
			}

			EndFireMode(playerId);
		}
		
		public void OnMiss(int playerId) {
			ref var active = ref playerId == 0 ? ref _fireActivePlayer : ref _fireActiveNPC;
			if (!active) return;

			EndFireMode(playerId);
		}

		private void EndFireMode(int playerId) {
			ref var active = ref playerId == 0 ? ref _fireActivePlayer : ref _fireActiveNPC;
			ref var charge = ref playerId == 0 ? ref _fireChargePlayer : ref _fireChargeNPC;
			ref var routine = ref playerId == 0 ? ref _fireCoroutinePlayer : ref _fireCoroutineNPC;

			if (routine != null) StopCoroutine(routine);
			active = false;
			charge = 0f;
			gameEvents.OnFireStateChanged.Invoke(playerId, false);
			gameEvents.OnFireChargeChanged.Invoke(playerId, 0f);
		}

		private bool IsFireActive(int playerId) {
			return playerId == 0 ? _fireActivePlayer : _fireActiveNPC;
		}
	}
}
