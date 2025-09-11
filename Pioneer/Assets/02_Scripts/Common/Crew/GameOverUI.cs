using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI ���")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI crewStatsText;
    public Button restartButton;
    public Button titleButton;

    private void Start()
    {
        gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (titleButton != null)
            titleButton.onClick.AddListener(GoToTitle);
    }

    public void ShowGameOverScreen(int totalCrewMembers, int deadCrewMembers)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        UpdateGameOverTexts(totalCrewMembers, deadCrewMembers);
    }

    private void UpdateGameOverTexts(int totalCrewMembers, int deadCrewMembers)
    {
        // �ð� ���
        float totalGameTime = 0f;
        if (GameManager.Instance != null)
        {
            totalGameTime = GameManager.Instance.currentGameTime;
        }

        int totalDays = Mathf.FloorToInt(totalGameTime / (GameManager.Instance.dayDuration + GameManager.Instance.nightDuration));
        float remainingTime = totalGameTime % (GameManager.Instance.dayDuration + GameManager.Instance.nightDuration);
        int totalHours = Mathf.FloorToInt(remainingTime / 3600f * 24f);

        // ���� �ð� �ؽ�Ʈ
        if (survivalTimeText != null)
        {
            if (totalDays > 0)
                survivalTimeText.text = $"����� {totalDays}��, {totalHours}�ð� ���� �����߽��ϴ�.";
            else
                survivalTimeText.text = $"����� {totalHours}�ð� ���� �����߽��ϴ�.";
        }

        // �¹��� ��� �ؽ�Ʈ
        if (crewStatsText != null)
        {
            crewStatsText.text = $"����� �����ϴ� ���� �¹��� �� {totalCrewMembers}��� �Բ��ϰ�, {deadCrewMembers}���� �������� �����ҽ��ϴ�.";
        }
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }
}
