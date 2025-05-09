using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float upForce = 350f;
    private Rigidbody2D playerRb;
    private bool isDead;
    //private Animator playerAnimator;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        //playerAnimator = GetComponent<Animator>();
    }



    void Update()
    {
        bool isInput = (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));

        // Mobile input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            isInput = true;
        }

        if (isInput && !isDead && !GameManager.Intance.isPause)
        {
            Flap();
        }
    }

    private void Flap()
    {
        playerRb.linearVelocity = Vector2.zero;
        playerRb.AddForce(Vector2.up * upForce);
        //playerAnimator.SetTrigger("Flip");
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "death")
        {
            isDead = true;
            //playerAnimator.SetTrigger("Die");
            GameManager.Intance.GameOver();
        }
    }
}
