using System.Collections;
using UnityEngine;

public class CarWebhookPresentation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform car;

    [Header("Movement")]
    [SerializeField] private float startY = 900f;
    [SerializeField] private float pushY = 0f;
    [SerializeField] private float endY = -900f;

    [SerializeField] private float enterDuration = 0.2f;
    [SerializeField] private float pushDuration = 0.45f;
    [SerializeField] private float exitDuration = 0.25f;

    [Header("Push Wiggle")]
    [SerializeField] private float wiggleDistance = 25f;
    [SerializeField] private float wiggleSpeed = 35f;

    private Coroutine playRoutine;

    private void Start()
    {
        car.gameObject.SetActive(false);
    }

    public void Play()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine =
            StartCoroutine(
                PlaySequence()
            );
    }

    private IEnumerator PlaySequence()
    {
        car.gameObject.SetActive(true);

        SetY(startY);

        // -------------------------
        // ENTER
        // -------------------------

        yield return MoveY(
            startY,
            pushY,
            enterDuration
        );

        // -------------------------
        // PUSH / WIGGLE
        // -------------------------

        float timer = 0f;

        while (timer < pushDuration)
        {
            timer += Time.deltaTime;

            float wiggle =
                Mathf.Sin(
                    timer * wiggleSpeed
                ) * wiggleDistance;

            SetY(pushY + wiggle);

            yield return null;
        }

        // -------------------------
        // EXIT
        // -------------------------

        yield return MoveY(
            pushY,
            endY,
            exitDuration
        );

        car.gameObject.SetActive(false);

        playRoutine = null;
    }

    private IEnumerator MoveY(
        float from,
        float to,
        float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            float eased =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            SetY(
                Mathf.Lerp(
                    from,
                    to,
                    eased
                )
            );

            yield return null;
        }

        SetY(to);
    }

    private void SetY(float y)
    {
        Vector2 position =
            car.anchoredPosition;

        position.y = y;

        car.anchoredPosition =
            position;
    }
}
