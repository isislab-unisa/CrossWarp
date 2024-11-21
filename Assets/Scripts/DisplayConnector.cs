using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DisplayConnector : MonoBehaviour
{
    public GameObject pointPrefab;
    public ARRaycastManager raycastManager;
    public ARSphereController aRSphereController;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*if(!aRSphereController || aRSphereController.Id.Object == null){
            aRSphereController = FindAnyObjectByType<ARSphereController>();
            ARSphereController[] capiamo = FindObjectsOfType<ARSphereController>();
            Debug.Log("Capiamo.count: " + capiamo.Length);
            Debug.Log("ArSphereController: " + aRSphereController);
            foreach(ARSphereController x in capiamo){
                Debug.Log("BCZ ID: " + x.Id);
            }
        }*/
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5f);
                //List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
                if (Physics.Raycast(ray, out RaycastHit hit))
                //if(raycastManager.Raycast(touch.position, hitResults))
                {
                    Vector3 direction = ray.direction;//hit.point - Camera.main.transform.position;
                    Debug.Log("BCZ colpisco, " + hit.transform.gameObject.name);
                    Subplane hittedSubplane = hit.transform.gameObject.GetComponent<Subplane>();
                    if(hittedSubplane){
                        /*Debug.Log("creo il punto");
                        GameObject point = Instantiate(pointPrefab, hit.point, Quaternion.identity, hit.transform);*/
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

    /*private void GetSubplaneHits(List<ARRaycastHit> raycastHits){
        for(int i = 0; i<raycastHits.Count; i++){
            if(raycastHits[i].)
        }
    }*/
}
