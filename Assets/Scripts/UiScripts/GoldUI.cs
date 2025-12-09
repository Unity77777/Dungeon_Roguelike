using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public PlayerStats playerStats;

    void Update()
    {
        goldText.text = $"°ñµå : {playerStats.gold}"; 
    }
}