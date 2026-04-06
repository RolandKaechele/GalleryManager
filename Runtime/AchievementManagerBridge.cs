#if GALLERYMANAGER_AM
using AchievementManager.Runtime;
using UnityEngine;

namespace GalleryManager.Runtime
{
    /// <summary>
    /// <b>AchievementManagerBridge</b> connects GalleryManager to AchievementManager.
    /// <para>
    /// When <c>GALLERYMANAGER_AM</c> is defined:
    /// <list type="bullet">
    ///   <item>Subscribes to <see cref="AchievementManager.Runtime.AchievementManager.OnAchievementUnlocked"/>
    ///   and calls <see cref="GalleryManager.Unlock"/> so every achievement unlock is automatically
    ///   mirrored as a gallery entry unlock (entries with matching ids).</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Setup:</b> Add this component to the same GameObject as <see cref="GalleryManager"/>
    /// and add <c>GALLERYMANAGER_AM</c> to Player Settings › Scripting Define Symbols.
    /// </para>
    /// </summary>
    [AddComponentMenu("GalleryManager/Achievement Manager Bridge")]
    [DisallowMultipleComponent]
    public class AchievementManagerBridge : MonoBehaviour
    {
        private GalleryManager                                   _galleryManager;
        private AchievementManager.Runtime.AchievementManager   _achievementManager;

        private void Awake()
        {
            _galleryManager     = GetComponent<GalleryManager>() ?? FindFirstObjectByType<GalleryManager>();
            _achievementManager = GetComponent<AchievementManager.Runtime.AchievementManager>()
                                  ?? FindFirstObjectByType<AchievementManager.Runtime.AchievementManager>();

            if (_galleryManager == null)
                Debug.LogWarning("[GalleryManager/AchievementManagerBridge] GalleryManager not found.");
            if (_achievementManager == null)
                Debug.LogWarning("[GalleryManager/AchievementManagerBridge] AchievementManager not found — achievement mirroring disabled.");
        }

        private void OnEnable()
        {
            if (_achievementManager != null)
                _achievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
        }

        private void OnDisable()
        {
            if (_achievementManager != null)
                _achievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
        }

        private void OnAchievementUnlocked(string achievementId)
        {
            _galleryManager?.Unlock(achievementId);
        }
    }
}
#else
namespace GalleryManager.Runtime
{
    /// <summary>No-op stub — define <c>GALLERYMANAGER_AM</c> to activate this bridge.</summary>
    public class AchievementManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
