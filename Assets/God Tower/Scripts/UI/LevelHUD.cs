using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ClimbingPlayer climbingPlayer;
    [SerializeField] private TowerBuilder towerBuilder;

    [Header("Level")]
    [SerializeField] private TMP_Text levelNameText;

    [Header("Progress")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text totalScoreText;

    [Header("Summit")]
    [SerializeField] private GameObject summitPanel;
    [SerializeField] private TMP_Text summitCountdownText;
    [SerializeField] private Slider summitProgressSlider;
    [SerializeField] private Slider summitProgressSlider2;

    [Header("Score")]
    [SerializeField] private float pointsPerUnit = 100f;

    private int totalScore;
    private LevelConfig levelConfig;

    private void Update()
    {
        UpdateClimbProgress();
        UpdateSummitUI();
    }

    public void Configure(LevelConfig levelConfig)
    {
        this.levelConfig = levelConfig;

        if (levelNameText != null)
        {
            levelNameText.text =
                levelConfig.LevelName.ToUpper();
        }
    }

    public void InitializeProgress()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }

        Debug.Log($"LevelHUD: Initializing progress for level '{levelConfig.LevelName}' with {levelConfig.BodyModuleCount} body modules and {towerBuilder.TowerHeight} tower height.");
        totalScore =
            Mathf.RoundToInt(
                // Calculate tower height with a buffer of 2 units for the base and the summit
                (levelConfig.BodyModuleCount + 2) *
                towerBuilder.TowerHeight *
                pointsPerUnit
            );

        if (totalScoreText != null)
            totalScoreText.text = totalScore.ToString("N0");

        if (currentScoreText != null)
            currentScoreText.text = "0";

        if (summitPanel != null)
            summitPanel.SetActive(false);
    }

    private void UpdateClimbProgress()
    {
        if (climbingPlayer == null ||
            towerBuilder == null)
        {
            return;
        }

        // Calculate tower height with a buffer of 2 units for the base and the summit
        float towerHeight =
            towerBuilder.TowerHeight + 2;

        if (towerHeight <= 0f)
            return;

        float progress =
        climbingPlayer.CurrentState ==
            ClimbingState.Summit ||
        climbingPlayer.CurrentState ==
            ClimbingState.Won
            ? 1f
            : Mathf.Clamp01(
                climbingPlayer.Height /
                towerHeight
            );

        if (progressSlider != null)
            progressSlider.value = progress;

        int currentScore =
            Mathf.RoundToInt(
                totalScore * progress
            );

        if (currentScoreText != null)
            currentScoreText.text =
                currentScore.ToString("N0");
    }

    private void UpdateSummitUI()
    {
        bool isAtSummit =
            climbingPlayer.CurrentState ==
            ClimbingState.Summit;

        if (summitPanel != null)
            summitPanel.SetActive(isAtSummit);

        if (!isAtSummit)
            return;

        if (summitCountdownText != null)
        {
            summitCountdownText.text =
                climbingPlayer
                    .SummitTimeRemaining
                    .ToString("0.0");
        }

        if (summitProgressSlider != null)
        {
            summitProgressSlider.value =
                1 - climbingPlayer.SummitProgress;
        }

        if (summitProgressSlider2 != null)
        {
            summitProgressSlider2.value =
                1 - climbingPlayer.SummitProgress;
        }
    }
}