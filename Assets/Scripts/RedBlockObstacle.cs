using UnityEngine;

public class RedBlockObstacle : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, что в красный блок врезался именно робот
        RobotController robot = collision.gameObject.GetComponent<RobotController>();

        if (robot != null)
        {
            Debug.Log("Робот наступил на красный блок! Запускаем красивую смерть...");

            // Вместо мгновенного вызова GameOver запускаем метод Die() на самом роботе.
            // Робот заблокирует управление, включит анимацию "DieTrigger" и сам 
            // запустит отложенный показ экрана смерти в GameManager!
            robot.Die();
        }
    }
}