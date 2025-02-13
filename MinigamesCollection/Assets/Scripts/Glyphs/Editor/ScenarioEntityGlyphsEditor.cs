using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScenarioEntityGlyphs))]
public class ScenarioEntityGlyphsEditor : Editor
{
    SerializedProperty settingsProp;
    SerializedProperty twoRingSetupProp;
    SerializedProperty smallRingsAmountProp;

    private void OnEnable()
    {
        settingsProp = serializedObject.FindProperty("settings");
        twoRingSetupProp = serializedObject.FindProperty("twoRingSetup");
        smallRingsAmountProp = serializedObject.FindProperty("smallRingsAmount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all properties except the ones we want to conditionally display
        DrawPropertiesExcluding(serializedObject, "twoRingSetup", "smallRingsAmount");

        if ((GlyphsDifficultyLevel)settingsProp.enumValueIndex == GlyphsDifficultyLevel.Fixed)
        {
            EditorGUILayout.PropertyField(twoRingSetupProp);
            EditorGUILayout.PropertyField(smallRingsAmountProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
