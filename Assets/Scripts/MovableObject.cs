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
    [Networked, OnChangedRender(nameof(OnTransitionStateChanged))]
    public TransitionState transitionState {get; set;}


    public Transition transition;

    
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
            lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();
            lastOffsetToSubplane = CalculateLastOffsetToSubplane(transform.position);
            networkedScale = transform.localScale;
        }
        if((GetComponent<NetworkObject>().Flags & NetworkObjectFlags.AllowStateAuthorityOverride) > 0)
            Debug.LogWarning("AllowStateAuthOverride attivato");
        transition = new Transition(this);

        particleEffects = Instantiate(particleEffectsPrefab, meshRenderer.transform);
        particleEffects.transform.parent = meshRenderer.transform;
        //particleEffects.GetComponent<ParticleSystem>().Play();
        StartAssemble();
        float seedForNoise = Random.Range(1, 100);
        meshRenderer.material.SetFloat("_SeedForRandomNoise", seedForNoise);
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
        meshRenderer.material.SetFloat("_DissolveAmount", 1);
        SetShowing(true);
        while(time < duration){
            progress = 1 - time/duration;
            meshRenderer.material.SetFloat("_DissolveAmount", progress);

            time += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator Dissolve(){
        float time = 0;
        float duration = 1f;
        float progress = 0;
        meshRenderer.material.SetFloat("_DissolveAmount", 0);
        while(time < duration){
            progress = time/duration;
            meshRenderer.material.SetFloat("_DissolveAmount", progress);

            time += Time.deltaTime;
            yield return null;
        }
    }

    void Update()
    {
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

        if(transitionState != TransitionState.ARtoVR && transitionState != TransitionState.VRtoAR){
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
    }

    public void SetShowing(bool showing){
        meshRenderer.enabled = showing;
        collider.enabled = showing;
        meshRenderer.material.SetFloat("_DissolveAmount", 0);
    }

    private void UpdateWorldState(){
        if(worldState == MovableObjectState.TransitioningToAR || worldState == MovableObjectState.TransitioningToVR)
            return;
        if(HasStateAuthority){
            if(PlatformManager.IsDesktop())
                worldState = MovableObjectState.inVR;
            else
                worldState = MovableObjectState.inAR;
        }
        Debug.LogWarning("worldState: " + worldState);
    }

    public async Task<bool> TrySelectObject(PhoneRepresentation playerSelecting){
        if(isSelectedBy == null){
            if(!HasStateAuthority){
                await GetComponent<NetworkObject>().WaitForStateAuthority();
            }
            UpdateWorldState();

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

    public void UpdateTransform(Vector3 newPosition){
        Debug.Log("Hanno chiamato update transform: " + newPosition);
        //transform.position = newPosition;

        SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if(subplaneConfig)
                activePhoneSubplane = subplaneConfig.GetSelectedSubplane();
            else
                activePhoneSubplane = null;
        lastRotationOffsetToSubplane = CalculateLastRotationOffsetToSubplane();
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

    public void StartPushInTransition(){
        Debug.LogError("worldState: " + worldState);
        if(worldState == MovableObjectState.inAR && HasStateAuthority){
            worldState = MovableObjectState.TransitioningToVR;
            transitionState = TransitionState.MovingToDisplay;
        }
        //transitionState = TransitionState.MovingToDisplay;
        /*transition.targetPosition = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        StartCoroutine(transition.StartMovingToDisplay());*/
    }

    public void StartPullOutTransition(){
        if(worldState == MovableObjectState.inVR){
            worldState = MovableObjectState.TransitioningToAR;
            transitionState = TransitionState.MovingToDisplay;
        }
        /*transition.targetPosition = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        StartCoroutine(transition.StartMovingToDisplay());*/
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public async void StartPullOutTransitionRPC(){
        if(!PlatformManager.IsDesktop())
            return;

        // può succedere che l'oggetto sia in VR ma il desktop non abbia state authority (succede se l'utente mobile fa resize o rotazione)
        if(!HasStateAuthority)
            await GetComponent<NetworkObject>().WaitForStateAuthority();

        Debug.LogError("worldState: " + worldState);
        if(worldState == MovableObjectState.inVR){
            worldState = MovableObjectState.TransitioningToAR;
            transitionState = TransitionState.MovingToDisplay;
        }
        /*transition.targetPosition = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        StartCoroutine(transition.StartMovingToDisplay());*/
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void StartTransitionFromDisplayToVRRPC(){
        worldState = MovableObjectState.TransitioningToVR;
        SetShowing(true);
        if(!PlatformManager.IsDesktop()){
            StartCoroutine(Dissolve());
        }
        else{
            if(!HasStateAuthority){
                await GetComponent<NetworkObject>().WaitForStateAuthority();
            }
            Transition transition = new Transition(this);
            StartCoroutine(transition.StartTransitionFromDisplayToVR(Camera.main.transform.position + Camera.main.transform.forward * 0.5f));
        }
        /*Debug.LogWarning("STARTTRansition");
        if(!HasStateAuthority){
            await GetComponent<NetworkObject>().WaitForStateAuthority();
        }
        UpdateWorldState();*/
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

    public void OnSelectionColorChanged(){
        GetComponent<Outline>().OutlineColor = selectionColor;
    }

    public void OnSelectedChanged(){
        GetComponent<Outline>().enabled = selected;
        
        Debug.LogWarning("offset: " + lastOffsetToSubplane);
        if(!PlatformManager.IsDesktop()){
            GameObject localSubplane = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
            Debug.LogWarning("pos: " + localSubplane.transform.TransformPoint(lastOffsetToSubplane));
        }
        else{
            
            Debug.LogWarning("pos: " + Camera.main.transform.TransformPoint(lastOffsetToSubplane));
        }
            
    }

    
    public async void OnTransitionStateChanged(){
        Debug.LogWarning("state: " + transitionState);
        if(transitionState == TransitionState.MovingToDisplay && HasStateAuthority){
            if(PlatformManager.IsDesktop() && worldState == MovableObjectState.TransitioningToAR){
                Debug.LogError("passo in AR");
                Transform cam = Camera.main.transform; 
                transition.targetPosition = cam.position + cam.forward * (Camera.main.nearClipPlane + 0.25f);
                StartCoroutine(transition.StartMovingToDisplay());
            }
            else if(!PlatformManager.IsDesktop() && worldState == MovableObjectState.TransitioningToVR){
                Debug.LogError("passo in VR");
                transition.targetPosition = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane().transform.position;
                StartCoroutine(transition.StartMovingToDisplay());
            }
        }
        else if(transitionState == TransitionState.ARtoVR){
            GetComponent<Outline>().enabled = false;
            if(PlatformManager.IsDesktop()){
                if(!GetComponent<NetworkObject>().HasStateAuthority)
                        await GetComponent<NetworkObject>().WaitForStateAuthority();
                Transform cam = Camera.main.transform;
                StartCoroutine(transition.StartARToVR(true, cam.position + cam.forward * (Camera.main.nearClipPlane + 0.25f)));
            }
            else{
                StartCoroutine(transition.StartARToVR(false, Vector3.zero));
            }
        }
        else if(transitionState == TransitionState.VRtoAR){
            GetComponent<Outline>().enabled = false;
            if(PlatformManager.IsDesktop()){
                StartCoroutine(transition.StartVRToAR(true, Vector3.zero));
            }
            else{
                await isSelectedBy.RequestStateAuthorityOnSelectedObject();
                Debug.LogWarning("hasSA: " + HasStateAuthority);
                GameObject subplane = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
                StartCoroutine(transition.StartVRToAR(false, subplane.transform.position - subplane.transform.forward * 0.15f ));
            }
        }
        else if(transitionState == TransitionState.MovingFromDisplay){
            GetComponent<Outline>().enabled = selected;
            if(PlatformManager.IsDesktop()){
                StartCoroutine(transition.StartMovingFromDisplay(true));
            }
            else{
                //SetShowing(false);
                StartCoroutine(transition.StartMovingFromDisplay(false));
            }
        }
        else if(transitionState == TransitionState.Ended){
            Debug.LogWarning("HasSA: " + HasStateAuthority);
            if(HasStateAuthority && PlatformManager.IsDesktop())
                worldState = MovableObjectState.inVR;
            else if(HasStateAuthority && !PlatformManager.IsDesktop())
                worldState = MovableObjectState.inAR;
        }
    }

}
