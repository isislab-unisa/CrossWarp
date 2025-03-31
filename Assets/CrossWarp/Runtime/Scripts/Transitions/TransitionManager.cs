using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class TransitionManager : NetworkBehaviour
{
    MovableObject movableObject;
    [Networked, OnChangedRender(nameof(OnTransitionStateChanged))]
    public TransitionState transitionState {get; set;}
    public Transition transition;

    public override void Spawned()
    {
        movableObject = GetComponent<MovableObject>();
        transition = new Transition(movableObject, this);
    }

    public void StartPushInTransition(){
        Debug.Log("worldState: " + movableObject.worldState);
        if(movableObject.worldState == MovableObjectState.inAR && HasStateAuthority){
            movableObject.worldState = MovableObjectState.TransitioningToVR;
            transitionState = TransitionState.MovingToDisplay;
        }
        //transitionState = TransitionState.MovingToDisplay;
        /*transition.targetPosition = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        StartCoroutine(transition.StartMovingToDisplay());*/
    }


    public void StartPushInTransitionOnScreen(){
        Debug.Log("worldState: " + movableObject.worldState);
        if(movableObject.worldState == MovableObjectState.inAR && HasStateAuthority){
            movableObject.worldState = MovableObjectState.TransitioningToVR;
            transitionState = TransitionState.ARtoVR;
        }
        //transitionState = TransitionState.MovingToDisplay;
        /*transition.targetPosition = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        StartCoroutine(transition.StartMovingToDisplay());*/
    }

    public void StartPullOutTransition(){
        if(movableObject.worldState == MovableObjectState.inVR){
            movableObject.worldState = MovableObjectState.TransitioningToAR;
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
        
        Debug.Log("worldState: " + movableObject.worldState);
        if(movableObject.worldState == MovableObjectState.inVR){
        
            if(!HasStateAuthority)
                await GetComponent<NetworkObject>().WaitForStateAuthority();

            movableObject.worldState = MovableObjectState.TransitioningToAR;
            transitionState = TransitionState.MovingToDisplay;
        }
        /*transition.targetPosition = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        StartCoroutine(transition.StartMovingToDisplay());*/
    }

    public async void OnTransitionStateChanged(){
        Debug.Log("state: " + transitionState);

        if(transitionState == TransitionState.MovingToDisplay && movableObject.HasStateAuthority){
            if(PlatformManager.IsDesktop() && movableObject.worldState == MovableObjectState.TransitioningToAR){
                Debug.Log("passo in AR");
                Transform NCPCenter = Camera.main.transform.GetChild(0);
                StartCoroutine(transition.StartMovingToDisplaySeamless(NCPCenter, NCPCenter.position + NCPCenter.forward * 0.05f));
            }
            else if(!PlatformManager.IsDesktop() && movableObject.worldState == MovableObjectState.TransitioningToVR){
                Debug.Log("passo in VR");
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
            if(movableObject.particleEffects){
            movableObject.particleEffects.transform.parent = transform;
            movableObject.particleEffects.transform.localPosition = movableObject.particleEffectsPrefab.transform.localPosition;
            movableObject.particleEffects.GetComponent<ParticleSystem>().Play();
            movableObject.particleEffects.transform.parent = null;
            }
            else{
                Debug.LogWarning("Missing Particle Effects");
            }
            Vector3 targetPosition = Vector3.zero;

            // se la transizione è da AR a VR allora il desktop deve richiedere la state authority per occuparsi degli spostamenti dell'oggetto
            if(movableObject.worldState == MovableObjectState.TransitioningToVR){
                if(PlatformManager.IsDesktop()){
                    movableObject.StartAssemble();
                    await GetComponent<NetworkObject>().WaitForStateAuthority();
                    Transform target = Camera.main.transform.GetChild(0);
                    targetPosition = target.position + target.forward * 0.25f;
                }
                else{
                    movableObject.StartDissolve();
                    Transform target = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane().transform;
                    targetPosition = target.position + target.forward * 0.25f;
                }
            }
            // se la transizione è da VR ad AR allora il client che ha selezionato l'oggetto deve richiedere la state authority per occuparsi degli spostamenti dell'oggetto
            else if(movableObject.worldState == MovableObjectState.TransitioningToAR){
                if(PlatformManager.IsDesktop()){
                    movableObject.StartDissolve();
                    Transform target = Camera.main.transform.GetChild(0);
                    targetPosition = target.position - target.forward * 0.25f;
                    Debug.Log("target: " + targetPosition);
                }
                
                else{
                    movableObject.StartAssemble();
                    await movableObject.isSelectedBy.RequestStateAuthorityOnSelectedObject();
                    Transform target = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane().transform;
                    targetPosition = target.position - target.forward * 0.25f;
                    Debug.Log("target: " + targetPosition);
                    Debug.Log("HasSAX: " + movableObject.HasStateAuthority);
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
            Debug.Log("HasSA: " + movableObject.HasStateAuthority);
            GetComponent<Outline>().enabled = movableObject.selected;
            // if(movableObject.HasStateAuthority && PlatformManager.IsDesktop()){
            //     //await GetComponent<NetworkObject>().WaitForStateAuthority();
            //     movableObject.worldState = MovableObjectState.inVR;
            // }
            // else if(movableObject.HasStateAuthority && !PlatformManager.IsDesktop()){
            //     //await isSelectedBy.RequestStateAuthorityOnSelectedObject();
            //     movableObject.worldState = MovableObjectState.inAR;
            // }
            Debug.Log("worldstateEnd: " + movableObject.worldState);
        }
    }

}
