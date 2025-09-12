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
    public GameObject[] otherUIPanels; // ���� �ٸ� UI �гε�

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
        // GameManager���� �ϼ��� �ð� ��������
        int days, hours;
        GameManager.Instance.GetGameTimeInfo(out days, out hours);

        // ���� �ð� �ؽ�Ʈ
        if (survivalTimeText != null)
        {
            if (days > 0)
                survivalTimeText.text = $"����� {days}�� {hours}�ð� ���� �����߽��ϴ�.";
            else
                survivalTimeText.text = $"����� {hours}�ð� ���� �����߽��ϴ�.";
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