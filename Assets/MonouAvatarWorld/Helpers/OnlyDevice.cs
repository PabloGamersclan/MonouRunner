using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyDevice : MonoBehaviour
{

    public enum Device {Mobile, Desktop};

    public Device device;

#if (UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR)
    private const bool isMovile = true;
#else
    private const bool isMovile = false;
#endif

    // Start is called before the first frame update
    void Start()
    {
        if( (isMovile && device==Device.Desktop) || (!isMovile && device==Device.Mobile) ) gameObject.SetActive(false);
    }
}
