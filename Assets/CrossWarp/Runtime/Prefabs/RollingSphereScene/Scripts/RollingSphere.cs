using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RollingSphere : NetworkBehaviour
{
    public GameObject objectShadow;
    [Networked]
    public Vector3 networkedVelocity {get; set;}
    [Networked]
    public Vector3 networkedAngularVelocity {get; set;}
    public Rigidbody rigidbody;


    public override void Spawned()
    {
        networkedVelocity = Vector3.zero;
        networkedAngularVelocity = Vector3.zero;
        rigidbody = objectShadow.GetComponent<Rigidbody>();
        //Destroy(this, 10);
    }

    void FixedUpdate()
    {
        if(!HasStateAuthority){
            
            if(PlatformManager.IsDesktop()){
                Transform NCPCenter = Camera.main.transform.GetChild(0);
                rigidbody.velocity = NCPCenter.TransformPoint(networkedVelocity);
                rigidbody.angularVelocity = NCPCenter.TransformPoint(networkedVelocity);
            }
            else{
                SubplaneConfig subplaneConfig = FindObjectOfType<SubplaneConfig>();
                if(subplaneConfig){
                    GameObject localSubplane = subplaneConfig.GetSelectedSubplane();
                    if(localSubplane){
                        rigidbody.velocity = localSubplane.transform.TransformPoint(networkedVelocity);
                        rigidbody.angularVelocity = localSubplane.transform.TransformPoint(networkedAngularVelocity);
                    }
                }
            }
        }
        //objectShadow.GetComponentInChildren<Renderer>().enabled = GetComponentInChildren<Renderer>().enabled;
    }

    public override void FixedUpdateNetwork(){
        if(HasStateAuthority){
            networkedVelocity = CalculateVectorRelativeToSubplane(objectShadow.GetComponent<Rigidbody>().velocity);
            networkedAngularVelocity = CalculateVectorRelativeToSubplane(objectShadow.GetComponent<Rigidbody>().angularVelocity);
        }
        else{
            objectShadow.GetComponent<Rigidbody>().velocity = networkedVelocity;
            objectShadow.GetComponent<Rigidbody>().angularVelocity = networkedAngularVelocity;
        }
    }

    public Vector3 CalculateVectorRelativeToSubplane(Vector3 vec){
        if(PlatformManager.IsDesktop()){
            Transform NCPCenter = Camera.main.transform.GetChild(0);
            return NCPCenter.InverseTransformPoint(vec);
        }
        GameObject activeSubplane = FindObjectOfType<SubplaneConfig>().GetSelectedSubplane();
        if(!activeSubplane)
            return Vector3.zero;

        Vector3 offset = activeSubplane.transform.InverseTransformPoint(vec);
        
        return offset;
    }
}
