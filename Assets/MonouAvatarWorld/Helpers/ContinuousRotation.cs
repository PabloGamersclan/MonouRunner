using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinusRotation : MonoBehaviour
{
    public Vector3 angleStep = Vector3.zero;
    public float speed = 1f;

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = angleStep * speed * Time.deltaTime;
        transform.Rotate(delta.x, delta.y, delta.z, Space.Self);
    }
}
