using UnityEditor;

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
    SerializedProperty canSmelt_Prop;
    SerializedProperty smeltItem_Prop;
    SerializedProperty canBake_Prop;
    SerializedProperty bakeItem_Prop;
    SerializedProperty bakeTime_Prop;
    SerializedProperty canEat_Prop;
    SerializedProperty hungerAmount_Prop;

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
        canSmelt_Prop = serializedObject.FindProperty("canSmelt");
        smeltItem_Prop = serializedObject.FindProperty("smeltItem");
        canBake_Prop = serializedObject.FindProperty("canBake");
        bakeItem_Prop = serializedObject.FindProperty("bakeItem");
        bakeTime_Prop = serializedObject.FindProperty("bakeTime");
        canEat_Prop = serializedObject.FindProperty("canEat");
        hungerAmount_Prop = serializedObject.FindProperty("hungerAmount");
        
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(idName_Prop);
        EditorGUILayout.PropertyField(itemName_Prop);
        EditorGUILayout.PropertyField(explain_Prop);
        EditorGUILayout.PropertyField(itemIcon_Prop);
        EditorGUILayout.PropertyField(maxAmount_Prop);

        EditorGUILayout.PropertyField(itemType_Prop);
     
        if ((Define.ItemType)itemType_Prop.enumValueIndex == Define.ItemType.Building)
        {
            EditorGUILayout.PropertyField(buildTile_Prop);
        }
        else
        {
            EditorGUILayout.PropertyField(itemPrefab_Prop);
        }

        EditorGUILayout.PropertyField(canSmelt_Prop);
        if (canSmelt_Prop.boolValue)
        {
            EditorGUILayout.PropertyField(smeltItem_Prop);
        }

        EditorGUILayout.PropertyField(canBake_Prop);
        if(canBake_Prop.boolValue)
        {
            EditorGUILayout.PropertyField(bakeItem_Prop);
            EditorGUILayout.PropertyField(bakeTime_Prop);
        }

        EditorGUILayout.PropertyField(canEat_Prop);
        if (canEat_Prop.boolValue)
        {
            EditorGUILayout.PropertyField(hungerAmount_Prop);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
