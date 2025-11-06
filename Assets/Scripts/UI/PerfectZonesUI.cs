using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	/// <summary>
	/// Handles the visualization of the "perfect shot" zones on the power slider.
	/// Updates the two zone highlights (direct and bank) whenever the perfect thresholds change.
	/// </summary>
	public class PerfectZonesUI : MonoBehaviour {
		
		[Header("References")]
		[SerializeField] private Slider slider;
		[SerializeField] private Image directZone;
		[SerializeField] private Image bankZone;
		[SerializeField] private GameEvents gameEvents;

		private RectTransform _sliderRect;

		private void Awake() {
			_sliderRect = slider.GetComponent<RectTransform>();
		}

		private void OnEnable() {
			gameEvents.OnPerfectZonesChanged.AddListener(UpdateZones);
		}

		private void OnDisable() {
			gameEvents.OnPerfectZonesChanged.RemoveListener(UpdateZones);
		}

		/// <summary>
		/// Updates the UI zones to match the current perfect shot thresholds for direct and bank shots.
		/// </summary>
		/// <param name="directValue">Normalized slider position of the direct shot perfect value.</param>
		/// <param name="bankValue">Normalized slider position of the bank shot perfect value.</param>
		/// <param name="threshold">Width of the perfect zone region around each value.</param>
		private void UpdateZones(float directValue, float bankValue, float threshold) {
			if (!_sliderRect)
				_sliderRect = slider.GetComponent<RectTransform>();

			UpdateZone(directZone.rectTransform, directValue, threshold);
			UpdateZone(bankZone.rectTransform, bankValue, threshold);
		}

		/// <summary>
		/// Updates a single zone on the slider to represent its perfect range.
		/// </summary>
		private void UpdateZone(RectTransform zone, float center, float threshold) {
			if (zone == null || _sliderRect == null) return;

			var start = Mathf.Clamp01(center - threshold);
			var end = Mathf.Clamp01(center + threshold);
			Debug.Log($"Value: {center}, Boundaries: {start} - {end}");

			zone.anchorMin = new(0f, start);
			zone.anchorMax = new(1f, end);
			zone.offsetMin = Vector2.zero;
			zone.offsetMax = Vector2.zero;
			zone.anchoredPosition = Vector2.zero;
			zone.sizeDelta = Vector2.zero;
		}
	}
}
