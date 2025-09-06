using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TileData))]
public class TileDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // インデント管理
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // type フィールドを取得
        var typeProp = property.FindPropertyRelative("type");
        var gimmickProp = property.FindPropertyRelative("gimmickPrefab");

        // 行の高さ
        Rect typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(typeRect, typeProp);

        // TileType が Gimmick のときだけ gimmickPrefab を表示
        if ((TileType)typeProp.enumValueIndex == TileType.Gimmick)
        {
            Rect gimmickRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + 2,
                position.width,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.PropertyField(gimmickRect, gimmickProp);
        }

        EditorGUI.indentLevel = indent;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var typeProp = property.FindPropertyRelative("type");

        // Gimmick のときは 2 行分、それ以外は 1 行分
        if ((TileType)typeProp.enumValueIndex == TileType.Gimmick)
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        else
            return EditorGUIUtility.singleLineHeight;
    }
}
