using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class KartController : MonoBehaviour
{
    public enum CarForwardDirection // ajeita a direção interpretada pelo carro (modelo veio com as direções erradas)
    {
        BlueArrow_Forward,
        BlueArrow_Backward,
        RedArrow_Right,
        RedArrow_Left
    }

    [Header("Model Alignment")]
    public CarForwardDirection modelForward = CarForwardDirection.RedArrow_Right;

    [Header("Speed & Jump Settings")] // maioria é auto explicativo sobre sua função, que nem um carro
    public float maxForwardSpeed = 22f;
    public float maxReverseSpeed = 8f;
    public float acceleration = 28f;
    public float brakeAndDecel = 35f;

    [Header("Turning & Slope Tilt")]
    public float turnSpeed = 140f;
    public float slopeTiltSpeed = 10f;    // grau de velocidade do carro inclinar em descidas e tal
    public float groundRayLength = 1.5f; // isso daqui é um raycast que detecta o chão pro carro saber fazer a curva direito

    [Header("Gravity")] // antes o carro não caía pq não tinha gravidade, esse valor lida com como ele deeve se comportar no chão (grounded) e no ar (airborne)
    public float groundedGravity = 10f;
    public float airborneGravity = 20f;

    private Rigidbody rb;
    private BoxCollider boxCol;
    private float currentSpeed = 0f;
    private float moveInput;
    private float steerInput;
    private float currentYaw;
    private Vector3 currentSurfaceNormal = Vector3.up;
    private bool isGrounded;

    private Vector3 GetLocalForwardAxis() // essas duas funções seguidas são só para saber a direção certa do carro
    {
        switch (modelForward)
        {
            case CarForwardDirection.BlueArrow_Backward: return Vector3.back;
            case CarForwardDirection.RedArrow_Right: return Vector3.right;
            case CarForwardDirection.RedArrow_Left: return Vector3.left;
            default: return Vector3.forward;
        }
    }

    private Vector3 GetCarForward()
    {
        return transform.TransformDirection(GetLocalForwardAxis());
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>();


        // como a gravidade nativa do Unity tava quebrando foi colocado tunagem própria basicamente. valeu unity
        rb.isKinematic = false;
        rb.useGravity = false; 
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // mesma coisa, tunagem própria da movimentação do rigidbody pq n tava funcionando direito
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        rb.automaticCenterOfMass = false;
        rb.centerOfMass = boxCol.center;

        currentYaw = transform.eulerAngles.y;

        PhysicsMaterial mat = new PhysicsMaterial("ZeroFriction");
        mat.dynamicFriction = 0f;
        mat.staticFriction = 0f;
        mat.frictionCombine = PhysicsMaterialCombine.Minimum;
        boxCol.material = mat;
    }

    void Update()
    { // movimentação
        moveInput = 0f;
        steerInput = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveInput = 1f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveInput = -1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) steerInput = 1f;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) steerInput = -1f;
    }

    void FixedUpdate()
    { // basicamente feito pra sempre saber onde tá o centro de massa do carro pois fisica, espaço e etc
        rb.centerOfMass = boxCol.center;

        DetectGroundSlope();
        HandleSpeed();
        HandleRotationAndSlopeTilt();
        HandleMovementAndJump();
    }

    private void DetectGroundSlope() // função que detecta se é inclinado pra saber como se comportar
    {
        Vector3 rayStart = transform.TransformPoint(boxCol.center);
        isGrounded = false;
        Vector3 targetNormal = Vector3.up;

        
        RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, groundRayLength, ~0, QueryTriggerInteraction.Ignore);
        float closestDist = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {

            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;

            if (hit.distance < closestDist)
            {
                closestDist = hit.distance;
                targetNormal = hit.normal;
                isGrounded = true;
            }
        }

        currentSurfaceNormal = Vector3.Slerp(currentSurfaceNormal, targetNormal, slopeTiltSpeed * Time.fixedDeltaTime);
    }

    private void HandleSpeed()
    {
        float targetSpeed = 0f;
        if (moveInput > 0f) targetSpeed = maxForwardSpeed;
        else if (moveInput < 0f) targetSpeed = -maxReverseSpeed;

        float rate = (Mathf.Abs(targetSpeed) > 0.1f && Mathf.Sign(targetSpeed) == Mathf.Sign(currentSpeed) || Mathf.Abs(currentSpeed) < 0.5f)
            ? acceleration
            : brakeAndDecel;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
    }

    private void HandleRotationAndSlopeTilt()
    {
        if (Mathf.Abs(currentSpeed) > 0.2f && Mathf.Abs(steerInput) > 0.01f)
        {
            float reverseDir = currentSpeed < 0f ? -1f : 1f;
            currentYaw += steerInput * turnSpeed * reverseDir * Time.fixedDeltaTime;
        }

        // rotação usando euler com valor de rampa pra saber o que fazer
        Quaternion yawRotation = Quaternion.Euler(0f, currentYaw, 0f);
        Quaternion slopeTilt = Quaternion.FromToRotation(Vector3.up, currentSurfaceNormal);

        rb.MoveRotation(slopeTilt * yawRotation);
    }

    private void HandleMovementAndJump()
    {
        Vector3 carForward = GetCarForward();

        if (isGrounded)
        {
            Vector3 slopeForward = Vector3.ProjectOnPlane(carForward, currentSurfaceNormal).normalized;
            Vector3 driveVel = slopeForward * currentSpeed;

            rb.linearVelocity = driveVel + (Vector3.down * groundedGravity * Time.fixedDeltaTime);
        }
        else
        {
            Vector3 flatForward = new Vector3(carForward.x, 0f, carForward.z).normalized;
            Vector3 horizVel = flatForward * currentSpeed;

            float vertVel = rb.linearVelocity.y - (airborneGravity * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(horizVel.x, vertVel, horizVel.z);
        }
    }

    void OnDrawGizmos()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        Vector3 pivotWorld = col != null ? transform.TransformPoint(col.center) : transform.position;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(pivotWorld, pivotWorld + Vector3.down * groundRayLength);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pivotWorld, pivotWorld + GetCarForward() * 3f);
    }
}