using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopUI : MonoBehaviour
{
    public GameObject sphere;
    public void OnStartBtnPressed(){
        DesktopPlayerController desktopSphereController = FindObjectOfType<DesktopPlayerController>();
        desktopSphereController.Runner.Spawn(sphere, new Vector3(0, 0, 8.63f));
    }
}
