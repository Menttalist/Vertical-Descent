using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DangerIndicator : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform waveTransform;    // Ссылка на объект волны DeathWave
    [SerializeField] private Transform cameraTransform;  // Ссылка на Main Camera
    [SerializeField] private GameObject indicatorVisual; // Сам визуал индикатора (контейнер с Image и Text)
    [SerializeField] private TextMeshProUGUI distanceText; // Текст для вывода метров до волны

    [Header("Настройки дистанции")]
    [SerializeField] private float showThreshold = 32f;   // На каком расстоянии НАВЕРХУ индикатор начинает показываться
    [SerializeField] private float criticalDistance = 12f; // Дистанция, ближе которой волна уже прямо над головой

    private Image indicatorImage;
    private float blinkTimer;

    void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        indicatorImage = indicatorVisual.GetComponentInChildren<Image>();
    }

    void Update()
    {
        if (waveTransform == null || cameraTransform == null) return;

        // Считаем разницу по высоте между волной и камерой
        float distanceToWave = waveTransform.position.y - cameraTransform.position.y;

        // Если волна близко к камере (внутри экрана или у границы), индикатор прячем, его и так видно
        if (distanceToWave <= 4f)
        {
            indicatorVisual.SetActive(false);
            return;
        }


        // --- ХАРДКОРНАЯ ЗОНА: Волна приближается сверху! ---
        if (distanceToWave <= showThreshold && distanceToWave > 4f)
        {
            indicatorVisual.SetActive(true);

            // Выводим точное расстояние до волны
            if (distanceText != null)
            {
                distanceText.text = $"{Mathf.RoundToInt(distanceToWave)}m";
            }

            // Динамическое мигание: чем ближе волна, тем быстрее мигает альфа-канал (прозрачность)
            float blinkSpeed = Mathf.Lerp(10f, 2f, (distanceToWave - criticalDistance) / (showThreshold - criticalDistance));
            blinkTimer += Time.deltaTime * blinkSpeed;

            float alpha = (Mathf.Sin(blinkTimer) + 1f) / 2f; // Синусоида от 0 до 1

            // Если дистанция критическая — перекрашиваем в агрессивный красный
            if (distanceToWave <= criticalDistance)
            {
                indicatorImage.color = new Color(1f, 0f, 0f, alpha);
            }
            else
            {
                // Плавный переход цвета от желтого к красному при приближении
                float colorLerp = Mathf.InverseLerp(showThreshold, criticalDistance, distanceToWave);
                Color targetColor = Color.Lerp(Color.yellow, Color.red, colorLerp);
                targetColor.a = alpha;
                indicatorImage.color = targetColor;
            }
        }
    }
}