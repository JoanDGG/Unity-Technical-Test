using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level")]
    [SerializeField] private LevelConfig debugLevel;
    [SerializeField] private Levels levels;
    private LevelConfig currentLevel;

    [Header("Systems")]
    [SerializeField] private TowerBuilder towerBuilder;
    [SerializeField] private TowerHandholdGenerator handholdGenerator;
    [SerializeField] private ClimbingPlayer climbingPlayer;
    [SerializeField] private LevelHUD levelHUD;
    [SerializeField] private WinScreen winScreen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError(
                "Multiple instances of LevelManager detected. Destroying duplicate."
            );
            Destroy(gameObject);
            return;
        }

        if (GameSession.SelectedLevel != null)
        {
            currentLevel =
                GameSession.SelectedLevel;
        }
        else
        {
            currentLevel = debugLevel;
        }

        ApplyLevelConfig();
    }

    private void OnEnable()
    {
        climbingPlayer.OnWin += HandleLevelWon;
    }

    private void OnDisable()
    {
        climbingPlayer.OnWin -= HandleLevelWon;
    }

    private void HandleLevelWon()
    {
        LevelProgressionManager.CompleteLevel(
            currentLevel.LevelIndex
        );
    }

    private void ApplyLevelConfig()
    {
        if (currentLevel == null)
        {
            Debug.LogError(
                "LevelManager has no LevelConfig assigned."
            );

            return;
        }

        handholdGenerator.Configure(
            currentLevel.BodyModuleCount,
            currentLevel.HandholdVerticalSpacing,
            currentLevel.HandholdsPerRing
        );

        climbingPlayer.Configure(
            currentLevel.ClimbDuration,
            currentLevel.SummitSurvivalDuration
        );

        levelHUD.Configure(currentLevel);

        winScreen.Configure(currentLevel, levels.collection);

        ApplyEnvironment();
    }

    private void ApplyEnvironment()
    {
        Debug.Log(
            $"Applying environment for level: {currentLevel.LevelName}"
        );
        if (currentLevel.SkyboxMaterial == null)
            return;

        RenderSettings.skybox =
            currentLevel.SkyboxMaterial;

        DynamicGI.UpdateEnvironment();
    }

    public void LoadNextLevel()
    {
        int nextIndex =
            currentLevel.LevelIndex;

        if (nextIndex >= levels.collection.Count)
        {
            SceneManager.LoadScene(
                "MainMenu"
            );

            return;
        }

        GameSession.SelectedLevel =
            levels.collection[nextIndex];

        SceneManager.LoadScene(
            "Gameplay"
        );
    }

    public LevelConfig GetLevelConfig() => currentLevel;
}