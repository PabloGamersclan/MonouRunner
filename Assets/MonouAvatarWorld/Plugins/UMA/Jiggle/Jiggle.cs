using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

public class Jiggle : MonoBehaviour 
{
    [Header("General Settings")]
    public string jiggleBoneName;
    public string[] colliderBoneNames;
    public List<string> exceptions;
    [Range(0,1)]
    public float reduceEffect;
    
    [Header("Removable Bone Settings")]
    public bool deleteBoneWithSlot;
    public string slotToWatch;
    private string linkedRecipe;
    
    public void AddJiggle(UMAData umaData)
    {
        Transform rootBone = umaData.gameObject.transform.FindDeepChild(jiggleBoneName);
        UMABoneCleaner cleaner = umaData.gameObject.GetComponent<UMABoneCleaner>();
        
        if(rootBone != null)
        {           
            DynamicBone jiggleBone = rootBone.GetComponent<DynamicBone>();
            if(jiggleBone == null)
            {
                jiggleBone = rootBone.gameObject.AddComponent<DynamicBone>();
            }
            
            jiggleBone.m_Root = rootBone;
            jiggleBone.m_Gravity = new Vector3(0,-0.2f,0);
            jiggleBone.m_UpdateRate = 20f;
            jiggleBone.m_Damping = 0.5f;
            jiggleBone.m_Radius = 0.05f;
            foreach(string boneName in colliderBoneNames){
                Transform colliderBone = umaData.gameObject.transform.FindDeepChild(boneName);
                Debug.Log(umaData.gameObject);
                Debug.Log(boneName);
                Debug.Log(colliderBone);
                if(colliderBone==null) continue;
                DynamicBoneCollider dbcb = colliderBone.gameObject.GetComponent<DynamicBoneCollider>();
                if(dbcb==null){
                    dbcb = colliderBone.gameObject.AddComponent<DynamicBoneCollider>();
                    dbcb.m_Direction = DynamicBoneColliderBase.Direction.Y;
                    dbcb.m_Radius = 0.12f;
                    dbcb.m_Height = 0.4f;
                }
                jiggleBone.m_Colliders = new List<DynamicBoneColliderBase>();
                jiggleBone.m_Colliders.Add(dbcb);
            }
            
            List<Transform> exclusionList = new List<Transform>();
            
            foreach(string exception in exceptions)
            {
                exclusionList.Add(umaData.gameObject.transform.FindDeepChild(exception));
            }
            
            jiggleBone.m_Exclusions = exclusionList;
            jiggleBone.m_Inert = reduceEffect;
            jiggleBone.UpdateParameters();
        }
        
        if(deleteBoneWithSlot)
        {
            if(cleaner == null)
                cleaner = umaData.gameObject.AddComponent<UMABoneCleaner>();
            
            UMAJiggleBoneListing listing = new UMAJiggleBoneListing();
            listing.boneName = jiggleBoneName;
            listing.carrierSlot = slotToWatch;
            
            linkedRecipe = umaData.gameObject.GetComponent<DynamicCharacterAvatar>().GetWardrobeItemName(slotToWatch);
            
            listing.recipe = linkedRecipe;
            cleaner.RegisterJiggleBone(listing);
        }
    }
}