using UnityEngine;

namespace Gameplay {
	/// <summary>
	/// Provides static methods to calculate ideal projectile velocities
	/// for direct and bank shots.
	/// </summary>
	public static class ShotCalculator {

		/// <summary>
		/// Calculates the initial velocity required to reach the target directly
		/// given a desired maximum height.
		/// </summary>
		/// <param name="start">The starting position of the shot.</param>
		/// <param name="target">The target position (hoop center).</param>
		/// <param name="maxHeight">The maximum arc height above the ground.</param>
		/// <param name="velocity">Output vector representing the computed launch velocity.</param>
		/// <returns>True if a valid trajectory was found; false otherwise.</returns>
		public static bool CalculateDirectShot(Vector3 start, Vector3 target, float maxHeight, out Vector3 velocity) {
			var gravity = Mathf.Abs(Physics.gravity.y);
			var toTarget = target - start;
			var toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
			var distance = toTargetXZ.magnitude;

			// Ensure the max height is above both start and target
			if (maxHeight <= Mathf.Max(start.y, target.y)) {
				velocity = Vector3.zero;
				return false;
			}

			// Vertical component based on projectile motion equations
			var vy = Mathf.Sqrt(2f * gravity * (maxHeight - start.y));
			var timeUp = vy / gravity;

			// Compute falling time from peak to target height
			var fallDistance = maxHeight - target.y;
			var timeDown = Mathf.Sqrt(2f * fallDistance / gravity);
			var totalTime = timeUp + timeDown;

			// Horizontal velocity to cover the distance within total time
			var vxz = distance / totalTime;
			velocity = toTargetXZ.normalized * vxz + Vector3.up * vy;
			return true;
		}

		/// <summary>
		/// Calculates shot velocity simulating a bank shot off the backboard.
		/// </summary>
		/// <param name="start">The starting position of the shot.</param>
		/// <param name="target">The intended hoop target position.</param>
		/// <param name="backboard">The backboard transform used to compute reflection.</param>
		/// <param name="maxHeight">The maximum arc height above the ground.</param>
		/// <param name="velocity">Output vector representing the computed launch velocity.</param>
		/// <returns>True if a valid reflected trajectory was found; false otherwise.</returns>
		public static bool CalculateBankShot(Vector3 start, Vector3 target, Transform backboard, float maxHeight, out Vector3 velocity) {
			var boardNormal = backboard.forward;
			var toBoard = target - backboard.position;

			// Reflect target position across backboard plane
			var distToPlane = Vector3.Dot(toBoard, boardNormal);
			var reflectedTarget = target - 2f * distToPlane * boardNormal;

			// Reuse the direct shot calculation toward the mirrored target
			return CalculateDirectShot(start, reflectedTarget, maxHeight, out velocity);
		}
	}
}
