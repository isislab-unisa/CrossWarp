using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class SubplaneAnchor : MonoBehaviour
{
    private Vector3 lastPosition = Vector3.zero;
    private GameObject subplane = null;

    void Start(){
        lastPosition = transform.localPosition;
    }

    void Update(){
        if(GetComponent<ARSelectionInteractable>().isSelected){
            IsUsePlane();
        }
        if(lastPosition != transform.localPosition){
            lastPosition = transform.localPosition;
            if(!subplane)
                return;
            subplane.GetComponent<Subplane>().OnAnchorMoved();
        }
    }

    public void SetSubplane(GameObject subplane){
        this.subplane = subplane;
    }

    public void IsUsePlane(){
        if(FindFirstObjectByType<SubplaneConfig>().configurationMode == SubplaneConfig.ConfigurationMode.InSpace){
            GetComponent<ARTranslationInteractable>().enabled = false;
            GetComponent<CustomARTranslateInteractable>().enabled = true;
        }
        else{
            GetComponent<ARTranslationInteractable>().enabled = true;
            GetComponent<CustomARTranslateInteractable>().enabled = false;
        }
    }
}
