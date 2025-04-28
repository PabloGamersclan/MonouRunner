using UnityEngine;
using System.Collections;

public class DoubleCoins : PoweUps
{
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
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.coinMultiplier = 2;
        }
    }

    public override void Deactivate(GameObject player)
    {
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.coinMultiplier = 1; // Restaurar multiplicador de monedas
        }
    }
}
