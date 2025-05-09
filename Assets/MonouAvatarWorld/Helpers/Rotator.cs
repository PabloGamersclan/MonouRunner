using UnityEngine;
public class Rotator : MonoBehaviour
{
    public Vector3 angularVelocity;
    public Space space = Space.Self;
    void Update ()
    {
        transform.Rotate(angularVelocity * Time.deltaTime, space);
    }
}