using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NearClipPlaneTrigger : MonoBehaviour
{
    public bool triggered = false;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        int layerMask = LayerMask.NameToLayer("MovableObjects");
        Debug.LogError("provo");
        Debug.LogError("layermask: " + layerMask);
        Debug.LogError("layer: " + other.gameObject.layer);
        if(other.gameObject.layer == layerMask && !triggered){
            Debug.LogError("Fatto");
            Debug.LogError("SA: " + other.GetComponent<MovableObject>().HasStateAuthority);
            PhoneRepresentation phoneRepresentation = FindFirstObjectByType<PhoneRepresentation>();
            phoneRepresentation.RequestStateAuthorityOnObjectRpc(other.GetComponent<NetworkObject>().Id);
            Debug.LogError("Cambio SA Avvenuto");
            Debug.LogError("SA: " + other.GetComponent<MovableObject>().HasStateAuthority);
            Debug.LogError("lastpos: " + other.GetComponent<MovableObject>().lastOffsetToSubplane);
            //other.GetComponent<MovableObject>().worldState = MovableObjectState.inAR;
            //triggered = true;
        }
    }
}
