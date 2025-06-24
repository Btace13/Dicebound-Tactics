using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dice))]
public class DiceEditor : Editor
{
    private SerializedProperty sidesProp;

    private void OnEnable()
    {
        sidesProp = serializedObject.FindProperty("sides");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Dice Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Show each side
        for (int i = 0; i < sidesProp.arraySize; i++)
        {
            SerializedProperty sideProp = sidesProp.GetArrayElementAtIndex(i);
            SerializedProperty modifierProp = sideProp.FindPropertyRelative("modifier");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Side " + (i + 1), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(modifierProp, new GUIContent("Modifier"));
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Side"))
        {
            sidesProp.InsertArrayElementAtIndex(sidesProp.arraySize);
        }

        if (GUILayout.Button("Remove Last Side") && sidesProp.arraySize > 0)
        {
            sidesProp.DeleteArrayElementAtIndex(sidesProp.arraySize - 1);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
