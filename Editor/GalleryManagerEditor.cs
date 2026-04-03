#if UNITY_EDITOR
using GalleryManager.Runtime;
using UnityEditor;
using UnityEngine;

namespace GalleryManager.Editor
{
    /// <summary>
    /// Custom Inspector for <see cref="GalleryManager.Runtime.GalleryManager"/>.
    /// Shows entry unlock status and provides unlock/lock controls at runtime.
    /// </summary>
    [CustomEditor(typeof(GalleryManager.Runtime.GalleryManager))]
    public class GalleryManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(6);

            // ── Validation ──────────────────────────────────────────────────────

            var entriesProp = serializedObject.FindProperty("entries");
            if (entriesProp != null && entriesProp.arraySize == 0)
                EditorGUILayout.HelpBox(
                    "No gallery entries defined. Add entries in the Inspector.",
                    MessageType.Info);

            // ── Runtime controls (Play Mode only) ───────────────────────────────

            if (!Application.isPlaying) return;

            var mgr = (GalleryManager.Runtime.GalleryManager)target;

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Gallery Entries", EditorStyles.boldLabel);

            var entries = mgr.Entries;
            if (entries.Count == 0)
            {
                EditorGUILayout.LabelField("  (none defined)");
            }
            else
            {
                foreach (var e in entries)
                {
                    if (e == null) continue;
                    bool unlocked = e.alwaysUnlocked || mgr.IsUnlocked(e.id);
                    EditorGUILayout.BeginHorizontal();
                    string label = $"[{e.type}]  {e.displayName ?? e.id}";
                    EditorGUILayout.LabelField(label);
                    EditorGUILayout.LabelField(
                        e.alwaysUnlocked ? "always" : (unlocked ? "✓ Unlocked" : "— Locked"),
                        GUILayout.Width(90));
                    GUI.enabled = !unlocked && !e.alwaysUnlocked;
                    if (GUILayout.Button("Unlock", GUILayout.Width(60))) mgr.Unlock(e.id);
                    GUI.enabled = unlocked && !e.alwaysUnlocked;
                    if (GUILayout.Button("Lock",   GUILayout.Width(55))) mgr.Lock(e.id);
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock All")) mgr.UnlockAll();
            if (GUILayout.Button("Lock All"))   mgr.LockAll();
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
