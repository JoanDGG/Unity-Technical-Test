using System.Collections;
using UnityEngine;

public class WebhookEventManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ClimbingPlayer climbingPlayer;
    [SerializeField] private WebhookPresentationController presentationController;

    public void TriggerEvent(
    WebhookEventConfig config)
    {
        if (config == null)
            return;

        presentationController.PlayPresentation(
            config
        );

        StartCoroutine(
            ApplyForceDelayed(config)
        );
    }

    private IEnumerator ApplyForceDelayed(
    WebhookEventConfig config)
    {
        if (config.ForceDelay > 0f)
        {
            yield return new WaitForSeconds(
                config.ForceDelay
            );
        }

        bool accepted =
            climbingPlayer.ReceiveWebhookForce(
                config.HeightDelta,
                config.MovementDuration
            );

        if (accepted)
        {
            presentationController
                .PlayImpactFeedback();
        }
    }
}