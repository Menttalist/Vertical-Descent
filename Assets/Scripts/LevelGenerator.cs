using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Префабы блоков")]
    public GameObject normalBlockPrefab;
    public GameObject redBlockPrefab;
    public GameObject wallBlockPrefab; // Префаб боковой 3D стены

    [Header("Настройки сетки")]
    [SerializeField] private float columnWidth = 1.0f;
    [SerializeField] private float rowHeight = 3.5f;

    [Header("Слежка за игроком")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float bufferDistance = 15f;

    private float nextRowY = -2f;

    // Переменная для хранения индекса дыры из ПРЕДЫДУЩЕГО ряда
    private int lastEmptyColumn = 2; // По умолчанию центр, так как игрок стартует там

    void Start()
    {
        // Первый ряд делаем полностью безопасным под игроком
        GenerateRow(true);

        // Остальные ряды генерируем в цикле
        for (int i = 0; i < 9; i++)
        {
            GenerateRow(false);
        }

        QualitySettings.vSyncCount = 0;

        // Выставляем лимит кадров на максимум (например, 120 FPS, или -1 для работы без ограничений)
        Application.targetFrameRate = 120;
    }

    void Update()
    {
        if (playerTransform == null) return;

        while (playerTransform.position.y - nextRowY < bufferDistance)
        {
            GenerateRow(false);
        }
    }

    // Добавили параметр Тормоз/Безопасность (isFirstRow)
    public void GenerateRow(bool isFirstRow)
    {
        // 1. Выбираем НОВУЮ пустую колонку для этого ряда
        int emptyColumn = Random.Range(0, 5);

        // 2. Логика спавна красного блока
        int redColumn = -1;

        // Если это не самый первый ряд, и сработал шанс 30%
        if (!isFirstRow && Random.Range(0f, 1f) < 0.3f)
        {
            redColumn = Random.Range(0, 5);

            // Условие жесткой проверки:
            // Красный блок НЕ должен быть там, где дыра СЕЙЧАС (emptyColumn)
            // И НЕ должен быть там, где была дыра НАД НИМ (lastEmptyColumn)
            while (redColumn == emptyColumn || redColumn == lastEmptyColumn)
            {
                redColumn = Random.Range(0, 5);
            }
        }

        // ====== СПАВН БОКОВЫХ СТЕН ШАХТЫ ======
        if (wallBlockPrefab != null)
        {
            // Левая стена: на одну колонку левее самой левой игровой (индекс -1)
            Vector3 leftWallPosition = new Vector3((-1 - 2) * columnWidth, nextRowY, 0f);
            GameObject leftWall = Instantiate(wallBlockPrefab, leftWallPosition, Quaternion.identity);
            leftWall.AddComponent<DestroyOldBlock>().playerTransform = playerTransform;

            // Правая стена: на одну колонку правее самой правой игровой (индекс 5)
            Vector3 rightWallPosition = new Vector3((5 - 2) * columnWidth, nextRowY, 0f);
            GameObject rightWall = Instantiate(wallBlockPrefab, rightWallPosition, Quaternion.identity);
            rightWall.AddComponent<DestroyOldBlock>().playerTransform = playerTransform;
        }
        // ======================================

        // 3. Спавним 5 игровых блоков
        for (int col = 0; col < 5; col++)
        {
            if (col == emptyColumn) continue;

            Vector3 spawnPosition = new Vector3((col - 2) * columnWidth, nextRowY, 0f);
            GameObject blockToSpawn = (col == redColumn) ? redBlockPrefab : normalBlockPrefab;

            GameObject newBlock = Instantiate(blockToSpawn, spawnPosition, Quaternion.identity);
            newBlock.AddComponent<DestroyOldBlock>().playerTransform = playerTransform;
        }

        // 4. ЗАПОМИНАЕМ дыру этого ряда для следующего шага генерации
        lastEmptyColumn = emptyColumn;

        nextRowY -= rowHeight;
    }
}