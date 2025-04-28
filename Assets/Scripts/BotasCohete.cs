using UnityEngine;
using System.Collections;

public class BotasCohete : PoweUps
{
    public float extraJumpHeight = 5f;
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
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.jumpHeight += extraJumpHeight;
        }
    }

    public override void Deactivate(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.jumpHeight -= extraJumpHeight; // Restaurar altura del salto
        }
    }
}
