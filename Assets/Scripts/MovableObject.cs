using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MovableObject : NetworkBehaviour
{
    [Networked]
    public PhoneRepresentation isSelectedBy {get; set;}
    private bool isVisible = false;
    [Networked, OnChangedRender(nameof(OnControlledChanged))]
    public bool controlledByAR {get; set;}

    
    public override void Spawned(){
        if(HasStateAuthority){
            if(PlatformManager.IsDesktop())
                controlledByAR = false;
            else
                controlledByAR = true;
        }
    }

    void Update()
    {
        if(PlatformManager.IsDesktop() && !controlledByAR)
            GetComponentInChildren<MeshRenderer>().enabled = true;
        else if(PlatformManager.IsDesktop() && controlledByAR)
            GetComponentInChildren<MeshRenderer>().enabled = false;
        else if(!PlatformManager.IsDesktop() && controlledByAR)
            GetComponentInChildren<MeshRenderer>().enabled = true;
        else if(!PlatformManager.IsDesktop() && !controlledByAR)
            GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    public bool TrySelectObject(PhoneRepresentation playerSelecting){
        if(isSelectedBy == null){
            Debug.Log("isSelectedBy: " + isSelectedBy);
            isSelectedBy = playerSelecting;
            playerSelecting.SelectObject(this);
            Debug.Log("isSelectedBy: " + isSelectedBy);
            //if(playerSelecting.GetComponent<PhoneRepresentation>())
                //GetComponent<Outline>().OutlineColor = playerSelecting.interactionColor;
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void UpdateTransformRPC(Vector3 newPosition, bool isControlledByAR){
        Debug.Log("Hanno chiamato update transform");
        transform.position = newPosition;
        controlledByAR = isControlledByAR;
        Debug.Log("controllato da AR: " + controlledByAR);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void SetControlledByARRPC(bool isControlledByAR){
        controlledByAR = isControlledByAR;
        Debug.Log("controllato da AR: " + controlledByAR);
    }

    public void OnControlledChanged(){
        Debug.Log("ControlledByArChanged: " + controlledByAR);
    }
}
