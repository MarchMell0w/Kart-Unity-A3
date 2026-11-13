using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CenterOfMassHelper : MonoBehaviour
{
    [Header("Center of Mass Offset (Local Space)")]
    [Tooltip("Adjust X (Left/Right), Y (Up/Down), and Z (Forward/Back)")]
    public Vector3 centerOfMassOffset = new Vector3(0f, -0.3f, 0f);

    [Header("Gizmo Display Settings")]
    public float gizmoRadius = 0.15f;
    public Color gizmoColor = Color.red;

    private Rigidbody rb;

    void Awake()
    {
        ApplyCenterOfMass();
    }

    void OnValidate()
    {
        // Updates the Rigidbody center of mass in real-time during Play Mode if adjusted in Inspector
        if (Application.isPlaying)
        {
            ApplyCenterOfMass();
        }
    }

    public void ApplyCenterOfMass()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.centerOfMass = centerOfMassOffset;
    }

    void OnDrawGizmosSelected()
    {
        // Calculate world-space position of the center of mass
        Vector3 worldCoM = transform.TransformPoint(centerOfMassOffset);

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(worldCoM, gizmoRadius);
        Gizmos.DrawWireSphere(worldCoM, gizmoRadius * 1.2f);

        // Draw an axis crosshair through the center of mass
        Gizmos.color = Color.blue; // Forward
        Gizmos.DrawRay(worldCoM, transform.forward * (gizmoRadius * 2f));

        Gizmos.color = Color.red; // Right
        Gizmos.DrawRay(worldCoM, transform.right * (gizmoRadius * 2f));

        Gizmos.color = Color.green; // Up
        Gizmos.DrawRay(worldCoM, transform.up * (gizmoRadius * 2f));
    }
}