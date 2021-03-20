using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AeroSurface)), CanEditMultipleObjects()]
public class AeroSurfaceEditor : Editor
{
    SerializedProperty config;
    SerializedProperty isControlSurface;
    SerializedProperty inputType;
    SerializedProperty inputMultiplyer;
    SerializedProperty aircraft;
    AeroSurface surface;

    private void OnEnable()
    {
        config = serializedObject.FindProperty("config");
        isControlSurface = serializedObject.FindProperty("IsControlSurface");
        inputType = serializedObject.FindProperty("InputType");
        inputMultiplyer = serializedObject.FindProperty("InputMultiplyer");
        aircraft = serializedObject.FindProperty("aircraft");
        surface = target as AeroSurface;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(config);
        EditorGUILayout.PropertyField(isControlSurface);
        if (surface.IsControlSurface)
        {
            EditorGUILayout.PropertyField(inputType);
            EditorGUILayout.PropertyField(inputMultiplyer);
        }
        EditorGUILayout.PropertyField(aircraft);
        serializedObject.ApplyModifiedProperties();
    }
}
