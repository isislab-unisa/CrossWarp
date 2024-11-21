using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ARObjects;
    [SerializeField]
    private List<GameObject> DesktopObjects;

    public static bool IsDesktop(){
        if(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            return true;
        return false;
    }

    void Start()
    {
        if(IsDesktop())
            SetupDesktopMode();
        else
            SetupARMode();
    }

    void SetupARMode()
    {
        foreach(GameObject x in ARObjects){
            x.SetActive(true);
        }
        foreach(GameObject x in DesktopObjects){
            x.SetActive(false);
        }
    }

    void SetupDesktopMode()
    {
        foreach(GameObject x in ARObjects){
            x.SetActive(false);
        }
        foreach(GameObject x in DesktopObjects){
            x.SetActive(true);
        }
        Debug.Log("larghezza: " + Screen.width/Screen.dpi);
        Debug.Log("altezza: " + Screen.height/Screen.dpi);
    }
}
