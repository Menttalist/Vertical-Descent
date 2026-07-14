using UnityEngine;

public class LockCameraX : MonoBehaviour
{
    [Header("Фиксированная позиция по X")]
    [SerializeField] private float lockedX = 0f; // Центр твоей шахты

    void LateUpdate()
    {
        // Намертво привязываем позицию объекта к нулю по X
        transform.position = new Vector3(lockedX, transform.position.y, transform.position.z);
    }
}