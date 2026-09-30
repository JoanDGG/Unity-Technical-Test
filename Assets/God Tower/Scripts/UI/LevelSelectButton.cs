using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private LevelConfig levelConfig;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject lockIcon;

    [Header("References")]
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    private void Start()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (levelConfig == null)
            return;

        bool unlocked =
            LevelProgressionManager.IsLevelUnlocked(
                levelConfig.LevelIndex
            );

        button.interactable = unlocked;

        if (levelText != null)
        {
            levelText.text =
                levelConfig.LevelIndex.ToString();
        }

        if (lockIcon != null)
        {
            lockIcon.SetActive(!unlocked);
        }
    }

    public void SelectLevel()
    {
        if (levelConfig == null)
            return;

        if (!LevelProgressionManager.IsLevelUnlocked(
            levelConfig.LevelIndex))
        {
            return;
        }

        GameSession.SelectedLevel =
            levelConfig;

        SceneManager.LoadScene(
            "Gameplay"
        );
    }
}