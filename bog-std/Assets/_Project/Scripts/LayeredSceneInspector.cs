using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LayeredScene))]
public class LayeredSceneInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // serializedObject.Update();
        // // EditorList.Show(serializedObject.FindProperty("Layers"));
        // // EditorGUILayout.PropertyField(serializedObject.FindProperty("Layers"), true);
        // serializedObject.ApplyModifiedProperties();
    }
}
