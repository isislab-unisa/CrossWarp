using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
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
    [Networked]
    public Vector3 networkedScale {get; set;}
    private GameObject activePhoneSubplane;
    public float minScale = 0.25f;
    public float maxScale = 2.5f;

    
    public override void Spawned(){
        if(HasStateAuthority){
            if(PlatformManager.IsDesktop()){
                controlledByAR = false;
                lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();
                lastOffsetToSubplane = CalculateLastOffsetToSubplane(transform.position);
            }
            else{
                controlledByAR = true;
                SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
                if(subplaneConfig)
                    activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
                else
                    activePhoneSubplane = null;
                lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();
                lastOffsetToSubplane = CalculateLastOffsetToSubplane(transform.position);
            }
            networkedScale = transform.localScale;
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
                    transform.position = Vector3.Lerp(transform.position, localSubplane.transform.TransformPoint(lastOffsetToSubplane), 0.5f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, localSubplane.transform.rotation * lastRotationOffsetToSubplane, 0.5f);
                }
            }
        }
        else{
            transform.position = Vector3.Lerp(transform.position, Camera.main.transform.TransformPoint(lastOffsetToSubplane), 0.5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Camera.main.transform.rotation * lastRotationOffsetToSubplane, 0.5f);
        }

        if(networkedScale != null && transform.localScale != networkedScale){
            transform.localScale = Vector3.Lerp(transform.localScale, networkedScale, 0.5f);
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

            // una volta che viene selezionato da qualcun altro cambio l'offset in posizione e rotazione del subplane
            SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
            lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();
            lastOffsetToSubplane = CalculateLastOffsetToSubplane(transform.position);

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

    public void UpdateTransform(Vector3 newPosition, bool isControlledByAR){
        Debug.Log("Hanno chiamato update transform: " + newPosition);
        //transform.position = newPosition;

        SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
        lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();
        lastOffsetToSubplane = CalculateLastOffsetToSubplane(newPosition);

        controlledByAR = isControlledByAR;
        // Debug.Log("controllato da AR: " + controlledByAR);
        // Debug.LogWarning("offsetTouSub: " + lastOffsetToSubplane);
        // Debug.LogWarning("phone relative: " + newPosition);
        // Debug.LogWarning("renderPos: " + (lastOffsetToSubplane + activePhoneSubplane.transform.position));
        // StampaPosizioneRPC(lastOffsetToSubplane, newPosition);
    }

    public void UpdateRotation(float rotationAngle){
        Debug.Log("Hanno chiamato update rotation: " + rotationAngle);
        transform.rotation *= Quaternion.AngleAxis(rotationAngle, transform.up);

        SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
        lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();
        //lastOffsetToSubplane = CalculateLastOffsetToSubplane();
    }

    public void UpdateRotation(float rotationAngle, Vector3 rotationAxis){
        Debug.Log("Hanno chiamato update rotation: " + rotationAngle);
        transform.rotation *= Quaternion.AngleAxis(rotationAngle, rotationAxis);

        SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
        lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();
        //lastOffsetToSubplane = CalculateLastOffsetToSubplane();

    }

    public void UpdateScale(Vector3 newScale){
        Debug.Log("Hanno chiamato update scale: " + newScale);
        networkedScale = newScale;
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

    public Vector3 CalculateLastOffsetToSubplane(Vector3 nextPosition){
        if(PlatformManager.IsDesktop())
            return Camera.main.transform.InverseTransformPoint(nextPosition);
        if(!activePhoneSubplane)
            return Vector3.zero;

        Vector3 offset = activePhoneSubplane.transform.InverseTransformPoint(nextPosition);
        
        return offset;
    }

    public Quaternion CalculateLastRotationOffsetToSubplane(){
        if(PlatformManager.IsDesktop())
            return Quaternion.Inverse(Camera.main.transform.rotation) * transform.rotation;
        if(!activePhoneSubplane)
            return Quaternion.identity;

        Quaternion rotation = Quaternion.Inverse(activePhoneSubplane.transform.rotation) * transform.rotation;

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
