using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAsPlayerOnStart : MonoBehaviour
{
    void Start(){
        Main.inst.SetAvatarMimic(gameObject);
    }
}
