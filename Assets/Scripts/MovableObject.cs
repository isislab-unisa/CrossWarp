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
    private bool isVisible = false;
    [Networked, OnChangedRender(nameof(OnControlledChanged))]
    public bool controlledByAR {get; set;}
    [Networked, OnChangedRender(nameof(OnCurrentPositionChanged))]
    public Vector3 currentPosition {get; set;}
    public MeshRenderer meshRenderer;
    public Collider collider;
    [Networked]
    public Vector3 lastOffsetToSubplane {get; set;}
    private GameObject activePhoneSubplane;

    
    public override void Spawned(){
        if(HasStateAuthority){
            if(PlatformManager.IsDesktop())
                controlledByAR = false;
            else{
                controlledByAR = true;
                SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
                if(subplaneConfig)
                    activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
                else
                    activePhoneSubplane = null;
                CalculateLastOffsetToSubplane();
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
                if(localSubplane)
                    transform.position = lastOffsetToSubplane + localSubplane.transform.position;
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
            CalculateLastOffsetToSubplane();

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

    // chiamata solo quando si interagisce con il display virtuale, 
    public void UpdateTransform(Vector3 newPosition, bool isControlledByAR){
        Debug.Log("Hanno chiamato update transform: " + newPosition);
        //currentPosition = newPosition;

        
        //GetComponent<NetworkTransform>().Teleport(newPosition);
        transform.position = newPosition;


        CalculateLastOffsetToSubplane(); 
        controlledByAR = isControlledByAR;
        Debug.Log("controllato da AR: " + controlledByAR);
        Debug.LogWarning("offsetTouSub: " + lastOffsetToSubplane);
        Debug.LogWarning("phone relative: " + newPosition);
        Debug.LogWarning("renderPos: " + (lastOffsetToSubplane + activePhoneSubplane.transform.position));
        StampaPosizioneRPC(lastOffsetToSubplane, newPosition);
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

    public void OnControlledChanged(){
        Debug.Log("ControlledByArChanged: " + controlledByAR);
    }

    public void OnCurrentPositionChanged(){
        transform.position = currentPosition;
    }

    public void CalculateLastOffsetToSubplane(){
        if(activePhoneSubplane)
            lastOffsetToSubplane = transform.position - activePhoneSubplane.transform.position;
        else 
            lastOffsetToSubplane = Vector3.zero;
    }
}
