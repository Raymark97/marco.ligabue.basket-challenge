using Core;
using Gameplay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Systems {
    public class ShotCompetitionManager : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GameEvents gameEvents;
        [SerializeField, Tooltip("Parent GameObject of all the shot positions")] private Transform shotPositionGroup;
		
        [SerializeField] private PlayerController player;
        [SerializeField] private NPCController npc;
        [SerializeField] private Transform hoop;
        [SerializeField] private Transform playerCamPos;

        [Header("Settings")]
        [SerializeField, Tooltip("Offset laterale dal punto base per separare player e NPC")]
        private float sideOffset = 1.5f;
        
        [Header("Match Settings")]
        [SerializeField, Tooltip("Duration of the match in seconds")]
        private int matchDuration = 90;
        [SerializeField, Tooltip("Initial delay before the first bonus appears (in seconds)")]
		private float bonusInitialDelay = 10f;
        [SerializeField, Tooltip("How often a new bonus can appear (in seconds)")]
        private Vector2 bonusSpawnInterval = new Vector2(5f, 12f);
        [SerializeField, Tooltip("Possible bonus point values")]
        private int[] bonusValues = { 4, 6, 8 };


        private int _playerIndex;
        private int _npcIndex;
        private bool _playerFinished;
        private bool _npcFinished;
        private Transform[] _shotPositions;

        private void OnEnable() {
            gameEvents.OnScoreAdded.AddListener(OnScoreAdded);
        }

        private void OnDisable() {
            gameEvents.OnScoreAdded.RemoveListener(OnScoreAdded);
        }

        private void Start() {
	        _shotPositions = shotPositionGroup.Cast<Transform>().ToArray();
            PositionPlayer(_playerIndex);
            PositionNPC(_npcIndex);
            player.RecalculateTrajectories();
            npc.RecalculateTrajectories();
            StartCoroutine(MatchTimer());
            StartCoroutine(BonusRoutine());
        }

        private void OnScoreAdded(int playerId, bool perfect, bool bankShot) {
            if (playerId == 0 && !_playerFinished)
	            AdvancePlayer(ref _playerIndex, player, ref _playerFinished, true);
            else if (playerId == 1 && !_npcFinished)
	            AdvancePlayer(ref _npcIndex, npc, ref _npcFinished, false);
        }


        private void AdvancePlayer(ref int index, IShotController controller, ref bool finishedFlag, bool isPlayer) {
	        index++;
	        if (index >= _shotPositions.Length) {
		        finishedFlag = true;
		        gameEvents.OnPlayerFinished.Invoke(isPlayer ? 0 : 1);
		        return;
	        }

	        if (isPlayer)
		        PositionPlayer(index);
	        else
		        PositionNPC(index);

	        controller.RecalculateTrajectories();
        }

        private void PositionPlayer(int index) {
            if (index < 0 || index >= _shotPositions.Length) return;

            var basePos = _shotPositions[index];
            var forward = (hoop.position - basePos.position).normalized;
            forward.y = 0f;
            var right = Vector3.Cross(Vector3.up, forward);

            var playerPos = basePos.position - right * (sideOffset / 2f);

            player.transform.SetPositionAndRotation(playerPos, Quaternion.LookRotation(forward));
        }

        private void PositionNPC(int index) {
            if (index < 0 || index >= _shotPositions.Length) return;

            var basePos = _shotPositions[index];
            var forward = (hoop.position - basePos.position).normalized;
            forward.y = 0f;
            var right = Vector3.Cross(Vector3.up, forward);

            var npcPos = basePos.position + right * (sideOffset / 2f);

            npc.transform.SetPositionAndRotation(npcPos, Quaternion.LookRotation(forward));
        }
        
        private IEnumerator MatchTimer() {
	        var remainingTime = matchDuration;

	        // Immediately send the initial value to the UI
	        gameEvents.OnTimerChanged.Invoke(remainingTime);

	        // Countdown loop
	        while (remainingTime > 0) {
		        yield return new WaitForSeconds(1f);
		        remainingTime--;
		        gameEvents.OnTimerChanged.Invoke(remainingTime);
	        }

	        // When time runs out
	        EndMatch();
        }


        private void EndMatch() {
	        npc.StopShooting();
	        player.enabled = false;

	        gameEvents.OnPlayerFinished.Invoke(0);
	        gameEvents.OnPlayerFinished.Invoke(1);

	        Debug.Log("Match ended — time is up!");
        }
        
        private IEnumerator BonusRoutine() {
	        yield return new WaitForSeconds(bonusInitialDelay);
	        while (Application.isPlaying) {
		        yield return new WaitForSeconds(Random.Range(bonusSpawnInterval.x, bonusSpawnInterval.y));
		        var bonusValue = bonusValues[Random.Range(0, bonusValues.Length)];
		        gameEvents.OnBackboardBonusUpdated.Invoke(bonusValue);
	        }
        }
    }
}
