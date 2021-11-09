using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorSortChildren : ScriptableObject {
    [MenuItem("GameObject/Sort Children")]
    public static void SortActiveTransformChildren() {
        if (Selection.activeTransform) {
            Sort(Selection.activeTransform);
        }
        else {
            Debug.LogErrorFormat("No game object selected in Hierarchy");
        }
    }

    private static void Sort(Transform current) {
        IOrderedEnumerable<Transform> orderedChildren = current.Cast<Transform>().OrderBy(tr => tr.name);

        foreach (Transform child in orderedChildren) {
            Undo.SetTransformParent(child, null, "Reorder children");
            Undo.SetTransformParent(child, current, "Reorder children");
        }
    }
}