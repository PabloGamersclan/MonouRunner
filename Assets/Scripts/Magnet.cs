using UnityEngine;
using System.Collections;

public class Magnet : PoweUps
{
    public float radius = 5f;
    public float duration = 10f;

    private void Start()
    {
        StartCoroutine(DeactivateAfterDuration());
    }

    private IEnumerator DeactivateAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        Deactivate(null);
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
