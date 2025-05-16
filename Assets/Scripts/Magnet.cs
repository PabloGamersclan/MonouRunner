using UnityEngine;
using System.Collections;

public class Magnet : PoweUps
{
    public float radius = 5f;
    public float _duration = 10f;

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
        Coin[] coins = FindObjectsOfType<Coin>();
        foreach (Coin coin in coins)
        {
            if (Vector3.Distance(player.transform.position, coin.transform.position) <= radius)
            {
                coin.transform.position = Vector3.MoveTowards(coin.transform.position, player.transform.position, 10f * Time.deltaTime);
            }
        }
    }

    public override void Deactivate(GameObject player)
    {
        // No se necesita lógica adicional para restaurar el control del personaje, pero se puede detener la atracción activa si es necesario.
    }
}
