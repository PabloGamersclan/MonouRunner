using UnityEngine;
using System.Collections;

public class Fuel : PoweUps
{
    public float extraDuration = 5f;

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
