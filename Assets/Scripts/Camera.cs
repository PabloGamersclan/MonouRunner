using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform player; // Referencia al jugador
    public Vector3 offset; // Desplazamiento relativo entre la cámara y el jugador

    private void LateUpdate()
    {
        // Actualizar la posición de la cámara para que siga al jugador
        if (player != null)
        {
            transform.position = new Vector3(
                player.position.x, // Mantener la posición fija en X
                player.position.y + offset.y, // Seguir al jugador en Y
                player.position.z + offset.z // Seguir al jugador en Z
            );
        }
    }
}
