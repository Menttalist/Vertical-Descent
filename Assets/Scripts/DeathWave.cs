using UnityEngine;

public class DeathWave : MonoBehaviour
{
    [Header("Настройки адаптивной скорости (Rubber-banding)")]
    [SerializeField] private Transform playerTransform; // Ссылка на трансформ робота (Player)
    [SerializeField] private float minSpeed = 2.0f;      // Минимальная скорость, когда волна нависла прямо над головой
    [SerializeField] private float targetSpeed = 3.5f;   // Комфортная средняя скорость погони
    [SerializeField] private float maxSpeed = 6.5f;      // Максимальная скорость, если игрок улетел слишком далеко вперед

    [Header("Дистанции адаптации")]
    [SerializeField] private float safeDistance = 3.5f;    // Если игрок ближе этой дистанции по Y, волна замедляется
    [SerializeField] private float runAwayDistance = 7.5f; // Если игрок дальше этой дистанции по Y, волна ускоряется на максимум

    [Header("Эффекты разрушения блоков")]
    [SerializeField] private GameObject normalDestroyPrefab; // Серый префаб для обычных блоков
    [SerializeField] private GameObject redDestroyPrefab;    // Красный префаб для красных блоков

    private float currentSpeed;

    void Start()
    {
        currentSpeed = targetSpeed;

        // Если забыли перетащить игрока в инспекторе, скрипт найдет его сам
        if (playerTransform == null)
        {
            RobotController player = FindFirstObjectByType<RobotController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Считаем расстояние по вертикали (Y) между волной и игроком
        float distanceToPlayer = Mathf.Abs(transform.position.y - playerTransform.position.y);

        // --- УМНАЯ СИСТЕМА ДИНАМИЧЕСКОЙ СКОРОСТИ ---
        if (distanceToPlayer < safeDistance)
        {
            // Игрок слишком близко! Даем ему глоток воздуха — плавно замедляем волну до minSpeed
            currentSpeed = Mathf.Lerp(currentSpeed, minSpeed, Time.deltaTime * 2.0f);
        }
        else if (distanceToPlayer > runAwayDistance)
        {
            // Игрок убежал слишком далеко вперед, ему скучно! Разгоняем волну до maxSpeed, чтобы нагнать его
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * 2.5f);
        }
        else
        {
            // Игрок держит идеальную дистанцию — волна идет с базовой скоростью targetSpeed
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 1.5f);
        }

        // Двигаем волну вниз с вычисленной адаптивной скоростью
        transform.Translate(Vector2.down * currentSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Если волна настигает Игрока
        RobotController robot = other.GetComponent<RobotController>();
        if (robot != null)
        {
            Debug.Log("Волна смерти поглотила робота! Запускаем плавную смерть...");

            // Запускаем красивый метод смерти у робота (анимация + задержка GameOver в 1.2 сек)
            robot.Die();
            return;
        }

        // 2. Если волна налетает на ОБЫЧНЫЙ БЛОК
        if (other.CompareTag("NormalBlock"))
        {
            SpawnEffect(normalDestroyPrefab, other.transform.position);
            Destroy(other.gameObject);
        }
        // 3. Если волна налетает на КРАСНЫЙ БЛОК
        else if (other.CompareTag("RedBlock"))
        {
            SpawnEffect(redDestroyPrefab, other.transform.position);
            Destroy(other.gameObject);
        }
    }

    // Спавн частиц с авто-удалением из памяти, чтобы не лагал телефон
    private void SpawnEffect(GameObject prefab, Vector3 position)
    {
        if (prefab != null)
        {
            GameObject effect = Instantiate(prefab, position, Quaternion.identity);
            Destroy(effect, 1.5f);
        }
    }
}