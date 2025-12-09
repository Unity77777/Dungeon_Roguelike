using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timeText;  // UI 연결
    private float elapsedTime = 0f;   // 누적 시간(초)

    private bool isRunning = true;    // 시간 진행 여부

    private void Update()
    {
        if (!isRunning)
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimeUI();
    }

    private void UpdateTimeUI()
    {
        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);

        timeText.text = $"{minutes:D2}:{seconds:D2}";
    }

    public void Pause()
    {
        isRunning = false;
    }

    public void Resume()
    {
        isRunning = true;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}