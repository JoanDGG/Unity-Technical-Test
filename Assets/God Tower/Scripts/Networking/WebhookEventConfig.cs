using UnityEngine;

[CreateAssetMenu(
    fileName = "WebhookEvent",
    menuName = "God Tower/Webhook Event"
)]
public class WebhookEventConfig : ScriptableObject
{
    [Header("Event")]
    [SerializeField] private string eventName;
    [SerializeField] private string route;

    [Header("Gameplay")]
    [SerializeField] private float heightDelta = -2.25f;
    [SerializeField] private float movementDuration = 0.65f;

    [Header("Presentation")]
    [SerializeField] private WebhookPresentationType presentationType;
    [SerializeField] private AudioClip soundEffect;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float shakeStrength = -0.5f;

    [Header("Timing")]
    [SerializeField] private float forceDelay = 0f;

    public float ForceDelay => forceDelay;
    public string EventName => eventName;
    public string Route => route;

    public float HeightDelta => heightDelta;
    public float MovementDuration => movementDuration;

    public float FlashDuration => flashDuration;
    public float ShakeStrength => shakeStrength;

    public WebhookPresentationType PresentationType =>
        presentationType;

    public AudioClip SoundEffect =>
        soundEffect;
}

public enum WebhookPresentationType
{
    Gloves,
    Car,
    Rockets,
    Hand
}