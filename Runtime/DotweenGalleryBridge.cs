#if GALLERYMANAGER_DOTWEEN
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GalleryManager.Runtime
{
    /// <summary>
    /// Optional bridge that adds DOTween-driven animations to the gallery UI:
    /// thumbnail grid items fade and scale up when the gallery opens, and a toast
    /// notification slides in whenever a new entry is unlocked at runtime.
    /// Enable define <c>GALLERYMANAGER_DOTWEEN</c> in Player Settings › Scripting Define Symbols.
    /// Requires <b>DOTween Pro</b>.
    /// <para>
    /// Assign <see cref="galleryPanelGroup"/> to the <see cref="CanvasGroup"/> of the gallery
    /// root panel, and optionally <see cref="unlockToastRoot"/> to the notification toast panel
    /// shown on entry unlock.
    /// </para>
    /// </summary>
    [AddComponentMenu("GalleryManager/DOTween Bridge")]
    [DisallowMultipleComponent]
    public class DotweenGalleryBridge : MonoBehaviour
    {
        [Header("Gallery Panel")]
        [Tooltip("CanvasGroup on the gallery root — faded in when the gallery opens.")]
        [SerializeField] private CanvasGroup galleryPanelGroup;

        [Tooltip("Duration for the gallery panel to fade in.")]
        [SerializeField] private float panelFadeInDuration = 0.3f;

        [Tooltip("DOTween ease for gallery panel fade-in.")]
        [SerializeField] private Ease panelFadeEase = Ease.OutQuad;

        [Header("Thumbnail Grid")]
        [Tooltip("Parent transform that holds thumbnail cells in the gallery grid. " +
                 "Each direct child is staggered-animated on OpenGallery().")]
        [SerializeField] private Transform thumbnailGrid;

        [Tooltip("Scale punch applied to each thumbnail when they appear.")]
        [SerializeField] private Vector3 thumbPunchScale = new Vector3(0.12f, 0.12f, 0f);

        [Tooltip("Duration of the per-thumbnail punch animation.")]
        [SerializeField] private float thumbPunchDuration = 0.3f;

        [Tooltip("Stagger delay between successive thumbnail animations.")]
        [SerializeField] private float thumbStagger = 0.04f;

        [Header("Unlock Toast")]
        [Tooltip("Root RectTransform of the unlock notification toast panel.")]
        [SerializeField] private RectTransform unlockToastRoot;

        [Tooltip("CanvasGroup on the unlock toast panel.")]
        [SerializeField] private CanvasGroup unlockToastGroup;

        [Tooltip("Pixel offset from which the toast slides in.")]
        [SerializeField] private float toastSlideOffset = 50f;

        [Tooltip("Duration for the toast to slide in.")]
        [SerializeField] private float toastInDuration = 0.3f;

        [Tooltip("How long the toast is held visible.")]
        [SerializeField] private float toastHoldDuration = 2f;

        [Tooltip("Duration for the toast to fade out.")]
        [SerializeField] private float toastOutDuration = 0.2f;

        [Tooltip("DOTween ease for the toast slide-in.")]
        [SerializeField] private Ease toastEase = Ease.OutBack;

        // -------------------------------------------------------------------------

        private GalleryManager _gm;
        private Sequence        _toastSequence;

        private void Awake()
        {
            _gm = GetComponent<GalleryManager>() ?? FindFirstObjectByType<GalleryManager>();
            if (_gm == null) Debug.LogWarning("[GalleryManager/DotweenGalleryBridge] GalleryManager not found.");

            if (unlockToastGroup != null) unlockToastGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            if (_gm != null) _gm.OnEntryUnlocked += OnEntryUnlocked;
        }

        private void OnDisable()
        {
            if (_gm != null) _gm.OnEntryUnlocked -= OnEntryUnlocked;
        }

        // -------------------------------------------------------------------------

        /// <summary>
        /// Call this to open the gallery with animated panel fade and thumbnail stagger.
        /// Wire to your "Gallery" button's onClick instead of / in addition to setting
        /// the gallery panel active directly.
        /// </summary>
        public void OpenGallery()
        {
            if (galleryPanelGroup != null)
            {
                DOTween.Kill(galleryPanelGroup);
                galleryPanelGroup.alpha = 0f;
                galleryPanelGroup.DOFade(1f, panelFadeInDuration).SetEase(panelFadeEase);
            }

            if (thumbnailGrid != null)
            {
                for (int i = 0; i < thumbnailGrid.childCount; i++)
                {
                    var child = thumbnailGrid.GetChild(i);
                    DOTween.Kill(child);
                    child.DOPunchScale(thumbPunchScale, thumbPunchDuration, 5, 0.4f)
                         .SetDelay(i * thumbStagger);
                }
            }
        }

        // -------------------------------------------------------------------------

        private void OnEntryUnlocked(string id)
        {
            if (unlockToastRoot == null) return;

            _toastSequence?.Kill();
            _toastSequence = DOTween.Sequence();

            Vector2 finalPos = unlockToastRoot.anchoredPosition;
            unlockToastRoot.anchoredPosition = finalPos + Vector2.down * toastSlideOffset;
            if (unlockToastGroup != null) unlockToastGroup.alpha = 0f;

            _toastSequence
                .Join(unlockToastRoot.DOAnchorPos(finalPos, toastInDuration).SetEase(toastEase));

            if (unlockToastGroup != null)
                _toastSequence.Join(unlockToastGroup.DOFade(1f, toastInDuration));

            _toastSequence.AppendInterval(toastHoldDuration);

            if (unlockToastGroup != null)
                _toastSequence.Append(unlockToastGroup.DOFade(0f, toastOutDuration));
        }
    }
}
#else
namespace GalleryManager.Runtime
{
    /// <summary>No-op stub — enable define <c>GALLERYMANAGER_DOTWEEN</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("GalleryManager/DOTween Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class DotweenGalleryBridge : UnityEngine.MonoBehaviour { }
}
#endif
