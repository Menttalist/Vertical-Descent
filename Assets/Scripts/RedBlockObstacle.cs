using UnityEngine;
using UnityEngine.SceneManagement; // Нужно для перезапуска сцены

public class RedBlockObstacle : MonoBehaviour
{
    // Этот метод срабатывает, когда КТО-ТО физически врезается в коллайдер этого блока
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, что врезался именно наш управляемый синий куб
        // (Убедись, что на твоем синем кубе висит скрипт RobotController)
        if (collision.gameObject.GetComponent<RobotController>() != null)
        {
            Debug.Log("Синий куб разбился о красный блок!");

            // Перезапускаем текущую сцену с нуля
            GameManager.Instance.GameOver();
        }
    }
}