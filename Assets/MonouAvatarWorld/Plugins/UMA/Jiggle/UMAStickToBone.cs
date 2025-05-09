using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

public class UMAStickToBone : MonoBehaviour 
{
    public static string boneName;
    public static string slotToWatch;
    public static Vector3 position = Vector3.zero;
    public static string objectResource;

    public static void SetBoneName(string bn){ UMAStickToBone.boneName = bn; }
    public static void SetSlotToWatch(string stw){ UMAStickToBone.slotToWatch = stw; }
    public static void ZeroPosition(){ UMAStickToBone.position = Vector3.zero; }
    public static void SetPositionX(float pos){ UMAStickToBone.position.x = pos; }
    public static void SetPositionY(float pos){ UMAStickToBone.position.y = pos; }
    public static void SetPositionZ(float pos){ UMAStickToBone.position.z = pos; }
    public static void SetResource(string obj){ UMAStickToBone.objectResource = obj; }

    private string linkedRecipe;
    
    public void AddObject(UMAData umaData)
    {

        Transform rootBone = umaData.gameObject.transform.FindDeepChild(UMAStickToBone.boneName);
        UMABoneCleaner cleaner = umaData.gameObject.GetComponent<UMABoneCleaner>();
        
        if(rootBone != null)
        {           
            GameObject obj = (GameObject)Instantiate((GameObject)Resources.Load(objectResource), Vector3.zero, rootBone.rotation);
            obj.transform.parent = rootBone;
            obj.transform.localPosition = UMAStickToBone.position;
        }
        
        
        if(cleaner == null)
            cleaner = umaData.gameObject.AddComponent<UMABoneCleaner>();
        
        UMAJiggleBoneListing listing = new UMAJiggleBoneListing();
        listing.boneName = UMAStickToBone.boneName;
        listing.carrierSlot = UMAStickToBone.slotToWatch;
        
        linkedRecipe = umaData.gameObject.GetComponent<DynamicCharacterAvatar>().GetWardrobeItemName(UMAStickToBone.slotToWatch);
        
        listing.recipe = linkedRecipe;
        cleaner.RegisterJiggleBone(listing);
    }
}
 