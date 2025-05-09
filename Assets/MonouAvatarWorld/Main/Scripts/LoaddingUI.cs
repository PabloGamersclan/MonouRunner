using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class LoaddingUI : MonoBehaviour
{
        private UIDocument ui;
        private VisualElement root;
        private VisualElement icon;

    // Start is called before the first frame update
    void Start()
    {
        // ui = GetComponent<UIDocument>();
        // root = ui.rootVisualElement;
        // root.style.height = Length.Percent(100);
        // icon = root[0][0];
        // icon.schedule.Execute(() => {
        //     icon.style.rotate = new Rotate(Time.time * 180);
        // }).Every(32);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnEnable(){
        ui = GetComponent<UIDocument>();
        root = ui.rootVisualElement;
        root.style.height = Length.Percent(100);
        icon = root[0][0];
        icon.schedule.Execute(() => {
            icon.style.rotate = new Rotate(Time.time * 180);
        }).Every(32);
    }
}
