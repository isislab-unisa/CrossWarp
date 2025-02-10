using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public enum TransitionState {
    Created,
    MovingToDisplay,
    ARtoVR,
    VRtoAR,
    MovingFromDisplay,
    Ended
}

public class Transition
{
    public MovableObject applyTo;
    public float transitionDuration = 2.5f;
    public float particleDuration = 2f;
    public Vector3 targetPosition;

    public Transition(MovableObject applyTo){
        this.applyTo = applyTo;
    }

    public IEnumerator StartMovingToDisplay(){
        Vector3 startPos = applyTo.transform.position;
        float timer = 0;
        Debug.LogError("SP: " + startPos);
        Debug.LogError("TP: " + targetPosition);
        while(startPos != targetPosition && timer < transitionDuration) 
        {
            float progress = timer / transitionDuration;
            applyTo.UpdatePosition(Vector3.Lerp(startPos, targetPosition, progress));
            
            timer += Time.deltaTime;
            yield return null;
        }

        if(applyTo.worldState == MovableObjectState.TransitioningToAR)
            applyTo.transitionState = TransitionState.VRtoAR;
        else if(applyTo.worldState == MovableObjectState.TransitioningToVR)
            applyTo.transitionState = TransitionState.ARtoVR;
    }

    public IEnumerator StartARToVR(bool inDesktop, Vector3 target){
        //applyTo.transitionState = TransitionState.ARtoVR;
        Debug.Log("particelle");
        ParticleSystem particleSystem = applyTo.particleEffects.GetComponent<ParticleSystem>();
        particleSystem.Play();
        if(inDesktop){
            applyTo.transform.position = target;
            Debug.LogWarning("posizione: " + applyTo.transform.position);
            applyTo.StartAssemble();
            //applyTo.SetShowing(true);
        }
        else{
            applyTo.StartDissolve();
        }
        
        yield return new WaitForSeconds(particleSystem.main.duration);
        if(inDesktop)
            applyTo.UpdatePosition(target);
        
        applyTo.transitionState = TransitionState.MovingFromDisplay;

    }

    public IEnumerator StartVRToAR(bool inDesktop, Vector3 target){
        //applyTo.transitionState = TransitionState.ARtoVR;
        Debug.Log("particelle");
        ParticleSystem particleSystem = applyTo.particleEffects.GetComponent<ParticleSystem>();
        particleSystem.Play();
        if(inDesktop){
            applyTo.StartDissolve();
        }
        else{
            applyTo.transform.position = target;
            Debug.LogWarning("posizione: " + applyTo.transform.position);
            applyTo.StartAssemble();
        }
        
        yield return new WaitForSeconds(particleSystem.main.duration);
        if(!inDesktop && applyTo.HasStateAuthority){
            applyTo.UpdatePosition(target);
            Debug.LogWarning("update position: " + target);
        }
        
        applyTo.transitionState = TransitionState.MovingFromDisplay;

    }

    public IEnumerator StartMovingFromDisplay(bool inDesktop){
        //applyTo.transitionState = TransitionState.MovingFromDisplay;

        /*applyTo.transform.position = targetPosition.transform.position;
        applyTo.Assemble();
        Debug.LogWarning("UpdateTRansform after assemble");
        applyTo.UpdateTransform(applyTo.transform.position);*/
        yield return null;

        applyTo.transitionState = TransitionState.Ended;

    }

    public IEnumerator StartTransitionFromDisplayToVR(Vector3 targetPosition){
        Vector3 startPos = applyTo.transform.position;
        float timer = 0;
        applyTo.transform.position = targetPosition;
        applyTo.Assemble();
        Debug.LogWarning("UpdateTRansform after assemble");
        applyTo.UpdatePosition(applyTo.transform.position);

        /*while(timer < transitionDuration) 
        {
            float progress = timer / transitionDuration;
            applyTo.UpdateTransform(Vector3.Lerp(startPos, targetPosition, progress));
            
            timer += Time.deltaTime;
            yield return null;
        }*/
        yield return null;
    }

    public IEnumerator StartMovingToDisplaySeamless(Transform targetTransform, Vector3 targetPosition){
        applyTo.transitionState = TransitionState.MovingToDisplay;
        Vector3 startPos = applyTo.transform.position;
        float timer = 0;
        Debug.LogError("SP: " + startPos);
        Debug.LogError("TP: " + targetPosition);
        while(startPos != targetPosition && timer < transitionDuration) 
        {
            float progress = timer / transitionDuration;
            applyTo.UpdatePosition(Vector3.Lerp(startPos, targetPosition, progress));
            
            timer += Time.deltaTime;
            yield return null;
        }

        if(applyTo.worldState == MovableObjectState.TransitioningToAR)
            applyTo.transitionState = TransitionState.VRtoAR;
        else if(applyTo.worldState == MovableObjectState.TransitioningToVR)
            applyTo.transitionState = TransitionState.ARtoVR;
    }

    public IEnumerator StartARToVRSeamless(bool inDesktop, Vector3 target){
        Debug.Log("particelle");
        ParticleSystem particleSystem = applyTo.particleEffects.GetComponent<ParticleSystem>();
        particleSystem.Play();
        if(inDesktop){
            applyTo.StartAssemble();
        }
        else{
            applyTo.StartDissolve();
        }
        applyTo.transitionState = TransitionState.MovingFromDisplay;
        yield return null;

    }

    public IEnumerator StartVRToARSeamless(bool inDesktop, Vector3 target){
        Debug.Log("particelle");
        ParticleSystem particleSystem = applyTo.particleEffects.GetComponent<ParticleSystem>();
        particleSystem.Play();
        if(inDesktop){
            applyTo.StartDissolve();
        }
        else{
            applyTo.StartAssemble();
        }

        applyTo.transitionState = TransitionState.MovingFromDisplay;
        yield return null;

    }

    public IEnumerator StartMovingFromDisplaySeamless(Vector3 targetPosition){

        Vector3 startPos = applyTo.transform.position;
        float timer = 0;
        Debug.LogError("SP: " + startPos);
        Debug.LogError("TP: " + targetPosition);
        while(startPos != targetPosition && timer < transitionDuration) 
        {
            float progress = timer / transitionDuration;
            if(applyTo.HasStateAuthority)
                applyTo.UpdatePosition(Vector3.Lerp(startPos, targetPosition, progress));
            
            timer += Time.deltaTime;
            yield return null;
        }

        applyTo.transitionState = TransitionState.Ended;
        if(applyTo.HasStateAuthority && PlatformManager.IsDesktop()){
            //await GetComponent<NetworkObject>().WaitForStateAuthority();
            applyTo.worldState = MovableObjectState.inVR;
        }
        else if(applyTo.HasStateAuthority && !PlatformManager.IsDesktop()){
            //await isSelectedBy.RequestStateAuthorityOnSelectedObject();
            applyTo.worldState = MovableObjectState.inAR;
        }

    }

}
