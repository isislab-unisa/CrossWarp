using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;

public enum MovableObjectState {
        inAR,
        inVR,
        TransitioningToAR,
        TransitioningToVR
    }

public class MovableObject : NetworkBehaviour
{
    private bool isSpawned = false;
    [Networked]
    public PhoneRepresentation isSelectedBy {get; set;}
    [Networked, OnChangedRender(nameof(OnSelectionColorChanged))]
    public Color selectionColor {get; set;}
    [Networked, OnChangedRender(nameof(OnSelectedChanged))]
    public bool selected {get; set;}
    [Networked]
    public MovableObjectState worldState {get; set;}
    public Renderer meshRenderer;
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
    public GameObject particleEffectsPrefab;
    public GameObject particleEffects;
    public TransitionManager transitionManager;
    public GameObject ObjectShadow;

    
    public override void Spawned(){
        UpdateWorldState();
        if(HasStateAuthority){
            if(!PlatformManager.IsDesktop()){
                SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
                if(subplaneConfig)
                    activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
                else
                    activePhoneSubplane = null;
            }
            lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane(transform.rotation);
            lastOffsetToSubplane = CalculateLastOffsetToSubplane(transform.position);
            networkedScale = transform.localScale;
        }
        if((GetComponent<NetworkObject>().Flags & NetworkObjectFlags.AllowStateAuthorityOverride) > 0)
            Debug.LogWarning("AllowStateAuthOverride attivato");
        transitionManager = GetComponent<TransitionManager>();


        particleEffects = Instantiate(particleEffectsPrefab, transform);
        //particleEffects.GetComponent<ParticleSystem>().Play();
        StartAssemble();
        float seedForNoise = Random.Range(1, 100);
        //meshRenderer.material.SetFloat("_SeedForRandomNoise", seedForNoise);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers){
            renderer.material.SetFloat("_SeedForRandomNoise", seedForNoise);
        }
        if(ObjectShadow){
            ObjectShadow.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ObjectShadow.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            Physics.IgnoreCollision(collider, ObjectShadow.GetComponent<Collider>());
        }

