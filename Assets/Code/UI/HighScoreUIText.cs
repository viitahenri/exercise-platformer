using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HighScoreUIText : MonoBehaviour
{
    private TextMeshProUGUI _text;

    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _text.text = PlayerPrefs.GetInt(LevelManager.HIGH_SCORE_PREFS_NAME).ToString();
    }
}
