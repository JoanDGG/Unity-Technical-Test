using UnityEngine;
using System.Collections;

public class HandWebhookPresentation :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform hand;

    [Header("Movement")]
    [SerializeField] private float startY = -900f;
    [SerializeField] private float pushY = -50f;
    [SerializeField] private float endY = 900f;

    [SerializeField] private float enterDuration = 0.35f;
    [SerializeField] private float holdDuration = 0.2f;
    [SerializeField] private float exitDuration = 0.35f;

    private Coroutine playRoutine;

    private void Start()
    {
        hand.gameObject.SetActive(false);
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
        hand.gameObject.SetActive(true);

        SetY(startY);

        yield return MoveY(
            startY,
            pushY,
            enterDuration
        );

        yield return new WaitForSeconds(
            holdDuration
        );

        yield return MoveY(
            pushY,
            endY,
            exitDuration
        );

        hand.gameObject.SetActive(false);

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
            hand.anchoredPosition;

        position.y = y;

        hand.anchoredPosition =
            position;
    }
}