using UnityEngine;
namespace Gameplay {
	public static class ShotCalculator {

		public static bool CalculateDirectShot(Vector3 start, Vector3 target, float maxHeight, out Vector3 velocity) {
			var gravity = Mathf.Abs(Physics.gravity.y);
			var toTarget = target - start;
			var toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
			var distance = toTargetXZ.magnitude;

			if (maxHeight <= Mathf.Max(start.y, target.y)) {
				velocity = Vector3.zero;
				return false;
			}

			var vy = Mathf.Sqrt(2f * gravity * (maxHeight - start.y));
			var timeUp = vy / gravity;
			var fallDistance = maxHeight - target.y;
			var timeDown = Mathf.Sqrt(2f * fallDistance / gravity);
			var totalTime = timeUp + timeDown;

			var vxz = distance / totalTime;
			velocity = toTargetXZ.normalized * vxz + Vector3.up * vy;
			return true;
		}

		public static bool CalculateBankShot(Vector3 start, Vector3 target, Transform backboard, float maxHeight, out Vector3 velocity) {
			var boardNormal = backboard.forward;
			var toBoard = target - backboard.position;
			var distToPlane = Vector3.Dot(toBoard, boardNormal);
			var reflectedTarget = target - 2f * distToPlane * boardNormal;
			return CalculateDirectShot(start, reflectedTarget, maxHeight, out velocity);
		}
	}
}
