using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public static HUD Instance;

    [SerializeField] private TextMeshProUGUI _coinAmountText;
    [SerializeField] private TextMeshProUGUI _timerText;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    public void SetCoinAmount(int amount)
    {
        _coinAmountText.text = $"{amount}";
    }

    public void SetTimer(int seconds)
    {
        _timerText.text = $"{seconds}";
    }
}
