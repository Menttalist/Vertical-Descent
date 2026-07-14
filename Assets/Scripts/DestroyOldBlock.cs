using UnityEngine;

public class DestroyOldBlock : MonoBehaviour
{
    public Transform playerTransform;
    [SerializeField] private float destroyDistance = 12f; // Расстояние выше игрока, при котором блок удаляется

    void Update()
    {
        if (playerTransform != null)
        {
            // Если блок оказался выше игрока на величину destroyDistance — удаляем его
            if (transform.position.y > playerTransform.position.y + destroyDistance)
            {
                Destroy(gameObject);
            }
        }
    }
}