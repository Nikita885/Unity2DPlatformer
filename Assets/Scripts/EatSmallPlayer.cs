using UnityEngine;

public class EatSmallPlayer : MonoBehaviour
{
    public GameObject smallPlayer;
    public PlayerController smallPlayerController;
    public GrapplingHook smallPlayerGrapplingHook;

    public float eatDistance = 1.0f;
    public float shootForce = 10f;  // начальная скорость
    public float gravity = 9.8f;    // ускорение свободного падения

    private bool isHoldingSmall = false;
    private bool isFlying = false;

    private Vector3 startPos;
    private Vector3 shootDirection;
    private float shootAngleRad;
    private float flightTime = 0f;

    void Update()
    {
        if (smallPlayer == null)
            return;

        if (!isHoldingSmall && !isFlying)
        {
            float distance = Vector2.Distance(transform.position, smallPlayer.transform.position);
            if (distance < eatDistance && Input.GetMouseButtonDown(0))
            {
                GrabSmall();
            }
        }
        else if (isHoldingSmall)
        {
            if (Input.GetMouseButton(0))
            {
                // держим маленького в центре большого
                smallPlayer.transform.position = transform.position;
            }
            else
            {
                ReleaseSmallAndShoot();
            }
        }
        else if (isFlying)
        {
            // Обновляем позицию маленького по параболе
            flightTime += Time.deltaTime;

            // Положение по X
            float x = shootDirection.x * shootForce * flightTime;

            // Положение по Y (формула движения с ускорением гравитации)
            float y = shootDirection.y * shootForce * flightTime - 0.5f * gravity * flightTime * flightTime;

            Vector3 newPos = startPos + new Vector3(x, y, 0);
            smallPlayer.transform.position = newPos;

            // Пример простого условия остановки — если маленький упал ниже начального уровня
            if (newPos.y <= startPos.y)
            {
                isFlying = false;
                // Можно включить скрипты снова
                if (smallPlayerController != null)
                    smallPlayerController.enabled = true;
                if (smallPlayerGrapplingHook != null)
                    smallPlayerGrapplingHook.enabled = true;

                // Можно "приземлить" маленького на землю (выровнять по Y)
                Vector3 landedPos = new Vector3(newPos.x, startPos.y, newPos.z);
                smallPlayer.transform.position = landedPos;
            }
        }
    }

    void GrabSmall()
    {
        isHoldingSmall = true;
        if (smallPlayerController != null)
            smallPlayerController.enabled = false;
        if (smallPlayerGrapplingHook != null)
            smallPlayerGrapplingHook.enabled = false;
    }

    void ReleaseSmallAndShoot()
    {
        isHoldingSmall = false;
        isFlying = true;
        flightTime = 0f;

        startPos = smallPlayer.transform.position;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Вычисляем направление (нормализуем)
        Vector3 dir = mouseWorldPos - startPos;
        dir.z = 0f;

        // Задаём угол вылета (например 45 градусов)
        float angleDegrees = 45f;
        shootAngleRad = angleDegrees * Mathf.Deg2Rad;

        // Вычисляем компоненты направления по X и Y с углом
        float dirX = dir.normalized.x;

        shootDirection = new Vector3(dirX * Mathf.Cos(shootAngleRad), Mathf.Sin(shootAngleRad), 0);
    }
}
