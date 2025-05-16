using UnityEngine;
using System.Collections;

public class Jetpack : PoweUps
{
    public float _duration = 5f; // Duración del power-up en segundos
    public Camera mainCamera; // Referencia a la cámara principal

    private void Start()
    {
        //StartCoroutine(DeactivateAfterDuration());
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Power-up activado"); // Mensaje de depuración
            Activate(other.gameObject);
            StartCoroutine(DeactivateAfterDuration(other.gameObject));
            gameObject.transform.position = new Vector3(0, -10, 0); // Desplazar el objeto a una posición fuera de la vista 
        }
    }

    private IEnumerator DeactivateAfterDuration(GameObject player)
    {
        yield return new WaitForSeconds(_duration);
        Deactivate(player);
    }

    public override void Activate(GameObject player)
    {
        player.transform.position += new Vector3(0, 5f, 0); // Elevar al jugador
        // Aquí puedes activar una barra de duración en la UI
    }

    public override void Deactivate(GameObject player)
    {
        if (player != null)
        {
            player.transform.position -= new Vector3(0, 10f, 0); // Restaurar posición en Y

            if (mainCamera != null)
            {
                mainCamera.transform.position -= new Vector3(0, 10f, 0); // Restaurar posición de la cámara
            }
            else
            {
                Debug.LogError("La referencia a la cámara principal no está asignada. Por favor, asigna la cámara en el Inspector.");
            }
        }
        // Aquí puedes desactivar la barra de duración en la UI
    }
}
