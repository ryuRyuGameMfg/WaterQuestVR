using UnityEngine;
using UnityEditor;

/// <summary>
/// SimpleGridPlacerのカスタムエディター
/// インスペクターに実行ボタンを表示します
/// </summary>
[CustomEditor(typeof(SimpleGridPlacer))]
public class SimpleGridPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // デフォルトのインスペクターを表示
        DrawDefaultInspector();

        SimpleGridPlacer placer = (SimpleGridPlacer)target;

        // 子オブジェクトの情報を表示
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("検出情報", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"子オブジェクト数: {placer.transform.childCount}");

        // 検出ボタン
        if (GUILayout.Button("子オブジェクトを再検出", GUILayout.Height(25)))
        {
            placer.DetectChildren();
            EditorUtility.SetDirty(placer);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("開発用ツール", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // 配置ボタン
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("すべてのオブジェクトを配置", GUILayout.Height(30)))
        {
            // 子オブジェクトの位置をUndoに記録（親オブジェクトは記録しない）
            for (int i = 0; i < placer.transform.childCount; i++)
            {
                Transform child = placer.transform.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                {
                    Undo.RecordObject(child, "グリッド配置");
                }
            }

            placer.PlaceAllObjectsOnGrid();

            // 変更をマーク（親オブジェクトは変更しない）
            for (int i = 0; i < placer.transform.childCount; i++)
            {
                Transform child = placer.transform.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                {
                    EditorUtility.SetDirty(child);
                }
            }
            EditorUtility.SetDirty(placer);
        }

        // リセットボタン
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("配置をリセット", GUILayout.Height(30)))
        {
            Undo.RecordObject(placer.transform, "配置リセット");
            placer.ResetPlacement();
            EditorUtility.SetDirty(placer);
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("開発用ツール: 親オブジェクトの位置は変更せず、子オブジェクトだけを均等に配置します。", MessageType.Info);
    }
}