        isSpawned = true;
    }

    public void StartAssemble(){
        meshRenderer.material.SetFloat("_DissolveAmount", 1);
        StartCoroutine(Assemble());
    }

    public void StartDissolve(){
        meshRenderer.material.SetFloat("_DissolveAmount", 0);
        StartCoroutine(Dissolve());
    }

    public IEnumerator Assemble(){
        float time = 0;
        float duration = 1f;
        float progress = 0;
        //meshRenderer.material.SetFloat("_DissolveAmount", 1);
        
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers){
            renderer.material.SetFloat("_DissolveAmount", 1);
        }
        SetShowing(true);
        while(time < duration){
            progress = 1 - time/duration;
            //meshRenderer.material.SetFloat("_DissolveAmount", progress);
            foreach(Renderer renderer in renderers){
                renderer.material.SetFloat("_DissolveAmount", progress);
            }

            time += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator Dissolve(){
        float time = 0;
        float duration = 1f;
        float progress = 0;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers){
            renderer.material.SetFloat("_DissolveAmount", 0);
        }
        //meshRenderer.material.SetFloat("_DissolveAmount", 0);
        while(time < duration){
            progress = time/duration;
            //meshRenderer.material.SetFloat("_DissolveAmount", progress);
            foreach(Renderer renderer in renderers){
                renderer.material.SetFloat("_DissolveAmount", progress);
            }

            time += Time.deltaTime;
            yield return null;
        }
    }

    void FixedUpdate()
    {
        if(!isSpawned)
            return;

        if(PlatformManager.IsDesktop() && worldState == MovableObjectState.inVR){
            SetShowing(true);
        }
        else if(PlatformManager.IsDesktop() && worldState == MovableObjectState.inAR){
            SetShowing(false);
        }
        else if(!PlatformManager.IsDesktop() && worldState == MovableObjectState.inAR){
            SetShowing(true);
        }
        else if(!PlatformManager.IsDesktop() && worldState == MovableObjectState.inVR){
            SetShowing(false);
        }

        if(ObjectShadow){
            if(selected){
                Rigidbody rigidbody = ObjectShadow.GetComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                ObjectShadow.transform.position = transform.position;
                ObjectShadow.transform.rotation = transform.rotation;
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
            else if(HasStateAuthority){
                ObjectShadow.GetComponent<Rigidbody>().isKinematic = false;
                lastOffsetToSubplane = CalculateLastOffsetToSubplane(ObjectShadow.transform.position);
                lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane(ObjectShadow.transform.rotation);
            }
            else if(!HasStateAuthority){
                Rigidbody rigidbody = ObjectShadow.GetComponent<Rigidbody>();
                rigidbody.isKinematic = false;
                ObjectShadow.transform.position = transform.position;
                ObjectShadow.transform.rotation = transform.rotation;
            }
        }

        //if(transitionState != TransitionState.ARtoVR && transitionState != TransitionState.VRtoAR){
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
                Transform NCPCenter = Camera.main.transform.GetChild(0);
                transform.position = Vector3.Lerp(transform.position, NCPCenter.TransformPoint(lastOffsetToSubplane), 0.5f);
                transform.rotation = Quaternion.Lerp(transform.rotation, NCPCenter.rotation * lastRotationOffsetToSubplane, 0.5f);
            }

            if(networkedScale != null && transform.localScale != networkedScale){
                transform.localScale = Vector3.Lerp(transform.localScale, networkedScale, 0.5f);
            }
        //}
    }

    public void SetShowing(bool showing){
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers){
            renderer.enabled = showing;
            if(showing && (worldState == MovableObjectState.inAR || worldState == MovableObjectState.inVR))
                renderer.material.SetFloat("_DissolveAmount", 0);
        }
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach(Collider collider in colliders){
            collider.enabled = showing;
        }
    }

    public void UpdateWorldState(){
        if(worldState == MovableObjectState.TransitioningToAR || worldState == MovableObjectState.TransitioningToVR)
            return;
        if(HasStateAuthority){
            if(PlatformManager.IsDesktop())
                worldState = MovableObjectState.inVR;
            else
                worldState = MovableObjectState.inAR;
        }
        Debug.Log("worldState: " + worldState);
    }

    public async Task<bool> TrySelectObject(PhoneRepresentation playerSelecting){
        if(isSelectedBy == null){
            if(!HasStateAuthority){
                await GetComponent<NetworkObject>().WaitForStateAuthority();
            }
            UpdateWorldState();

            // una volta che viene selezionato da qualcun altro cambio l'offset in posizione e rotazione rispetto al subplane
            SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
            lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane(transform.rotation);
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

    public void UpdatePosition(Vector3 newPosition){
        Debug.Log("Hanno chiamato update transform: " + newPosition);

        SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
        lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane(transform.rotation);
        lastOffsetToSubplane = CalculateLastOffsetToSubplane(newPosition);

        UpdateWorldState();
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
        lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane(transform.rotation);
    }

    public void UpdateRotation(float rotationAngle, Vector3 rotationAxis){
        Debug.Log("Hanno chiamato update rotation: " + rotationAngle);
        transform.rotation *= Quaternion.AngleAxis(rotationAngle, rotationAxis);

        SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
        lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane(transform.rotation);

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
        Debug.Log("offsetTouSub: " + lastOffsetToSubplane);
        Debug.Log("phone relative: " + phonePosition);
        if(localPhoneSubplane)
            Debug.Log("renderPosRPC: " + (lastOffsetToSubplane + localPhoneSubplane.transform.position));
    }

    public Vector3 CalculateLastOffsetToSubplane(Vector3 nextPosition){
        if(PlatformManager.IsDesktop()){
            Transform NCPCenter = Camera.main.transform.GetChild(0);
            return NCPCenter.InverseTransformPoint(nextPosition);
        }
        if(!activePhoneSubplane)
            activePhoneSubplane = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        if(!activePhoneSubplane)
            return Vector3.zero;

        Vector3 offset = activePhoneSubplane.transform.InverseTransformPoint(nextPosition);
        
        return offset;
    }

    public Quaternion CalculateLastRotationOffsetToSubplane(Quaternion nextRotation){
        if(PlatformManager.IsDesktop()){
            Transform NCPCenter = Camera.main.transform.GetChild(0);
            return Quaternion.Inverse(NCPCenter.rotation) * nextRotation;
        }
        if(!activePhoneSubplane)
            activePhoneSubplane = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        if(!activePhoneSubplane)
            return Quaternion.identity;

        Quaternion rotation = Quaternion.Inverse(activePhoneSubplane.transform.rotation) * nextRotation;

        return rotation;
    }

    public void OnSelectionColorChanged(){
        GetComponent<Outline>().OutlineColor = selectionColor;
    }

    public void OnSelectedChanged(){
        GetComponent<Outline>().enabled = selected;
        
        Debug.Log("offset: " + lastOffsetToSubplane);
        if(!PlatformManager.IsDesktop()){
            GameObject localSubplane = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
            Debug.Log("pos: " + localSubplane.transform.TransformPoint(lastOffsetToSubplane));
        }
        else{
            
            Debug.Log("pos: " + Camera.main.transform.TransformPoint(lastOffsetToSubplane));
        }
            
    }

}
