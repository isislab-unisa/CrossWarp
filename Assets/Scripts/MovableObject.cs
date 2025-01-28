using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class MovableObject : NetworkBehaviour
{
    [Networked]
    public PhoneRepresentation isSelectedBy {get; set;}
    [Networked, OnChangedRender(nameof(OnSelectionColorChanged))]
    public Color selectionColor {get; set;}
    [Networked, OnChangedRender(nameof(OnSelectedChanged))]
    public bool selected {get; set;}
    private bool isVisible = false;
    [Networked, OnChangedRender(nameof(OnControlledChanged))]
    public bool controlledByAR {get; set;}
    [Networked, OnChangedRender(nameof(OnCurrentPositionChanged))]
    public Vector3 currentPosition {get; set;}
    public MeshRenderer meshRenderer;
    public Collider collider;
    [Networked]
    public Vector3 lastOffsetToSubplane {get; set;}
    [Networked]
    public Quaternion lastRotationOffsetToSubplane {get; set;}
    private GameObject activePhoneSubplane;

    
    public override void Spawned(){
        if(HasStateAuthority){
            if(PlatformManager.IsDesktop()){
                controlledByAR = false;
                lastRotationOffsetToSubplane = transform.rotation;
            }
            else{
                controlledByAR = true;
                SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
                if(subplaneConfig)
                    activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
                else
                    activePhoneSubplane = null;
                lastOffsetToSubplane = CalculateLastOffsetToSubplane();
                lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();
            }
        }
        if((GetComponent<NetworkObject>().Flags & NetworkObjectFlags.AllowStateAuthorityOverride) > 0)
            Debug.LogWarning("AllowStateAuthOverride attivato");
    }

    void Update()
    {
        if(PlatformManager.IsDesktop() && !controlledByAR){
            SetShowing(true);
        }
        else if(PlatformManager.IsDesktop() && controlledByAR){
            SetShowing(false);
        }
        else if(!PlatformManager.IsDesktop() && controlledByAR){
            SetShowing(true);
        }
        else if(!PlatformManager.IsDesktop() && !controlledByAR){
            SetShowing(false);
        }

        if(!PlatformManager.IsDesktop()){
            SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig){
                GameObject localSubplane = subplaneConfig.GetSelectedSubplane();
                if(localSubplane){
                    transform.position = lastOffsetToSubplane + localSubplane.transform.position;
                    //transform.rotation = lastRotationOffsetToSubplane * localSubplane.transform.rotation;
                }
            }
        }
    }

    private void SetShowing(bool showing){
        meshRenderer.enabled = showing;
        collider.enabled = showing;
    }

    public async Task<bool> TrySelectObject(PhoneRepresentation playerSelecting){
        if(isSelectedBy == null){
            if(!HasStateAuthority){
                await GetComponent<NetworkObject>().WaitForStateAuthority();
            }

            // una volta che viene selezionato da qualcun altro cambio l'offset del subplane
            SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
            lastOffsetToSubplane = CalculateLastOffsetToSubplane();
            lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();

            Debug.Log("isSelectedBy: " + isSelectedBy);
            isSelectedBy = playerSelecting;
            //playerSelecting.SelectObject(this);
            Debug.Log("isSelectedBy: " + isSelectedBy);
            //if(playerSelecting.GetComponent<PhoneRepresentation>())
                //GetComponent<Outline>().OutlineColor = playerSelecting.interactionColor;
            //GetComponent<Outline>().enabled = true;
            selectionColor = playerSelecting.interactionColor;
            selected = true;
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

    public async void ReleaseSelection(){
        if(!HasStateAuthority){
            await GetComponent<NetworkObject>().WaitForStateAuthority();
        }
        isSelectedBy = null;
        selected = false;
    }

    // chiamata solo quando si interagisce con il display virtuale, 
    public void UpdateTransform(Vector3 newPosition, bool isControlledByAR){
        Debug.Log("Hanno chiamato update transform: " + newPosition);
        //currentPosition = newPosition;

        
        //GetComponent<NetworkTransform>().Teleport(newPosition);
        transform.position = newPosition;

        SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
        lastOffsetToSubplane = CalculateLastOffsetToSubplane();
        lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();

        controlledByAR = isControlledByAR;
        // Debug.Log("controllato da AR: " + controlledByAR);
        // Debug.LogWarning("offsetTouSub: " + lastOffsetToSubplane);
        // Debug.LogWarning("phone relative: " + newPosition);
        // Debug.LogWarning("renderPos: " + (lastOffsetToSubplane + activePhoneSubplane.transform.position));
        // StampaPosizioneRPC(lastOffsetToSubplane, newPosition);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void StampaPosizioneRPC(Vector3 lastOffsetToSubplane, Vector3 phonePosition){
        SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
        GameObject localPhoneSubplane;
        if(subplaneConfig)
            localPhoneSubplane = subplaneConfig.GetSelectedSubplane();
        else
            localPhoneSubplane = null;
        Debug.LogWarning("offsetTouSub: " + lastOffsetToSubplane);
        Debug.LogWarning("phone relative: " + phonePosition);
        if(localPhoneSubplane)
            Debug.LogWarning("renderPosRPC: " + (lastOffsetToSubplane + localPhoneSubplane.transform.position));
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

    public Vector3 CalculateLastOffsetToSubplane(){
        Vector3 offset = Vector3.zero;
        if(activePhoneSubplane)
            offset = transform.position - activePhoneSubplane.transform.position;
        
        return offset;
    }

    public Quaternion CalculateLastRotationOffsetToSubplane(){

        if(!activePhoneSubplane)
            return transform.rotation;
        
        Quaternion rotation = Quaternion.FromToRotation(transform.forward, activePhoneSubplane.transform.forward);
        return rotation;
    }

    public void OnControlledChanged(){
        Debug.Log("ControlledByArChanged: " + controlledByAR);
    }

    public void OnCurrentPositionChanged(){
        transform.position = currentPosition;
    }

    public void OnSelectionColorChanged(){
        GetComponent<Outline>().OutlineColor = selectionColor;
    }

    public void OnSelectedChanged(){
        GetComponent<Outline>().enabled = selected;
    }

}
