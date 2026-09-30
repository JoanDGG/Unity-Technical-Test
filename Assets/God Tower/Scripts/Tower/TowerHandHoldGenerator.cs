using System.Collections.Generic;
using UnityEngine;

public class TowerHandholdGenerator : MonoBehaviour
{
    [Header("Tower")]
    [SerializeField] private Transform towerCenter;
    [SerializeField] private float towerRadius = 2.15f;

    [Header("Generation")]
    [SerializeField] private GameObject handholdPrefab;
    [SerializeField] private float startHeight = 0.75f;
    [SerializeField] private float endHeight = 20f;
    [SerializeField] private float verticalSpacing = 0.75f;
    [SerializeField] private int holdsPerRing = 8;

    [Header("Handhold Grab")]
    [SerializeField] private Vector3 grabRotationOffset;

    [SerializeField] private float surfaceOffset = 0.05f;

    [Header("Layout")]
    [SerializeField] private bool staggerRings = true;

    public float StartHeight => startHeight;
    public float EndHeight => endHeight;
    private readonly List<Handhold> handholds = new();
    public IReadOnlyList<Handhold> Handholds => handholds;

    public void Configure(
        int bodyModuleCount,
        float verticalSpacing,
        int holdsPerRing
    )
    {
        endHeight += endHeight * (bodyModuleCount + 1);
        this.verticalSpacing = verticalSpacing;
        this.holdsPerRing = holdsPerRing;

        Generate();
    }

    public void Generate()
    {
        ClearExisting();

        handholds.Clear();

        if (towerCenter == null)
        {
            Debug.LogError("TowerHandholdGenerator requires a Tower Center.");
            return;
        }

        float angleStep = 360f / holdsPerRing;

        for (float height = startHeight; height <= endHeight; height += verticalSpacing)
        {
            int ringIndex = Mathf.RoundToInt(
                (height - startHeight) / verticalSpacing
            );

            float ringOffset =
                staggerRings && ringIndex % 2 != 0
                    ? angleStep * 0.5f
                    : 0f;

            for (int i = 0; i < holdsPerRing; i++)
            {
                float angle = i * angleStep + ringOffset;

                CreateHandhold(height, angle);
            }
        }
    }

    private void CreateHandhold(float height, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        Vector3 outward = new Vector3(
            Mathf.Sin(radians),
            0f,
            Mathf.Cos(radians)
        );

        Vector3 position =
            towerCenter.position +
            outward * (towerRadius + surfaceOffset);

        position.y = towerCenter.position.y + height;

        GameObject handholdObject = Instantiate(
            handholdPrefab,
            position,
            Quaternion.LookRotation(outward, Vector3.up),
            transform
        );

        handholdObject.name =
            $"Handhold_H{height:00.00}_A{angle:000.00}";

        Handhold handhold =
            handholdObject.AddComponent<Handhold>();

        handhold.Initialize(
            height,
            angle,
            outward,
            grabRotationOffset
        );

        handholds.Add(handhold);
    }

    private void ClearExisting()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    public Handhold FindStartingHandhold(
        float height,
        float angle)
    {
        Handhold closest = null;
        float closestScore = float.MaxValue;

        foreach (Handhold handhold in handholds)
        {
            float heightDifference =
                Mathf.Abs(handhold.Height - height);

            float angleDifference =
                Mathf.Abs(
                    Mathf.DeltaAngle(
                        angle,
                        handhold.Angle
                    )
                );

            float score =
                heightDifference * 2f +
                angleDifference * 0.02f;

            if (score < closestScore)
            {
                closestScore = score;
                closest = handhold;
            }
        }

        return closest;
    }

    public Handhold FindNextHandhold(
        float currentHeight,
        float currentAngle,
        float horizontalInput,
        float maxVerticalReach)
    {
        Handhold best = null;
        float bestScore = float.MaxValue;

        foreach (Handhold handhold in handholds)
        {
            float verticalDelta =
                handhold.Height - currentHeight;

            // We only want holds above the player.
            if (verticalDelta <= 0.05f)
                continue;

            // Don't allow impossible jumps.
            if (verticalDelta > maxVerticalReach)
                continue;

            float angleDelta =
                Mathf.DeltaAngle(
                    currentAngle,
                    handhold.Angle
                );

            float absoluteAngleDelta =
                Mathf.Abs(angleDelta);

            // Don't allow the player to reach absurdly far around
            // the tower in a single climbing step.
            if (absoluteAngleDelta > 100f)
                continue;

            float score = 0f;

            // Prefer nearby vertical steps.
            score += verticalDelta * 1.5f;

            // Prefer nearby handholds around the tower.
            score += absoluteAngleDelta * 0.025f;

            // Horizontal input biases the selected handhold.
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                bool wantsRight =
                    horizontalInput > 0f;

                bool holdIsRight =
                    angleDelta > 0f;

                if (wantsRight != holdIsRight)
                {
                    score += 10f;
                }
                else
                {
                    score -= 3f;
                }
            }

            if (score < bestScore)
            {
                bestScore = score;
                best = handhold;
            }
        }

        return best;
    }
}