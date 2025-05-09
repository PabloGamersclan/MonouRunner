using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundRepeat : MonoBehaviour
{
    private float spriteWidth;

   
    void Start()
    {
        BoxCollider2D groundCollider = GetComponent<BoxCollider2D>();
        spriteWidth = groundCollider.size.x;
        Debug.Log(spriteWidth/2);
    }


    void Update()
    {
        if(transform.position.x < -12f)
        {
            ResetPosition();
        }
    }

    private void ResetPosition()
    {
        transform.Translate(new Vector3(spriteWidth, 0f, 0f));
    }
}
