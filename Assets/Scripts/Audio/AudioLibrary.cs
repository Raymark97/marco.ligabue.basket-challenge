using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Audio {
	/// <summary>
	/// ScriptableObject that stores and manages a collection of named audio clips.
	/// Provides efficient runtime access to clips via string keys.
	/// </summary>
	[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library")]
	public class AudioLibrary : ScriptableObject {
		[SerializeField, Tooltip("List of key–clip pairs used to populate the audio lookup table.")]
		private List<AudioEntry> entries = new();

		private Dictionary<string, AudioClip> lookup;

		/// <summary>
		/// Initializes the lookup dictionary when the asset is loaded or reloaded.
		/// Ensures that each key is unique and accessible at runtime.
		/// </summary>
		private void OnEnable() {
			lookup = new();
			foreach (var e in entries.Where(e => !lookup.ContainsKey(e.key)))
				lookup.Add(e.key, e.clip);
		}

		/// <summary>
		/// Retrieves the <see cref="AudioClip"/> associated with the specified key.
		/// </summary>
		/// <param name="key">The unique identifier of the desired clip.</param>
		/// <returns>
		/// The corresponding <see cref="AudioClip"/>, or <c>null</c> if no entry with that key exists.
		/// </returns>
		public AudioClip GetClip(string key) {
			if (lookup == null) OnEnable();
			lookup.TryGetValue(key, out var clip);
			return clip;
		}
	}
}
