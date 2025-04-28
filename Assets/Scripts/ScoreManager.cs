using UnityEngine;
using TMPro; // Importar el espacio de nombres para TextMeshPro

public class ScoreManager : MonoBehaviour
{
    public Transform player; // Referencia al jugador
    public TextMeshProUGUI distanceText; // Referencia al texto de la UI
    public TextMeshProUGUI coinText; // Referencia al texto del contador de monedas
    private Vector3 startPosition; // Posición inicial del jugador
    private int coinCount = 0; // Contador de monedas
    public int coinMultiplier = 1; // Multiplicador de monedas

    private void Start()
    {
        // Guardar la posición inicial del jugador
        if (player != null)
        {
            startPosition = player.position;
        }

        // Inicializar el texto del contador de monedas
        UpdateCoinText();
    }

    private void Update()
    {
        // Calcular la distancia recorrida y actualizar el texto
        if (player != null && distanceText != null)
        {
            float distance = Vector3.Distance(startPosition, player.position);
            distanceText.text = "Distancia: " + Mathf.FloorToInt(distance) + " m";
        }
    }

    public void AddCoin()
    {
        // Incrementar el contador de monedas y actualizar el texto
        coinCount += coinMultiplier;
        UpdateCoinText();
    }

    private void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = "Monedas: " + coinCount;
        }
    }
    public float GetDistance()
{
    // Retorna la distancia recorrida
    return Vector3.Distance(startPosition, player.position);
}

public int GetCoins()
{
    // Retorna el número de monedas recolectadas
    return coinCount;
}
}
