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
            applyTo.UpdateTransform(Vector3.Lerp(startPos, targetPosition, progress));
            
            timer += Time.deltaTime;
            yield return null;
        }

        if(applyTo.worldState == MovableObjectState.TransitioningToAR)
            applyTo.transitionState = TransitionState.VRtoAR;
        else if(applyTo.worldState == MovableObjectState.TransitioningToVR)
            applyTo.transitionState = TransitionState.ARtoVR;

        /*timer = 0;
        while(timer < particleDuration) 
        {*/
            
        //}

        //yield return StartARToVR();

        
        //yield return StartMovingFromDisplay();

        //applyTo.transitionState = TransitionState.Ended;

        //applyTo.StartTransitionFromDisplayToVRRPC();

        /*timer = 0;
        startPos = applyTo.transform.position;
        while(timer < transitionDuration) 
        {
            float progress = timer / transitionDuration;
            applyTo.UpdateTransform(Vector3.Lerp(startPos, targetPosition.transform.forward * 2, progress));
            applyTo.worldState = MovableObjectState.inVR;
            
            timer += Time.deltaTime;
            yield return null;
        }*/
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
            applyTo.UpdateTransform(target);
        
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
            applyTo.UpdateTransform(target);
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
        applyTo.UpdateTransform(applyTo.transform.position);

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
            applyTo.UpdateTransform(Vector3.Lerp(startPos, targetPosition, progress));
            
            timer += Time.deltaTime;
            yield return null;
        }

        if(applyTo.worldState == MovableObjectState.TransitioningToAR)
            applyTo.transitionState = TransitionState.VRtoAR;
        else if(applyTo.worldState == MovableObjectState.TransitioningToVR)
            applyTo.transitionState = TransitionState.ARtoVR;
        
        /*timer = 0;
        transitionDuration = 0.5f;
        float velocity = 0.1f;
        while(timer < transitionDuration && applyTo.transitionState != TransitionState.MovingFromDisplay && applyTo.HasStateAuthority){
            if(applyTo.worldState == MovableObjectState.TransitioningToAR){
                applyTo.UpdateTransform(Vector3.Lerp(applyTo.transform.position, applyTo.transform.position + targetTransform.forward * velocity, 0.5f));
            }
            else if(applyTo.worldState == MovableObjectState.TransitioningToVR){
                applyTo.UpdateTransform(Vector3.Lerp(applyTo.transform.position, applyTo.transform.position - targetTransform.forward * velocity, 0.5f));
            }
            timer += Time.deltaTime;
            yield return null;
        }*/
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
        // Vector3 startPos = applyTo.transform.position;
        // float timer = 0;
        // while(!PlatformManager.IsDesktop() && applyTo.HasStateAuthority){
        //     float progress = timer / transitionDuration;
        //     if(progress > 1)
        //         progress = 1;
        //     applyTo.UpdateTransform(Vector3.Lerp(startPos, target, progress));
            
        //     timer += Time.deltaTime;
        //     yield return null;
        // }
        
        // while(PlatformManager.IsDesktop() && !applyTo.HasStateAuthority)
        //     yield return null;

        // if(PlatformManager.IsDesktop())
        //     applyTo.transitionState = TransitionState.MovingFromDisplay;

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

        // Vector3 startPos = applyTo.transform.position;
        // float timer = 0;
        // while(PlatformManager.IsDesktop() && applyTo.HasStateAuthority){
        //     float progress = timer / transitionDuration;
        //     if(progress > 1)
        //         progress = 1;
        //     applyTo.UpdateTransform(Vector3.Lerp(startPos, target, progress));
            
        //     timer += Time.deltaTime;
        //     yield return null;
        // }
        
        // while(!PlatformManager.IsDesktop() && !applyTo.HasStateAuthority)
        //     yield return null;
            
        // if(!PlatformManager.IsDesktop())
        //     applyTo.transitionState = TransitionState.MovingFromDisplay;

    }

    public IEnumerator StartMovingFromDisplaySeamless(Vector3 targetPosition){
        //applyTo.transitionState = TransitionState.MovingFromDisplay;

        /*applyTo.transform.position = targetPosition.transform.position;
        applyTo.Assemble();
        Debug.LogWarning("UpdateTRansform after assemble");
        applyTo.UpdateTransform(applyTo.transform.position);*/

        Vector3 startPos = applyTo.transform.position;
        float timer = 0;
        Debug.LogError("SP: " + startPos);
        Debug.LogError("TP: " + targetPosition);
        while(startPos != targetPosition && timer < transitionDuration) 
        {
            float progress = timer / transitionDuration;
            if(applyTo.HasStateAuthority)
                applyTo.UpdateTransform(Vector3.Lerp(startPos, targetPosition, progress));
            
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
