using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadNodding : MonoBehaviour
{
    public float seed = 0;
    public float speed = 1;
    public Vector2 maxAngles = Vector2.zero;
    public float zAngle = 0;

    private Vector3 initialOffset;
    private float offset = 0;

    // Start is called before the first frame update
    void Start()
    {
        initialOffset = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        offset+=speed;
        float noise = (Mathf.PerlinNoise(offset, seed)-0.5f) * Mathf.PI*2;
        Vector3 movemet = initialOffset + new Vector3(Mathf.Cos(noise)*maxAngles.x,Mathf.Sin(noise)*maxAngles.y, zAngle);
        transform.rotation = Quaternion.Euler(movemet.x, movemet.y, movemet.z);
    }
}
