using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemSO))]
public class EditorInspector : Editor
{
    SerializedProperty idName_Prop;
    SerializedProperty itemName_Prop;
    SerializedProperty explain_Prop;
    SerializedProperty itemIcon_Prop;
    SerializedProperty maxAmount_Prop;
    SerializedProperty itemType_Prop;
    SerializedProperty itemPrefab_Prop;
    SerializedProperty buildTile_Prop;

    private void Awake()
    {
        idName_Prop = serializedObject.FindProperty("idName");
        itemName_Prop = serializedObject.FindProperty("itemName");
        explain_Prop = serializedObject.FindProperty("explain");
        itemIcon_Prop = serializedObject.FindProperty("itemIcon");
        itemType_Prop = serializedObject.FindProperty("itemType");
        maxAmount_Prop = serializedObject.FindProperty("maxAmount");
        itemPrefab_Prop = serializedObject.FindProperty("itemPrefab");
        buildTile_Prop = serializedObject.FindProperty("buildTile");
        
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(idName_Prop);
        EditorGUILayout.PropertyField(itemName_Prop);
        EditorGUILayout.PropertyField(explain_Prop);
        EditorGUILayout.PropertyField(itemIcon_Prop);
        EditorGUILayout.PropertyField(maxAmount_Prop);

        EditorGUILayout.PropertyField(itemType_Prop);
        if((Define.ItemType)itemType_Prop.enumValueIndex == Define.ItemType.Pick)
        {
            EditorGUILayout.PropertyField(itemPrefab_Prop);
        }
        if ((Define.ItemType)itemType_Prop.enumValueIndex == Define.ItemType.Building)
        {
            EditorGUILayout.PropertyField(buildTile_Prop);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
