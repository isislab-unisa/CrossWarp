using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    
    public void OnInteractableDemoBtnPressed(){
        SceneManager.LoadScene("InteractableScene");
    }
    
    public void OnMirrorDemoBtnPressed(){
        SceneManager.LoadScene("MirrorScene");
    }

    public void OnHouseDemoBtnPressed(){
        SceneManager.LoadScene("HouseScene");
    }

    public void OnRollingSphereDemoBtnPressed(){
        SceneManager.LoadScene("RollingSphereScene");
    }

}
