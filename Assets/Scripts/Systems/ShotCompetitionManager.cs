using Audio;
using Core;
using Gameplay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Systems {
	/// <summary>
	/// Manages the entire shooting competition flow between player and NPC.
	/// Handles positioning, scoring progression, match timing, and random bonuses.
	/// </summary>
	public class ShotCompetitionManager : MonoBehaviour {
		[System.Serializable]
		public struct BonusEntry {
			public int value;
			public int frequency;
			public BonusEntry(int value, int frequency) {
				this.value = value;
				this.frequency = frequency;
			}
		}

		[Header("References")]
		[SerializeField] private GameEvents gameEvents;
		[SerializeField] private Transform shotPositionGroup;
		[SerializeField] private PlayerController player;
		[SerializeField] private NPCController npc;
		[SerializeField] private Transform hoop;
		[SerializeField] private Transform playerCamPos;

		[Header("Settings")]
		[SerializeField, Tooltip("Lateral offset to separate player and NPC positions.")]
		private float sideOffset = 1.5f;

		[Header("Match Settings")]
		[SerializeField] private int matchDuration = 90;
		[SerializeField] private Vector2 bonusDuration = new(5f, 10f);
		[SerializeField] private Vector2 bonusSpawnInterval = new(5f, 10f);

		[SerializeField, Tooltip("List of bonuses with their corresponding frequency. Higher frequency means higher chance to be selected.")]
		private List<BonusEntry> bonusFrequencies = new() {
		new(4, 8),
		new(6, 8),
		new(8, 4)
		};

		private int _playerIndex;
		private int _npcIndex;
		private Transform[] _shotPositions;

		private void OnEnable() => gameEvents.OnBasketMade.AddListener(OnScoreAdded);
		private void OnDisable() => gameEvents.OnBasketMade.RemoveListener(OnScoreAdded);

		private void Start() {
			_shotPositions = shotPositionGroup.Cast<Transform>().ToArray();
			PositionPlayer(_playerIndex);
			PositionNPC(_npcIndex);

			player.RecalculateTrajectories();
			npc.RecalculateTrajectories();

			StartCoroutine(MatchTimer());
			StartCoroutine(BonusRoutine());
		}

		#region Match Flow

		/// <summary>
		/// Handles score updates and moves the corresponding player forward.
		/// </summary>
		private void OnScoreAdded(int playerId, bool perfect, bool bankShot) {
			if (playerId == 0)
				AdvancePlayer(ref _playerIndex, player, true);
			else if (playerId == 1)
				AdvancePlayer(ref _npcIndex, npc, false);
		}

		/// <summary>
		/// Advances a player to the next shooting position and recalculates its trajectories.
		/// </summary>
		private void AdvancePlayer(ref int index, IShotController controller, bool isPlayer) {
			index = (index + 1) % _shotPositions.Length;

			if (isPlayer)
				PositionPlayer(index);
			else
				PositionNPC(index);

			controller.RecalculateTrajectories();
		}

		/// <summary>
		/// Handles match countdown and triggers end-of-match events when time expires.
		/// </summary>
		private IEnumerator MatchTimer() {
			var remainingTime = matchDuration;
			gameEvents.OnTimerChanged.Invoke(remainingTime);

			while (remainingTime > 0) {
				yield return new WaitForSeconds(1f);
				remainingTime--;
				gameEvents.OnTimerChanged.Invoke(remainingTime);
			}

			EndMatch();
		}

		/// <summary>
		/// Stops the match, disables player input, and triggers the end-of-match sound and event.
		/// </summary>
		private void EndMatch() {
			npc.StopShooting();
			player.enabled = false;
			Debug.Log("Match ended — time is up!");
			AudioManager.Instance.PlaySFXAtPoint("MatchEnd", playerCamPos);
			gameEvents.OnMatchEnded.Invoke();
		}


		/// <summary>
		/// Places the player at the specified shooting position and aligns them toward the hoop.
		/// </summary>
		private void PositionPlayer(int index) {
			if (index < 0 || index >= _shotPositions.Length) return;

			var basePos = _shotPositions[index];
			var forward = (hoop.position - basePos.position).normalized;
			forward.y = 0f;
			var right = Vector3.Cross(Vector3.up, forward);

			var playerPos = basePos.position - right * (sideOffset / 2f);
			player.transform.SetPositionAndRotation(playerPos, Quaternion.LookRotation(forward));
		}

		/// <summary>
		/// Places the NPC at the specified shooting position and aligns them toward the hoop.
		/// </summary>
		private void PositionNPC(int index) {
			if (index < 0 || index >= _shotPositions.Length) return;

			var basePos = _shotPositions[index];
			var forward = (hoop.position - basePos.position).normalized;
			forward.y = 0f;
			var right = Vector3.Cross(Vector3.up, forward);

			var npcPos = basePos.position + right * (sideOffset / 2f);
			npc.transform.SetPositionAndRotation(npcPos, Quaternion.LookRotation(forward));
		}

		#endregion

		/// <summary>
		/// Periodically activates random backboard bonuses during the match.
		/// </summary>
		private IEnumerator BonusRoutine() {
			while (Application.isPlaying) {
				yield return new WaitForSeconds(Random.Range(bonusSpawnInterval.x, bonusSpawnInterval.y));

				var totalWeight = bonusFrequencies.Sum(b => b.frequency);
				var r = Random.Range(0, totalWeight);
				var cumulative = 0;

				var bonusValue = bonusFrequencies
				.First(b => (cumulative += b.frequency) > r)
				.value;
				gameEvents.OnBackboardBonusUpdated.Invoke(bonusValue);
				yield return new WaitForSeconds(Random.Range(bonusDuration.x, bonusDuration.y));
				gameEvents.OnBackboardBonusUpdated.Invoke(0);
			}
		}
	}
}
