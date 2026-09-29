using UnityEngine;

public class Handhold : MonoBehaviour
{
    public float Height { get; private set; }
    public float Angle { get; private set; }
    public Vector3 OutwardDirection { get; private set; }

    [SerializeField] private float grabOffset = 0.15f;

    public Vector3 GrabPosition =>
        transform.position + OutwardDirection * grabOffset;

    public Quaternion GrabRotation =>
        Quaternion.LookRotation(-OutwardDirection, Vector3.up);

    public void Initialize(
        float height,
        float angle,
        Vector3 outwardDirection)
    {
        Height = height;
        Angle = angle;
        OutwardDirection = outwardDirection.normalized;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (OutwardDirection.sqrMagnitude < 0.001f)
            return;

        Gizmos.DrawSphere(GrabPosition, 0.06f);
    }
#endif
}