using UnityEngine;
using System.Collections; // Importar el espacio de nombres para IEnumerator

public abstract class PoweUps : MonoBehaviour // Cambiar la clase a abstract
{
    public float duration; // Duración del power-up

    public abstract void Activate(GameObject player);
    public abstract void Deactivate(GameObject player);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Activate(other.gameObject);
            StartCoroutine(DeactivateAfterDuration(other.gameObject));
            Destroy(gameObject); // Destruir el power-up tras recogerlo
        }
    }

    private IEnumerator DeactivateAfterDuration(GameObject player)
    {
        yield return new WaitForSeconds(duration);
        Deactivate(player);
    }
}
