using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class GlovesWebhookPresentation : MonoBehaviour
{
    [Header("Gloves")]
    [SerializeField] private RectTransform glovePrefab;
    [SerializeField] private RectTransform gloveContainer;
    [SerializeField] private int minGloves = 4;
    [SerializeField] private int maxGloves = 6;

    [Header("Glove Animation")]
    [SerializeField] private Vector2 impactPositionOffset = Vector2.zero;
    [SerializeField] private float gloveRotationOffset = 0f;
    [SerializeField] private float gloveTravelDistance = 1400f;
    [SerializeField] private float gloveAttackDuration = 0.12f;
    [SerializeField] private float gloveHoldDuration = 0.08f;
    [SerializeField] private float gloveExitDuration = 0.18f;
    [SerializeField] private float gloveDelay = 0.05f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip punchClip;
    [SerializeField] private float punchVolume = 0.5f;
    [SerializeField] private float punchDelay = 0.1f;

    public void Play()
    {
        StartCoroutine(PlayBumpSequence());
    }

    private IEnumerator PlayBumpSequence()
    {
        int gloveCount = Random.Range(minGloves, maxGloves + 1);

        // Immediate impact feedback.
        PlaySound(gloveCount);

        for (int i = 0; i < gloveCount; i++)
        {
            SpawnGlove();

            yield return new WaitForSeconds(gloveDelay);
        }
    }

    private void SpawnGlove()
    {
        if (glovePrefab == null || gloveContainer == null)
            return;

        RectTransform glove =
            Instantiate(glovePrefab, gloveContainer);

        StartCoroutine(AnimateGlove(glove));
    }

    private IEnumerator AnimateGlove(RectTransform glove)
    {
        // Randomize where this glove attacks around the center.
        Vector2 impact =
            impactPositionOffset +
            new Vector2(
                Random.Range(-Screen.width * 0.35f, Screen.width * 0.35f),
                Random.Range(-Screen.height * 0.15f, Screen.height * 0.15f)
            );

        // Spawn above the screen, roughly aligned with its impact point.
        Vector2 start =
            new Vector2(
                impact.x + Random.Range(-gloveTravelDistance, gloveTravelDistance),
                Screen.height * 0.75f
            );

        // Continue slightly past the impact to sell the downward force.
        Vector2 exit =
            impact + new Vector2(
                Random.Range(-50f, 50f),
                -gloveTravelDistance
            );

        glove.anchoredPosition = start;

        // Point the glove in the direction it's traveling.
        SetGloveDirection(glove, start, impact);

        yield return MoveUI(
            glove,
            start,
            impact,
            gloveAttackDuration
        );

        yield return new WaitForSeconds(gloveHoldDuration);

        SetGloveDirection(glove, impact, exit);

        yield return MoveUI(
            glove,
            impact,
            exit,
            gloveExitDuration
        );

        Destroy(glove.gameObject);
    }

    private void SetGloveDirection(
        RectTransform glove,
        Vector2 from,
        Vector2 to
    )
    {
        Vector2 direction = to - from;

        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        glove.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle + gloveRotationOffset
            );
    }

    private IEnumerator MoveUI(
        RectTransform target,
        Vector2 start,
        Vector2 end,
        float duration
    )
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(timer / duration);

            // Fast punch-in with a slight ease-out.
            t = 1f - Mathf.Pow(1f - t, 3f);

            target.anchoredPosition =
                Vector2.LerpUnclamped(start, end, t);

            yield return null;
        }

        target.anchoredPosition = end;
    }

    public void PlaySound(int gloveCount)
    {
        StartCoroutine(PlaySoundRoutine(gloveCount));
    }

    private IEnumerator PlaySoundRoutine(int gloveCount)
    {
        for (int i = 0; i < gloveCount; i++)
        {
            if (audioSource != null && punchClip != null)
            {
                audioSource.PlayOneShot(punchClip, punchVolume);
            }

            yield return new WaitForSeconds(punchDelay);
        }
    }
}
