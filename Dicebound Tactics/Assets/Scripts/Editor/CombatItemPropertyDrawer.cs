using UnityEngine;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CombatItem), true)]
public class CombatItemPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // Draw the object field with proper filtering
        EditorGUI.ObjectField(position, property, typeof(CombatItem), label);
        
        EditorGUI.EndProperty();
    }
}

// Custom drawer for CombatItemEntry to show items and quantities together nicely
[CustomPropertyDrawer(typeof(CombatItemEntry))]
public class CombatItemEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var itemProperty = property.FindPropertyRelative("item");
        var quantityProperty = property.FindPropertyRelative("quantity");
        
        // Calculate rects
        var itemRect = new Rect(position.x, position.y, position.width * 0.7f, position.height);
        var quantityRect = new Rect(position.x + position.width * 0.72f, position.y, position.width * 0.28f, position.height);
        
        // Draw fields
        EditorGUI.ObjectField(itemRect, itemProperty, new GUIContent("Item"));
        EditorGUI.PropertyField(quantityRect, quantityProperty, GUIContent.none);
        
        EditorGUI.EndProperty();
    }
}
#endif
