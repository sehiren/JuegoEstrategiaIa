using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
    [CustomEditor(typeof(DrawGrid))]
public class DrawGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawGrid grid = (DrawGrid)target;

        if (grid.nodePrefab == null)
            EditorGUILayout.HelpBox("The Tile prefab is missing", MessageType.Warning, true);

        if (grid.gridLayout == null)
            EditorGUILayout.HelpBox("The gridLayout reference is missing", MessageType.Warning, true);
    }
}
#endif
