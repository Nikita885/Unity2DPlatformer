using UnityEngine;

public class LandingCameraShake : MonoBehaviour
{
    public Camera mainCamera;          // ������ �� ������
    public float shakeDuration = 0.3f; // ������������ ������
    public float shakeMagnitude = 0.1f; // ��������� ������

    private bool isGrounded = false;    // ���� �����������
    private Rigidbody2D rb;

    private Vector3 originalCamPos;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        rb = GetComponent<Rigidbody2D>();
        originalCamPos = mainCamera.transform.localPosition;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ��������� ������������ � "�����" �� ���� ��� ����
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isGrounded && rb.linearVelocity.y <= 0f)
            {
                isGrounded = true;
                StartCoroutine(ShakeCamera());
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    System.Collections.IEnumerator ShakeCamera()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            mainCamera.transform.localPosition = new Vector3(originalCamPos.x + x, originalCamPos.y + y, originalCamPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalCamPos;
    }
}
