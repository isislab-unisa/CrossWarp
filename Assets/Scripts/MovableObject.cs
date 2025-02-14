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

        particleEffects = Instantiate(particleEffectsPrefab, transform);
        //particleEffects.GetComponent<ParticleSystem>().Play();
        StartAssemble();
        float seedForNoise = Random.Range(1, 100);
        //meshRenderer.material.SetFloat("_SeedForRandomNoise", seedForNoise);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers){
            renderer.material.SetFloat("_SeedForRandomNoise", seedForNoise);
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
        /*meshRenderer.enabled = showing;
        collider.enabled = showing;
        if(showing && (worldState == MovableObjectState.inAR || worldState == MovableObjectState.inVR))
            meshRenderer.material.SetFloat("_DissolveAmount", 0);*/
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

            // una volta che viene selezionato da qualcun altro cambio l'offset in posizione e rotazione rispetto al subplane
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

    public void UpdatePosition(Vector3 newPosition){
        Debug.Log("Hanno chiamato update transform: " + newPosition);

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

    public void StartPushInTransitionOnScreen(){
        Debug.LogError("worldState: " + worldState);
        if(worldState == MovableObjectState.inAR && HasStateAuthority){
            worldState = MovableObjectState.TransitioningToVR;
            transitionState = TransitionState.ARtoVR;
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
        
        Debug.LogError("worldState: " + worldState);
        if(worldState == MovableObjectState.inVR){
        
            if(!HasStateAuthority)
                await GetComponent<NetworkObject>().WaitForStateAuthority();

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
        if(PlatformManager.IsDesktop()){
            Transform NCPCenter = Camera.main.transform.GetChild(0);
            return NCPCenter.InverseTransformPoint(nextPosition);
        }
        if(!activePhoneSubplane)
            return Vector3.zero;

        Vector3 offset = activePhoneSubplane.transform.InverseTransformPoint(nextPosition);
        
        return offset;
    }

    public Quaternion CalculateLastRotationOffsetToSubplane(){
        if(PlatformManager.IsDesktop()){
            Transform NCPCenter = Camera.main.transform.GetChild(0);
            return Quaternion.Inverse(NCPCenter.rotation) * transform.rotation;
        }
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
        // if(transitionState == TransitionState.MovingToDisplay && HasStateAuthority){
        //     if(PlatformManager.IsDesktop() && worldState == MovableObjectState.TransitioningToAR){
        //         Debug.LogError("passo in AR");
        //         Transform cam = Camera.main.transform; 
        //         transition.targetPosition = cam.position + cam.forward * (Camera.main.nearClipPlane + 0.25f);
        //         StartCoroutine(transition.StartMovingToDisplay());
        //     }
        //     else if(!PlatformManager.IsDesktop() && worldState == MovableObjectState.TransitioningToVR){
        //         Debug.LogError("passo in VR");
        //         Transform subplane = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane().transform;
        //         transition.targetPosition = subplane.position + subplane.forward * 0.25f;
        //         StartCoroutine(transition.StartMovingToDisplay());
        //     }
        // }
        // else if(transitionState == TransitionState.ARtoVR){
        //     GetComponent<Outline>().enabled = false;
        //     if(PlatformManager.IsDesktop()){
        //         if(!GetComponent<NetworkObject>().HasStateAuthority)
        //                 await GetComponent<NetworkObject>().WaitForStateAuthority();
        //         Transform cam = Camera.main.transform;
        //         StartCoroutine(transition.StartARToVR(true, cam.position + cam.forward * (Camera.main.nearClipPlane + 0.25f)));
        //     }
        //     else{
        //         StartCoroutine(transition.StartARToVR(false, Vector3.zero));
        //     }
        // }
        // else if(transitionState == TransitionState.VRtoAR){
        //     GetComponent<Outline>().enabled = false;
        //     if(PlatformManager.IsDesktop()){
        //         StartCoroutine(transition.StartVRToAR(true, Vector3.zero));
        //     }
        //     else{
        //         await isSelectedBy.RequestStateAuthorityOnSelectedObject();
        //         Debug.LogWarning("hasSA: " + HasStateAuthority);
        //         GameObject subplane = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        //         StartCoroutine(transition.StartVRToAR(false, subplane.transform.position - subplane.transform.forward * 0.25f ));
        //     }
        // }
        // else if(transitionState == TransitionState.MovingFromDisplay){
        //     GetComponent<Outline>().enabled = selected;
        //     if(PlatformManager.IsDesktop()){
        //         StartCoroutine(transition.StartMovingFromDisplay(true));
        //     }
        //     else{
        //         //SetShowing(false);
        //         StartCoroutine(transition.StartMovingFromDisplay(false));
        //     }
        // }
        // else if(transitionState == TransitionState.Ended){
        //     Debug.LogWarning("HasSA: " + HasStateAuthority);
        //     if(HasStateAuthority && PlatformManager.IsDesktop())
        //         worldState = MovableObjectState.inVR;
        //     else if(HasStateAuthority && !PlatformManager.IsDesktop())
        //         worldState = MovableObjectState.inAR;
        // }

        if(transitionState == TransitionState.MovingToDisplay && HasStateAuthority){
            if(PlatformManager.IsDesktop() && worldState == MovableObjectState.TransitioningToAR){
                Debug.LogError("passo in AR");
                Transform NCPCenter = Camera.main.transform.GetChild(0);
                StartCoroutine(transition.StartMovingToDisplaySeamless(NCPCenter, NCPCenter.position + NCPCenter.forward * 0.05f));
            }
            else if(!PlatformManager.IsDesktop() && worldState == MovableObjectState.TransitioningToVR){
                Debug.LogError("passo in VR");
                Transform subplane = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane().transform;
                StartCoroutine(transition.StartMovingToDisplaySeamless(subplane, subplane.position - subplane.forward * 0.05f));
            }
        }
        else if(transitionState == TransitionState.ARtoVR){
            GetComponent<Outline>().enabled = false;
            transitionState = TransitionState.MovingFromDisplay;
        }
        else if(transitionState == TransitionState.VRtoAR){
            GetComponent<Outline>().enabled = false;
            
            transitionState = TransitionState.MovingFromDisplay;
        }
        else if(transitionState == TransitionState.MovingFromDisplay){
            // effetti particellari
            particleEffects.transform.parent = transform;
            particleEffects.transform.localPosition = particleEffectsPrefab.transform.localPosition;
            particleEffects.GetComponent<ParticleSystem>().Play();
            particleEffects.transform.parent = null;
            Vector3 targetPosition = Vector3.zero;

            // se la transizione è da AR a VR allora il desktop deve richiedere la state authority per occuparsi degli spostamenti dell'oggetto
            if(worldState == MovableObjectState.TransitioningToVR){
                if(PlatformManager.IsDesktop()){
                    StartAssemble();
                    await GetComponent<NetworkObject>().WaitForStateAuthority();
                    Transform target = Camera.main.transform.GetChild(0);
                    targetPosition = target.position + target.forward * 0.25f;
                }
                else{
                    StartDissolve();
                    Transform target = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane().transform;
                    targetPosition = target.position + target.forward * 0.25f;
                }
            }
            // se la transizione è da VR ad AR allora il client che ha selezionato l'oggetto deve richiedere la state authority per occuparsi degli spostamenti dell'oggetto
            else if(worldState == MovableObjectState.TransitioningToAR){
                if(PlatformManager.IsDesktop()){
                    StartDissolve();
                    Transform target = Camera.main.transform.GetChild(0);
                    targetPosition = target.position - target.forward * 0.25f;
                    Debug.LogError("target: " + targetPosition);
                }
                else{
                    StartAssemble();
                    await isSelectedBy.RequestStateAuthorityOnSelectedObject();
                    Transform target = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane().transform;
                    targetPosition = target.position - target.forward * 0.25f;
                    Debug.LogError("target: " + targetPosition);
                    Debug.LogError("HasSAX: " + HasStateAuthority);
                }
            }

            // che sia desktop o meno tutti i client chiamano la fase finale della transizione
            if(PlatformManager.IsDesktop()){
                //Transform target = Camera.main.transform.GetChild(0);
                StartCoroutine(transition.StartMovingFromDisplaySeamless(targetPosition));
            }
            else if(!PlatformManager.IsDesktop()){
                //Transform target = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane().transform;
                StartCoroutine(transition.StartMovingFromDisplaySeamless(targetPosition));
            }
        }
        else if(transitionState == TransitionState.Ended){
            Debug.LogWarning("HasSA: " + HasStateAuthority);
            GetComponent<Outline>().enabled = selected;
            // if(HasStateAuthority && PlatformManager.IsDesktop()){
            //     //await GetComponent<NetworkObject>().WaitForStateAuthority();
            //     worldState = MovableObjectState.inVR;
            // }
            // else if(HasStateAuthority && !PlatformManager.IsDesktop()){
            //     //await isSelectedBy.RequestStateAuthorityOnSelectedObject();
            //     worldState = MovableObjectState.inAR;
            // }
            Debug.LogError("worldstateEnd: " + worldState);
        }
    }

}
