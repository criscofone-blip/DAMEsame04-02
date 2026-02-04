using UnityEngine;

public class CursorAutoClick : MonoBehaviour
{
    [Header("Orbit")]
    public Transform target;
    public float radius = 2.9f;
    public float orbitSpeed = 90f;
    public float startAngle = 0;

    [Header("Flash Color")]
    public Color flashColor = Color.red;
    public float flashTime = 0.08f;

    SpriteRenderer spriteRenderer;
    Color originalColor;
    float flashTimer = 0;

    public void Initialize(Transform cookieTarget, float angle)
    {
        target = cookieTarget;
        startAngle = angle;
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (target == null)
            return;

        float angle = startAngle + (Time.time * orbitSpeed);
        float rad = angle * Mathf.Deg2Rad;

        Vector3 pos = target.position;
        pos.x += Mathf.Cos(rad) * radius;
        pos.y += Mathf.Sin(rad) * radius;

        transform.position = pos;

        Vector3 dir = (target.position - transform.position).normalized;
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rotZ -= 90f;

        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0)
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = originalColor;
            }
        }
    }

    public void FlashRed()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = flashColor;
        flashTimer = flashTime;
    }
}


