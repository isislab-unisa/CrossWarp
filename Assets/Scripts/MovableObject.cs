using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MovableObject : NetworkBehaviour
{
    [Networked]
    public PhoneRepresentation isSelectedBy {get; set;}
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool TrySelectObject(PhoneRepresentation playerSelecting){
        if(isSelectedBy == null){
            Debug.Log("isSelectedBy: " + isSelectedBy);
            isSelectedBy = playerSelecting;
            Debug.Log("isSelectedBy: " + isSelectedBy);
            //if(playerSelecting.GetComponent<PhoneRepresentation>())
                GetComponent<Outline>().OutlineColor = playerSelecting.interactionColor;
            GetComponent<Outline>().enabled = true;
            return true;
        }
        else if(isSelectedBy != playerSelecting){
            return false;
        }
        else{
            ReleaseSelection();
            return false;
        }

    }

    public void ReleaseSelection(){
        isSelectedBy = null;
        GetComponent<Outline>().enabled = false;
    }
}
