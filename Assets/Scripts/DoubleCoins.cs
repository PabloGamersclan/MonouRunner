using UnityEngine;
using System.Collections;

public class DoubleCoins : PoweUps
{
    
    public float _duration = 10f; // Duración del power-up en segundos
    private void Start()
    {
        // StartCoroutine(DeactivateAfterDuration());
    }

    private IEnumerator DeactivateAfterDuration(GameObject player)
    {
        yield return new WaitForSeconds(_duration);
        Deactivate(null);
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
