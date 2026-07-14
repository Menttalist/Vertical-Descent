using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathWave : MonoBehaviour
{
    [Header("Настройки скорости")]
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float speedIncrease = 0.15f;
    [SerializeField] private float maxSpeed = 10f;

    [Header("Эффекты разрушения блоков")]
    [SerializeField] private GameObject normalDestroyPrefab; // Сюда закинь серый префаб для NormalBlock
    [SerializeField] private GameObject redDestroyPrefab;    // Сюда закинь свой новый красный префаб для RedBlock

    void Update()
    {
        if (speed < maxSpeed)
        {
            speed += speedIncrease * Time.deltaTime;
        }

        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Если волна налетает на Игрока
        if (other.CompareTag("Player") || other.GetComponent<RobotController>() != null)
        {
            Debug.Log("Синий куб уничтожен волной смерти!");
            GameManager.Instance.GameOver();
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

    // Вспомогательный метод, чтобы не дублировать код спавна частиц
    private void SpawnEffect(GameObject prefab, Vector3 position)
    {
        if (prefab != null)
        {
            GameObject effect = Instantiate(prefab, position, Quaternion.identity);
            Destroy(effect, 1.5f); // Авто-удаление через 1.5 сек, чтобы не забивать память телефона
        }
    }
}