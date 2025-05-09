using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.left * speed;
    }

   
    void Update()
    {
        if (GameManager.Intance.isGameOver)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
