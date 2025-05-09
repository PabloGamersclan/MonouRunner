using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEditor;

public class CircadianShaderController : MonoBehaviour
{
    public float cicleDelay = 10f;
    private float val = 0;
    private float prevVal = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        val = (Mathf.Sin((Time.time%cicleDelay)*Mathf.PI*2/cicleDelay)+1f)/2;

        Shader.SetGlobalFloat("_CircadianFactor", val);
        if(Mathf.Abs(prevVal-val)<0.1){
            DynamicGI.UpdateEnvironment();
            prevVal = val;            
        }
        // EditorWindow view = EditorWindow.GetWindow<SceneView>();
        // view.Repaint();
        //Debug.Log(val);
    }
}
