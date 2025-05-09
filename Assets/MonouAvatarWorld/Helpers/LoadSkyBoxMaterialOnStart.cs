using UnityEngine;

public class LoadSkyBoxMaterialOnStart : MonoBehaviour
{

    public Material skyboxMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        RenderSettings.skybox = skyboxMaterial;
    }

}
