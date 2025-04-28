using UnityEngine;

public class Obstaculos : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que colisiona es el jugador
        if (other.CompareTag("Player"))
        {
            // Obtener el script del jugador
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Detener al jugador y empujarlo hacia atrás
                StartCoroutine(playerController.HandleObstacleCollision());
            }
        }
    }
}
