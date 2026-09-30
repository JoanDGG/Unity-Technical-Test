using UnityEngine;


[CreateAssetMenu(fileName = "LevelConfig", menuName = "God Tower/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level")]
    [SerializeField] private int levelIndex = 1;
    [SerializeField] private string levelName = "Level 1";

    [Header("Tower")]
    [Min(0)]
    [SerializeField] private int bodyModuleCount = 0;

    [Header("Handholds")]
    [Min(0.1f)]
    [SerializeField] private float handholdVerticalSpacing = 0.75f;

    [Min(3)]
    [SerializeField] private int handholdsPerRing = 8;

    [Header("Climbing")]
    [Min(0.1f)]
    [SerializeField] private float climbDuration = 0.55f;

    [Header("Summit")]
    [Min(0.1f)]
    [SerializeField] private float summitSurvivalDuration = 5f;

    [Header("Environment")]
    [SerializeField] private Material skyboxMaterial;

    public Material SkyboxMaterial => skyboxMaterial;
    public int LevelIndex => levelIndex;
    public string LevelName => levelName;

    public int BodyModuleCount => bodyModuleCount;

    public float HandholdVerticalSpacing =>
        handholdVerticalSpacing;

    public int HandholdsPerRing =>
        handholdsPerRing;

    public float ClimbDuration =>
        climbDuration;

    public float SummitSurvivalDuration =>
        summitSurvivalDuration;
}