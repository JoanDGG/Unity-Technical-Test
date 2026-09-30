using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class WebhookPresentationController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Flash")]
    [SerializeField] private Image flashImage;

    [Header("Camera Shake")]
    [SerializeField] private CinemachineImpulseSource cameraShake;

    [Header("Presentations")]
    [SerializeField] private GlovesWebhookPresentation glovesPresentation;
    [SerializeField] private CarWebhookPresentation carPresentation;
    [SerializeField] private RocketWebhookPresentation rocketPresentation;
    [SerializeField] private HandWebhookPresentation handPresentation;

    private WebhookEventConfig config;

    private void Awake()
    {
        if (flashImage != null)
            SetFlashAlpha(0f);
    }

    public void PlayPresentation(
        WebhookEventConfig config)
    {
        if (config == null)
            return;

        this.config = config;

        PlaySound(config);

        Debug.Log($"Playing presentation: {config.PresentationType}");

        switch (config.PresentationType)
        {
            case WebhookPresentationType.Gloves:
                glovesPresentation.Play();
                break;

            case WebhookPresentationType.Car:
                carPresentation.Play();
                break;

            case WebhookPresentationType.Rockets:
                rocketPresentation.Play();
                break;

            case WebhookPresentationType.Hand:
                handPresentation.Play();
                break;
        }
    }

    public void PlayImpactFeedback()
    {
        PlayFlash();
        PlayCameraShake();
    }

    private void PlayFlash()
    {
        if (flashImage != null)
            StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetFlashAlpha(0.75f);

        float timer = 0f;

        while (timer < config.FlashDuration)
        {
            timer += Time.deltaTime;

            float alpha =
                Mathf.Lerp(
                    0.75f,
                    0f,
                    timer / config.FlashDuration
                );

            SetFlashAlpha(alpha);

            yield return null;
        }

        SetFlashAlpha(0f);
    }

    private void SetFlashAlpha(float alpha)
    {
        Color color = flashImage.color;
        color.a = alpha;
        flashImage.color = color;
    }

    private void PlayCameraShake()
    {
        if (cameraShake == null)
            return;

        cameraShake.GenerateImpulse(config.ShakeStrength);
    }

    private void PlaySound(
        WebhookEventConfig config)
    {
        if (audioSource == null ||
            config.SoundEffect == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            config.SoundEffect
        );
    }
}