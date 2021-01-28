using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelDataScriptableObject))]
public class LevelDataEditor : Editor
{
    LevelDataScriptableObject levelData;

    public void OnEnable()
    {
        levelData = (LevelDataScriptableObject)target;
        if (levelData.LevelData.Length < LevelManager.LEVEL_HEIGHT * LevelManager.LEVEL_WIDTH)
        {
            levelData.LevelData = new bool[LevelManager.LEVEL_HEIGHT * LevelManager.LEVEL_WIDTH];
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Level Editor");

        for (int y = 0; y < LevelManager.LEVEL_HEIGHT; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < LevelManager.LEVEL_WIDTH; x++)
            {
                levelData.LevelData[y * LevelManager.LEVEL_WIDTH + x] = EditorGUILayout.Toggle(levelData.LevelData[y * LevelManager.LEVEL_WIDTH + x], GUILayout.Width(20));
            }
            EditorGUILayout.EndHorizontal();
        }

        
    }
}
