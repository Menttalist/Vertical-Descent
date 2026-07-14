using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeightForce = 10f;
    [SerializeField] private float jumpSideForce = 8f;

    [Header("Эффекты разрушения (Для Бура)")]
    [SerializeField] private GameObject normalDestroyPrefab; // Сюда закинь серый префаб осколков
    [SerializeField] private GameObject drillProcessPrefab;  // Сюда закинь искры процесса бурения
    [SerializeField] private GameObject redDestroyPrefab;    // Сюда закинь красный префаб осколков

    [Header("Настройки бурения")]
    [SerializeField] private int maxDrills = 3;

    // СДЕЛАЛ ПУБЛИЧНЫМ! Теперь твой старый скрипт UI снова видит заряды напрямую!
    [HideInInspector] public int currentDrills;

    [SerializeField] private float drillCheckDistance = 1.6f;
    [SerializeField] private float drillDuration = 0.5f; // Время самого бурения
    private bool isDrilling = false;

    [Header("Проверка приземления")]
    [SerializeField] private float groundCheckDistance = 0.8f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ограничения шахты")]
    [SerializeField] private float minX = -2.5f;
    [SerializeField] private float maxX = 2.5f;

    private Rigidbody2D rb;
    private float horizontalInput = 0f;
    private bool isJumping = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentDrills = maxDrills;
    }

    void Update()
    {
        if (isDrilling) return;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        if (isJumping && rb.linearVelocity.y < 0.1f)
        {
            isJumping = false;
        }
    }

    void FixedUpdate()
    {
        if (isJumping || isDrilling) return;
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public void StartMoveLeft() { if (!isDrilling) horizontalInput = -1f; }
    public void StartMoveRight() { if (!isDrilling) horizontalInput = 1f; }
    public void StopMove()
    {
        horizontalInput = 0f;
        if (rb != null && !isDrilling)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, hit.collider != null ? Color.green : Color.red);
        return hit.collider != null;
    }

    public void JumpLeft()
    {
        if (isDrilling || !IsGrounded()) return;
        isJumping = true;
        rb.linearVelocity = new Vector2(-jumpSideForce, jumpHeightForce);
    }

    public void JumpRight()
    {
        if (isDrilling || !IsGrounded()) return;
        isJumping = true;
        rb.linearVelocity = new Vector2(jumpSideForce, jumpHeightForce);
    }

    // --- БУРЕНИЕ С ТАЙМЕРОМ И ОЧИСТКОЙ ЧАСТИЦ ---
    public void UseDrill()
    {
        if (currentDrills <= 0 || isDrilling) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, drillCheckDistance, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * drillCheckDistance, Color.cyan, 1f);

        if (hit.collider != null && hit.collider.CompareTag("NormalBlock"))
        {
            currentDrills--; // Списываем заряд мгновенно
            StartCoroutine(DrillingRoutine(hit.collider.gameObject));
        }
    }

    private IEnumerator DrillingRoutine(GameObject blockToDestroy)
    {
        isDrilling = true;
        horizontalInput = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        GameObject activeSparks = null;
        ParticleSystem ps = null;

        // Создаем непрерывный эффект бурения
        if (drillProcessPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0f, -0.6f, -1f);
            activeSparks = Instantiate(drillProcessPrefab, spawnPos, Quaternion.identity);
            activeSparks.transform.SetParent(this.transform);

            ps = activeSparks.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        // Ждем время бурения блока
        yield return new WaitForSeconds(drillDuration);

        // --- МГНОВЕННОЕ ТУШЕНИЕ ИСКР БУРА ---
        if (activeSparks != null)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Гасим новые искры и принудительно стираем старые с экрана
            }
            Destroy(activeSparks);
        }

        if (blockToDestroy != null)
        {
            Vector3 targetPosition = blockToDestroy.transform.position;
            Destroy(blockToDestroy);

            rb.bodyType = RigidbodyType2D.Dynamic;
            isJumping = false;
            rb.linearVelocity = new Vector2(0f, -5f);

            // Финальный взрыв осколков на месте куба
            if (normalDestroyPrefab != null)
            {
                Vector3 particlePos = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z - 1f);
                GameObject explosion = Instantiate(normalDestroyPrefab, particlePos, Quaternion.identity);
                Destroy(explosion, 1.5f);
            }
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        isDrilling = false;
    }
}