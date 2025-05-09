using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            GameManager.Intance.IncreaseScore();
            GameManager.Intance.PlaySFX("Point");
        }
        else
        { 
            Debug.Log("Collided with something not scorable"); 
            return;
        }
    }
}
