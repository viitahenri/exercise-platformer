using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Level Data/LevelDataScriptableObject", order = 1)]
public class LevelDataScriptableObject : ScriptableObject
{
    public bool[] LevelData = new bool[LevelManager.LEVEL_HEIGHT * LevelManager.LEVEL_WIDTH];
}
