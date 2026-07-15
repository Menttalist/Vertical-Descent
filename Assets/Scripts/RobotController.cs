using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeightForce = 10f;
    [SerializeField] private float jumpSideForce = 8f;

    [Header("Эффекты разрушения (Для Бура)")]
    [SerializeField] private GameObject normalDestroyPrefab;
    [SerializeField] private GameObject drillProcessPrefab;
    [SerializeField] private GameObject redDestroyPrefab;

    [Header("Настройки бурения")]
    [SerializeField] private int maxDrills = 3;

    [HideInInspector] public int currentDrills;
    [HideInInspector] public int coinsCollected = 0;

    [SerializeField] private float drillCheckDistance = 1.6f;
    [SerializeField] private float drillDuration = 0.5f;
    private bool isDrilling = false;

    // СОСТОЯНИЕ СМЕРТИ
    private bool isDead = false;

    [Header("Проверка приземления")]
    [SerializeField] private float groundCheckDistance = 0.8f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ограничения шахты")]
    [SerializeField] private float minX = -2.5f;
    [SerializeField] private float maxX = 2.5f;

    [Header("Анимации и Модель")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform modelTransform;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float modelYOffset = -0.5f;

    private Rigidbody2D rb;
    private float horizontalInput = 0f;
    private bool isJumping = false;
    private float lastTargetYAngle = 90f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentDrills = maxDrills;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (modelTransform == null && transform.childCount > 0)
        {
            modelTransform = transform.GetChild(0);
        }
    }

    void Update()
    {
        // Если робот мертв — полностью отключаем апдейт логики и движений
        if (isDead) return;

        if (animator != null)
        {
            bool grounded = IsGrounded();
            animator.SetBool("IsGrounded", grounded);

            if (rb != null)
            {
                animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
            }

            float visualSpeed = isDrilling ? 0f : Mathf.Abs(horizontalInput);
            animator.SetFloat("Speed", visualSpeed);
        }

        RotateModel();

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
        if (isJumping || isDrilling || isDead) return;
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void RotateModel()
    {
        if (modelTransform == null || isDead) return;

        modelTransform.localPosition = new Vector3(0f, modelYOffset, modelTransform.localPosition.z);

        if (!isDrilling)
        {
            if (horizontalInput > 0.1f)
            {
                lastTargetYAngle = 90f;
            }
            else if (horizontalInput < -0.1f)
            {
                lastTargetYAngle = 270f;
            }
        }

        Quaternion targetRotation = Quaternion.Euler(0f, lastTargetYAngle, 0f);
        modelTransform.localRotation = Quaternion.Slerp(modelTransform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    public void StartMoveLeft() { if (!isDrilling && !isDead) horizontalInput = -1f; }
    public void StartMoveRight() { if (!isDrilling && !isDead) horizontalInput = 1f; }
    public void StopMove()
    {
        horizontalInput = 0f;
        if (rb != null && !isDrilling && !isDead)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        float circleRadius = 0.3f;
        float castDistance = groundCheckDistance - circleRadius;
        if (castDistance < 0.05f) castDistance = 0.05f;

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, circleRadius, Vector2.down, castDistance, groundLayer);
        return hit.collider != null;
    }

    public void JumpLeft()
    {
        if (isDrilling || !IsGrounded() || isDead) return;
        isJumping = true;
        rb.linearVelocity = new Vector2(-jumpSideForce, jumpHeightForce);
    }

    public void JumpRight()
    {
        if (isDrilling || !IsGrounded() || isDead) return;
        isJumping = true;
        rb.linearVelocity = new Vector2(jumpSideForce, jumpHeightForce);
    }

    public void UseDrill()
    {
        if (currentDrills <= 0 || isDrilling || isDead) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, drillCheckDistance, groundLayer);
        if (hit.collider != null && hit.collider.CompareTag("NormalBlock"))
        {
            currentDrills--;
            StartCoroutine(DrillingRoutine(hit.collider.gameObject));
        }
    }

    private IEnumerator DrillingRoutine(GameObject blockToDestroy)
    {
        isDrilling = true;
        horizontalInput = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (animator != null)
        {
            animator.SetTrigger("DrillTrigger");
        }

        GameObject activeSparks = null;
        ParticleSystem ps = null;

        if (drillProcessPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0f, -0.6f, -1f);
            activeSparks = Instantiate(drillProcessPrefab, spawnPos, Quaternion.identity);
            activeSparks.transform.SetParent(this.transform);

            ps = activeSparks.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        yield return new WaitForSeconds(drillDuration);

        if (activeSparks != null)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

    // --- ЛОГИКА СМЕРТИ ---
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        horizontalInput = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static; // Фиксируем робота на месте, чтобы он не проваливался физически

        // Включаем анимацию смерти
        if (animator != null)
        {
            animator.SetTrigger("DieTrigger");
        }

        // Запускаем отложенный вызов панели GameOver в GameManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.StartCoroutine(gameManager.DelayedGameOver(1.2f)); // 1.2 секунды задержки, чтобы досмотрели анимацию
        }
    }

    // Автоматически умираем, если наступили на красный блок
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("RedBlock"))
        {
            Die();
        }
    }
}