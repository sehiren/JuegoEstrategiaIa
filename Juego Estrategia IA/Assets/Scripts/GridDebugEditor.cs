using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridDebug))]
public class GridDebugEditor : Editor
{
    private GUIStyle toggleButtonNormalState = null;
    private GUIStyle toggleButtonToggledState = null;

    public override void OnInspectorGUI()
    {
        GridDebug grid = (GridDebug)target;

        if(toggleButtonNormalState == null)
        {
            toggleButtonNormalState = "Button";
            toggleButtonToggledState = new GUIStyle(toggleButtonNormalState);
            toggleButtonToggledState.normal.background = toggleButtonNormalState.active.background;
        }


        GUILayout.BeginVertical();
        if(GUILayout.Button("Mostrar grid", grid.showGizmos ? toggleButtonToggledState : toggleButtonNormalState))
        {
            grid.showGizmos = !grid.showGizmos;
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Mostrar puntos medios", grid.showCenter ? toggleButtonToggledState : toggleButtonNormalState))
        {
            grid.showCenter = !grid.showCenter;
            SceneView.RepaintAll();
        }
        GUILayout.EndVertical();
    }
}
