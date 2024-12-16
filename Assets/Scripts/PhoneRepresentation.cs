using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PhoneRepresentation : NetworkBehaviour
{
    public GameObject hitObjectPrefab;
    public Color interactionColor {get; set;}
    private GameObject selectedObject = null;

    public void SendRemotePoint(Vector3 point, Vector3 direction, bool isMirror){
        Debug.Log("BCZ ricevuto punto, x: " + point.x);
        Debug.Log("BCZ ricevuto punto, y: " + point.y);
        Vector3 invertedPoint = new Vector3(point.x, point.y, Camera.main.nearClipPlane);
        Ray ray = Camera.main.ViewportPointToRay(invertedPoint);
        Debug.DrawLine(ray.origin, ray.direction*5000, interactionColor, 50);
        if(!isMirror)
            Debug.DrawLine(transform.position, direction, interactionColor, 50);
        if (Physics.Raycast(ray, out RaycastHit hit)){
            if(hit.collider.tag.Equals("MovableObject")){
                if(hit.collider.gameObject == selectedObject){
                    selectedObject.GetComponent<MovableObject>().ReleaseSelection();
                    selectedObject = null;
                }
                else if(selectedObject != null){
                    // deselect old selectedObjecr
                    selectedObject.GetComponent<MovableObject>().ReleaseSelection();
                    // select new object
                    /*selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;*/
                    if(hit.collider.gameObject.GetComponent<MovableObject>().TrySelectObject(this))
                        selectedObject = hit.collider.gameObject;
                }
                else{
                    /*selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;*/
                    if(hit.collider.gameObject.GetComponent<MovableObject>().TrySelectObject(this))
                        selectedObject = hit.collider.gameObject;
                }
            }
            else{
                if(selectedObject == null)
                    //Instantiate(hitObjectPrefab, hit.point, Quaternion.identity);
                    Runner.Spawn(hitObjectPrefab, hit.point, Quaternion.identity);
                else
                    selectedObject.transform.position = hit.point;
            }
        }
    }
}
