using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ClimbingPlayer climbingPlayer;

    [Header("UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text levelCompleteText;
    [SerializeField] private GameObject continueButton;

    private LevelConfig currentLevel;
    private List<LevelConfig> levels;

    private void Awake()
    {
        winPanel.SetActive(false);
    }

    private void OnEnable()
    {
        climbingPlayer.OnWin += Show;
    }

    private void OnDisable()
    {
        climbingPlayer.OnWin -= Show;
    }

    private void Show()
    {
        winPanel.SetActive(true);
    }

    public void Configure(
        LevelConfig levelConfig,
        List<LevelConfig> allLevels)
    {
        currentLevel = levelConfig;
        levels = allLevels;

        UpdateContinueButton();
    }

    private void UpdateContinueButton()
    {
        bool isLastLevel =
            currentLevel.LevelIndex >= levels.Count;

        continueButton.SetActive(!isLastLevel);
    }

    public void Continue()
    {
        bool isLastLevel =
            currentLevel.LevelIndex >= levels.Count;

        if (isLastLevel)
        {
            LoadMainMenu();
            return;
        }

        // LevelIndex starts at 1, array starts at 0.
        // Therefore Level 1's next level is levels[1].
        GameSession.SelectedLevel =
            levels[currentLevel.LevelIndex];

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}