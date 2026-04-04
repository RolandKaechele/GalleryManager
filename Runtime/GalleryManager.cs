using System;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GalleryManager.Runtime
{
    // -------------------------------------------------------------------------
    // GalleryEntryType
    // -------------------------------------------------------------------------

    /// <summary>Category of a gallery entry.</summary>
    public enum GalleryEntryType { Artwork, Cutscene, Music, Collectible }

    // -------------------------------------------------------------------------
    // GalleryEntry
    // -------------------------------------------------------------------------

    /// <summary>
    /// Defines a single unlockable item in the gallery.
    /// Unlock state is persisted via <c>PlayerPrefs</c>.
    /// When <c>GALLERYMANAGER_SM</c> is active, SaveManager flags are also checked.
    /// When <c>GALLERYMANAGER_AM</c> is active, AchievementManager unlock events are listened to.
    /// </summary>
    [Serializable]
    public class GalleryEntry
    {
        [Tooltip("Unique key used to track unlock state. Must be globally unique.")]
        public string id;

        [Tooltip("Display label shown in the gallery UI.")]
        public string displayName;

        [Tooltip("Preview thumbnail shown in the gallery grid.")]
        public Sprite thumbnail;

        public GalleryEntryType type;

        [Tooltip("Always shown regardless of unlock state (e.g. first artwork, title music).")]
        public bool alwaysUnlocked;
    }

    // -------------------------------------------------------------------------
    // GalleryManager
    // -------------------------------------------------------------------------

    /// <summary>
    /// <b>GalleryManager</b> manages all unlockable gallery content:
    /// artworks, cutscenes, music tracks, and collectibles.
    ///
    /// <para><b>Responsibilities:</b>
    /// <list type="number">
    ///   <item>Persist unlock state per entry via <c>PlayerPrefs</c>.</item>
    ///   <item>Expose unlock/query API consumed by UI and gameplay systems.</item>
    ///   <item>Broadcast <see cref="OnEntryUnlocked"/> for UI to react live.</item>
    /// </list>
    /// </para>
    ///
    /// <para><b>Setup:</b> Add to a persistent manager GameObject (survives scene loads).
    /// Define entries in the Inspector.</para>
    ///
    /// <para><b>Optional integration defines:</b>
    /// <list type="bullet">
    ///   <item><c>GALLERYMANAGER_SM</c> — SaveManager: checks <c>gallery_&lt;id&gt;</c> save flags as an additional unlock source.</item>
    ///   <item><c>GALLERYMANAGER_AM</c> — AchievementManager: subscribes to achievement unlock events and mirrors them as gallery unlocks.</item>
    ///   <item><c>GALLERYMANAGER_EM</c> — EventManager: fires <c>GalleryEntryUnlocked</c> as a named GameEvent.</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("GalleryManager/Gallery Manager")]
    [DisallowMultipleComponent]
#if ODIN_INSPECTOR
    public class GalleryManager : SerializedMonoBehaviour
#else
    public class GalleryManager : MonoBehaviour
#endif
    {
        // -------------------------------------------------------------------------
        // Inspector
        // -------------------------------------------------------------------------

        [Header("Entries")]
        [Tooltip("All gallery entries defined for this project.")]
        [SerializeField] private GalleryEntry[] entries = Array.Empty<GalleryEntry>();

        // -------------------------------------------------------------------------
        // Events
        // -------------------------------------------------------------------------

        /// <summary>Fired when an entry is unlocked. Parameter: entry id.</summary>
        public event Action<string> OnEntryUnlocked;

        // -------------------------------------------------------------------------
        // Internals
        // -------------------------------------------------------------------------

        private const string UnlockPrefix = "gallery_unlock_";

        // Fast lookup: id → entry
        private readonly Dictionary<string, GalleryEntry> _index = new();

        // -------------------------------------------------------------------------
        // Unity lifecycle
        // -------------------------------------------------------------------------

        private void Awake()
        {
            _index.Clear();
            foreach (var e in entries)
                if (e != null && !string.IsNullOrEmpty(e.id))
                    _index[e.id] = e;
        }

        // -------------------------------------------------------------------------
        // Public API
        // -------------------------------------------------------------------------

        /// <summary>Read-only collection of all configured gallery entries.</summary>
        public IReadOnlyList<GalleryEntry> Entries => entries;

        /// <summary>
        /// Returns true if the entry identified by <paramref name="id"/> is unlocked.
        /// Check order: <c>alwaysUnlocked</c> flag → PlayerPrefs → SaveManager flag (if <c>GALLERYMANAGER_SM</c>).
        /// </summary>
        public bool IsUnlocked(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            if (_index.TryGetValue(id, out var entry) && entry.alwaysUnlocked) return true;
            if (PlayerPrefs.GetInt(UnlockPrefix + id, 0) == 1) return true;

#if GALLERYMANAGER_SM
            var sm = FindFirstObjectByType<SaveManager.Runtime.SaveManager>();
            if (sm != null && sm.IsSet("gallery_" + id)) return true;
#endif

            return false;
        }

        /// <summary>
        /// Unlock the entry with <paramref name="id"/> and persist the state.
        /// Fires <see cref="OnEntryUnlocked"/>. Idempotent.
        /// </summary>
        public void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            bool wasLocked = !IsUnlocked(id);
            PlayerPrefs.SetInt(UnlockPrefix + id, 1);
            PlayerPrefs.Save();

            if (!wasLocked) return; // Already unlocked — no event

            OnEntryUnlocked?.Invoke(id);

#if GALLERYMANAGER_EM
            FindFirstObjectByType<EventManager.Runtime.EventManager>()?.Fire("GalleryEntryUnlocked", id);
#endif

            Debug.Log($"[GalleryManager] Unlocked entry: {id}");
        }

        /// <summary>
        /// Lock (re-lock) the entry with <paramref name="id"/>.
        /// Clears the PlayerPrefs flag. Does not affect <c>alwaysUnlocked</c> entries.
        /// Useful for testing/reset flows only.
        /// </summary>
        public void Lock(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            PlayerPrefs.DeleteKey(UnlockPrefix + id);
            PlayerPrefs.Save();
        }

        /// <summary>Returns the <see cref="GalleryEntry"/> for <paramref name="id"/>, or null if not found.</summary>
        public GalleryEntry GetEntry(string id) =>
            _index.TryGetValue(id, out var e) ? e : null;

        /// <summary>Returns all entries that are currently unlocked.</summary>
        public List<GalleryEntry> GetUnlockedEntries()
        {
            var result = new List<GalleryEntry>();
            foreach (var e in entries)
                if (e != null && IsUnlocked(e.id))
                    result.Add(e);
            return result;
        }

        /// <summary>Returns entries filtered by <paramref name="type"/>.</summary>
        public List<GalleryEntry> GetEntriesByType(GalleryEntryType type)
        {
            var result = new List<GalleryEntry>();
            foreach (var e in entries)
                if (e != null && e.type == type)
                    result.Add(e);
            return result;
        }

        /// <summary>
        /// Unlock all entries. Useful for cheat/debug menus.
        /// </summary>
        public void UnlockAll()
        {
            foreach (var e in entries)
                if (e != null && !string.IsNullOrEmpty(e.id))
                    Unlock(e.id);
        }

        /// <summary>
        /// Lock all entries (reset). Does not affect <c>alwaysUnlocked</c> entries at query time.
        /// </summary>
        public void LockAll()
        {
            foreach (var e in entries)
                if (e != null && !string.IsNullOrEmpty(e.id))
                    Lock(e.id);
        }

        // -------------------------------------------------------------------------
        // Static helper (callable without a scene reference — e.g. from gameplay)
        // -------------------------------------------------------------------------

        /// <summary>
        /// Unlock a gallery entry directly via PlayerPrefs without needing a scene reference.
        /// Use this from gameplay scripts when <c>GalleryManager</c> may not be in the same scene.
        /// </summary>
        public static void UnlockStatic(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            PlayerPrefs.SetInt(UnlockPrefix + id, 1);
            PlayerPrefs.Save();
        }
    }
}
