using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrapplingHook : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxGrappleDistance = 10f;
    public float swingForce = 10f;
    public LayerMask grappleMask;

    private Rigidbody2D rb;
    private DistanceJoint2D joint;
    private Camera cam;
    private Vector2 grapplePoint;
    private bool isGrappling;

    private PlayerController playerController;

    private enum SwingSide { Left, Right }
    private SwingSide? lastReleaseSide = null;
    private SwingSide currentSide;
    private float swingPower = 0f;
    private SwingSide? lastBoostedZone = null;
    private bool holdingLeft = false;
    private bool holdingRight = false;
    private SwingSide? releasedSide = null;
    public ControllerWithToggle controller;
    public bool isActive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        playerController = GetComponent<PlayerController>();

        joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.enabled = false;
        joint.autoConfigureConnectedAnchor = false;
        joint.enableCollision = true;

        if (!lineRenderer)
        {
            GameObject lrObj = new GameObject("GrappleLine");
            lrObj.transform.parent = transform;
            lineRenderer = lrObj.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
        }

        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (isActive)
        {
            if (Input.GetMouseButtonDown(0))
                TryStartGrapple();

            if (Input.GetMouseButtonUp(0))
                StopGrapple();

            if (isGrappling)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, grapplePoint);
                ApplySwingForce();
            }
        }
    }

    void TryStartGrapple()
    {
        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mouseWorldPos - (Vector2)transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, maxGrappleDistance, grappleMask);

        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            joint.connectedAnchor = grapplePoint;
            joint.distance = Vector2.Distance(transform.position, grapplePoint);
            joint.enabled = true;

            isGrappling = true;
            lineRenderer.enabled = true;

            if (playerController != null)
                playerController.isSwinging = true;
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        joint.enabled = false;
        lineRenderer.enabled = false;

        if (playerController != null)
            playerController.isSwinging = false;
    }

    void ApplySwingForce()
    {
        Vector2 dirToAnchor = grapplePoint - rb.position;
        Vector2 tangent = Vector2.Perpendicular(dirToAnchor).normalized;
        Vector2 velocity = rb.linearVelocity;

        float angle = Vector2.SignedAngle(dirToAnchor, velocity);

        bool inLeftBoostZone = angle > 30f && angle < 150f;
        bool inRightBoostZone = angle < -30f && angle > -150f;

        // Обновляем состояние зажатия кнопок
        holdingLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        holdingRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        // Если кнопка отжата — запоминаем, с какой стороны отпустили
        if (!holdingLeft && releasedSide == SwingSide.Left)
        {
            // Уже отпустили, продолжаем уменьшать
        }
        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            releasedSide = SwingSide.Left;
        }

        if (!holdingRight && releasedSide == SwingSide.Right)
        {
            // Уже отпустили, продолжаем уменьшать
        }
        else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            releasedSide = SwingSide.Right;
        }

        // Если держим левую кнопку и в левой зоне — добавляем силу
        if (holdingLeft && inLeftBoostZone)
        {
            swingPower -= 1.5f; // минус потому что левая сторона — это отрицательная сила
            releasedSide = null; // сбрасываем отпускание
        }
        // Если держим правую кнопку и в правой зоне — добавляем силу
        else if (holdingRight && inRightBoostZone)
        {
            swingPower += 1.5f; // плюс — правая сторона положительная сила
            releasedSide = null; // сбрасываем отпускание
        }
        else
        {
            // Если кнопку не держим, то постепенно уменьшаем силу
            if (releasedSide == SwingSide.Left)
            {
                // Уменьшаем силу, если она отрицательная
                if (swingPower < 0)
                    swingPower = Mathf.MoveTowards(swingPower, 0f, 10f * Time.deltaTime);
                else
                    releasedSide = null; // если сила дошла до 0 — сбрасываем
            }
            else if (releasedSide == SwingSide.Right)
            {
                // Уменьшаем силу, если она положительная
                if (swingPower > 0)
                    swingPower = Mathf.MoveTowards(swingPower, 0f, 10f * Time.deltaTime);
                else
                    releasedSide = null; // если сила дошла до 0 — сбрасываем
            }
        }

        // Ограничиваем силу по максимальному значению
        swingPower = Mathf.Clamp(swingPower, -10f, 10f);

        // Применяем силу по тангенте
        rb.AddForce(tangent * swingPower * swingForce * Time.deltaTime);
    }




}
