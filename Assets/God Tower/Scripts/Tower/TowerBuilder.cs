using UnityEngine;

public class TowerBuilder : MonoBehaviour
{
    [Header("Modules")]
    [SerializeField] private GameObject towerBasePrefab;
    [SerializeField] private GameObject towerBodyPrefab;
    [SerializeField] private GameObject towerSummitPrefab;

    [Header("Dimensions")]
    [SerializeField] private float moduleHeight = 18.5f;

    [Header("Generated Content")]
    [SerializeField] private Transform moduleContainer;

    [Header("References")]
    [SerializeField] private LevelHUD levelHUD;

    public float TowerHeight { get; private set; }

    private void Start()
    {
        BuildTower();
    }

    public void BuildTower()
    {
        LevelConfig levelConfig = LevelManager.Instance.GetLevelConfig();
        if (levelConfig == null)
        {
            Debug.LogError(
                "TowerBuilder has no LevelConfig assigned."
            );

            return;
        }

        ClearTower();

        float currentHeight = 0f;

        // Base
        SpawnModule(
            towerBasePrefab,
            currentHeight
        );

        // Repeating body sections
        for (int i = 0;
             i < levelConfig.BodyModuleCount;
             i++)
        {
            currentHeight += moduleHeight;

            SpawnModule(
                towerBodyPrefab,
                currentHeight
            );
        }

        // Summit
        currentHeight += moduleHeight;

        SpawnModule(
            towerSummitPrefab,
            currentHeight
        );

        // Include the Summit pillar itself.
        TowerHeight = currentHeight + moduleHeight;

        Debug.Log(
            $"{levelConfig.LevelName} tower built. " +
            $"Height: {TowerHeight}"
        );

        levelHUD.InitializeProgress();
    }

    private void SpawnModule(
        GameObject prefab,
        float height
    )
    {
        if (prefab == null)
            return;

        GameObject module =
            Instantiate(
                prefab,
                moduleContainer
            );

        module.transform.localPosition =
            new Vector3(
                0f,
                height,
                0f
            );

        module.transform.localRotation =
            Quaternion.identity;

        module.transform.localScale =
            Vector3.one;
    }

    private void ClearTower()
    {
        if (moduleContainer == null)
            return;

        for (int i =
                 moduleContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                moduleContainer
                    .GetChild(i)
                    .gameObject
            );
        }
    }
}