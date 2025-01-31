using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition
{
    public MovableObject applyTo;
    public float transitionDuration = 5f;
    public float particleDuration = 2f;
    public GameObject targetPosition;

    public Transition(MovableObject applyTo){
        this.applyTo = applyTo;
    }

    public IEnumerator StartTransition(){
        Vector3 startPos = applyTo.transform.position;
        float timer = 0;

        while(timer < transitionDuration) 
        {
            float progress = timer / transitionDuration;
            applyTo.UpdateTransform(Vector3.Lerp(startPos, targetPosition.transform.position, progress));
            
            timer += Time.deltaTime;
            yield return null;
        }

        /*timer = 0;
        while(timer < particleDuration) 
        {*/
            Debug.Log("particelle");
            ParticleSystem particleSystem = applyTo.particleEffects.GetComponent<ParticleSystem>();
            particleSystem.Play();
            
            yield return new WaitForSeconds(particleSystem.main.duration);
        //}

        applyTo.StartTransitionFromDisplayToVRRPC();

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

    public IEnumerator StartTransitionFromDisplayToVR(Vector3 targetPosition){
        Vector3 startPos = applyTo.transform.position;
        float timer = 0;

        while(timer < transitionDuration) 
        {
            float progress = timer / transitionDuration;
            applyTo.UpdateTransform(Vector3.Lerp(startPos, targetPosition, progress));
            
            timer += Time.deltaTime;
            yield return null;
        }
    }

}
