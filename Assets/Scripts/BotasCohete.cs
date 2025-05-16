using UnityEngine;
using System.Collections;

public class BotasCohete : PoweUps
{
    public float extraJumpHeight = 5f;
    public float _duration = 10f;

    private void Start()
    {
        //StartCoroutine(DeactivateAfterDuration());
    }

    private IEnumerator DeactivateAfterDuration(GameObject player)
    {
        Debug.Log("desactivando power-up en: " + _duration + "s"); // Mensaje de depuración
        yield return new WaitForSeconds(_duration);
        Debug.Log("ya pasaron " + _duration + "ahora hay que desactivar el power up de " + player); // Mensaje de depuración
        Deactivate(player);
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
    public override void Activate(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.jumpHeight += extraJumpHeight;
        }
    }

    public override void Deactivate(GameObject player)
    {
        Debug.Log("desactivando power-up depues de la espera"); // Mensaje de depuración
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.jumpHeight -= extraJumpHeight; // Restaurar altura del salto
            Debug.Log("Power-up Desactivado"); // Mensaje de depuración
        }
    }
}
