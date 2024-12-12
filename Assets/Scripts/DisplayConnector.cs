using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DisplayConnector : NetworkBehaviour
{
    public ARSphereController aRSphereController;
    
    void Start()
    {

    }

    void Update()
    {
        if (HasStateAuthority && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5f);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 direction = ray.direction;
                    Debug.Log("BCZ colpisco, " + hit.transform.gameObject.name);
                    Subplane hittedSubplane = hit.transform.gameObject.GetComponent<Subplane>();
                    if(hittedSubplane){
                        Debug.Log("Hit display global: " + hit.point);
                        Debug.Log("Hit display local: " + hit.transform.InverseTransformPoint(hit.point));
                        Vector3 cameraMappedPoint = hittedSubplane.NormalizedHitPoint(hit.transform.InverseTransformPoint(hit.point));
                        if(aRSphereController){
                            Debug.Log("BCZ ID: " + aRSphereController.Id);
                            aRSphereController.SendPointToDesktop(cameraMappedPoint, direction);
                        }
                    }
                }
            }
        }
    }
}
