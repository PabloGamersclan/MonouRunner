using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Slider sfxSlider;

    public void ToggleSFX()
    {
        GameManager.Intance.ToggleSFX();
    }

    public void SFXVolume()
    {
        GameManager.Intance.SFXVolume(sfxSlider.value);
    }
}
