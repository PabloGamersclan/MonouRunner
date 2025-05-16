using UnityEngine;
using System.Collections;

public class Fuel : PoweUps
{
    public float extraDuration = 5f;

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
        yield return new WaitForSeconds(duration);
        Deactivate(null);
    }

    public override void Activate(GameObject player)
    {
        Jetpack Jetpack = player.GetComponentInChildren<Jetpack>();
        if (Jetpack != null)
        {
            Jetpack.duration += extraDuration;
        }
    }

    public override void Deactivate(GameObject player)
    {
        // No se necesita lógica adicional para restaurar el control del personaje
    }
}
