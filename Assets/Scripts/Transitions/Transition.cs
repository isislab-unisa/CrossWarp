using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
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
    public float transitionDuration = 5f;
    public float particleDuration = 2f;
    public Vector3 targetPosition;

    public Transition(MovableObject applyTo){
        this.applyTo = applyTo;
    }

    public IEnumerator StartMovingToDisplay(){
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

}
