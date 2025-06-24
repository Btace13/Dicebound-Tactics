using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Dice))]
public class DiceEditor : Editor
{
    private Dice dice;
    private SerializedProperty sidesProp;

    private void OnEnable()
    {
        dice = (Dice)target;
        sidesProp = serializedObject.FindProperty("sides");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Dice Editor", EditorStyles.boldLabel);

        for (int i = 0; i < 6; i++)
        {
            if (i >= dice.sides.Count)
            {
                dice.sides.Add(new DiceSide { value = i + 1 });
            }

            SerializedProperty sideProp = sidesProp.GetArrayElementAtIndex(i);
            SerializedProperty sideNumberProp = sideProp.FindPropertyRelative("value");
            SerializedProperty modifierProp = sideProp.FindPropertyRelative("modifier");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Side " + (i + 1), EditorStyles.boldLabel);

            sideNumberProp.intValue = i + 1;
            EditorGUILayout.PropertyField(modifierProp, new GUIContent("Modifier"));

            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}