using UnityEngine;
using System.Collections;

public class RocketWebhookPresentation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform rocketPrefab;

    [Header("Rockets")]
    [SerializeField] private int rocketCount = 5;

    [SerializeField] private float horizontalRange = 400f;

    [SerializeField] private float startY = -900f;
    [SerializeField] private float endY = 900f;

    [SerializeField] private float minDuration = 0.4f;
    [SerializeField] private float maxDuration = 0.7f;

    [SerializeField] private float spawnDelay = 0.08f;

    public void Play()
    {
        gameObject.SetActive(true);

        StartCoroutine(
            SpawnSequence()
        );
    }

    private IEnumerator SpawnSequence()
    {
        for (int i = 0; i < rocketCount; i++)
        {
            SpawnRocket();

            yield return new WaitForSeconds(
                spawnDelay
            );
        }

        yield return new WaitForSeconds(
            maxDuration
        );

        gameObject.SetActive(false);
    }

    private void SpawnRocket()
    {
        RectTransform rocket =
            Instantiate(
                rocketPrefab,
                container
            );

        rocket.gameObject.SetActive(true);

        float x =
            Random.Range(
                -horizontalRange,
                horizontalRange
            );

        rocket.anchoredPosition =
            new Vector2(
                x,
                startY
            );

        float duration =
            Random.Range(
                minDuration,
                maxDuration
            );

        StartCoroutine(
            MoveRocket(
                rocket,
                duration
            )
        );
    }

    private IEnumerator MoveRocket(
        RectTransform rocket,
        float duration)
    {
        Vector2 start =
            rocket.anchoredPosition;

        Vector2 end =
            new Vector2(
                start.x,
                endY
            );

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            // Accelerate upward.
            float eased = t * t;

            rocket.anchoredPosition =
                Vector2.Lerp(
                    start,
                    end,
                    eased
                );

            yield return null;
        }

        Destroy(rocket.gameObject);
    }
}
