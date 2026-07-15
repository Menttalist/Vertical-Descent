using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ссылки на UI Смерти")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText; // Текст для текущего результата на экране смерти
    [SerializeField] private TextMeshProUGUI highScoreText;  // Текст для лучшего рекорда на экране смерти

    [Header("Ссылки на ИГРОВОЙ HUD (Счетчики на экране во время игры)")]
    [SerializeField] private TextMeshProUGUI hudMetersText;      // НОВЫЙ: Сюда перетащи текст метров игры (слева сверху)
    [SerializeField] private TextMeshProUGUI hudDrillsCountText; // НОВЫЙ: Сюда перетащи текст количества буров

    [Header("Ссылка на игрока")]
    [SerializeField] private RobotController player;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Каждую секунду обновляем счетчики на экране, пока игрок жив
        if (player != null && gameOverPanel != null && !gameOverPanel.activeSelf)
        {
            // 1. ОБНОВЛЕНИЕ МЕТРОВ НА ЭКРАНЕ ИГРЫ
            if (hudMetersText != null)
            {
                float currentDepth = -player.transform.position.y;
                if (currentDepth < 0) currentDepth = 0;
                hudMetersText.text = $"{Mathf.RoundToInt(currentDepth)}м";
            }

            // 2. ОБНОВЛЕНИЕ КОЛИЧЕСТВА БУРОВ НА ЭКРАНЕ ИГРЫ
            if (hudDrillsCountText != null)
            {
                // Читаем напрямую из публичной переменной currentDrills робота
                hudDrillsCountText.text = player.currentDrills.ToString();
            }
        }
    }

    public void GameOver()
    {
        Debug.Log("Вызов экрана смерти!");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (player != null)
        {
            // 1. Считаем текущую глубину спуска
            float finalDepth = -player.transform.position.y;
            if (finalDepth < 0) finalDepth = 0;
            int currentScore = Mathf.RoundToInt(finalDepth);

            // Выводим текущий результат
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Глубина спуска: {currentScore} м";
            }

            // 2. РАБОТА С РЕКОРДОМ (PlayerPrefs)
            int oldHighScore = PlayerPrefs.GetInt("HighScore", 0);

            if (currentScore > oldHighScore)
            {
                PlayerPrefs.SetInt("HighScore", currentScore);
                PlayerPrefs.Save();
                oldHighScore = currentScore;
                Debug.Log($"Ура! Новый рекорд сохранен: {currentScore} м");
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"Рекорд: {oldHighScore} м";
            }
        }

        // Ставим игру на паузу
        Time.timeScale = 0f;
    }

    // НОВЫЙ МЕТОД ДЛЯ ПЛАВНОЙ СМЕРТИ С ЗАДЕРЖКОЙ
    public System.Collections.IEnumerator DelayedGameOver(float delay)
    {
        // Ждем указанное время (пока робот играет анимацию смерти)
        yield return new WaitForSeconds(delay);

        // Вызываем твой стандартный экран смерти
        GameOver();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
    }
}