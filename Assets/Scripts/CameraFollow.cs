using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("За кем следить")]
    public Transform target; // Сюда перетащим робота

    [Header("Смещение вниз")]
    public float yOffset = -2.5f; // Опускаем камеру пониже, чтобы видеть блоки внизу

    void LateUpdate()
    {
        // Проверяем, привязан ли робот, чтобы Unity не ругалась ошибками
        if (target != null)
        {
            // Берем текущую позицию камеры, но Y заменяем на Y робота + смещение
            Vector3 newPosition = new Vector3(transform.position.x, target.position.y + yOffset, transform.position.z);

            // Перемещаем камеру в эту точку
            transform.position = newPosition;
        }
    }
}