using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class DesktopSphereController : NetworkBehaviour
{

    public GameObject hitObjectPrefab;
    private GameObject selectedObject;

    void Start()
    {
        // screen height in cm
        float screenHeight = (Screen.height/Screen.dpi)*2.54f;
        // frustum height in m
        float frustumHeight = screenHeight/100;
        float distance = frustumHeight * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Camera.main.nearClipPlane = distance;
        transform.position = Camera.main.transform.position + new Vector3(0, 0, distance + 1);
        Debug.Log("BCZ frustum distance: " + distance);
    }

    void Update()
    {
        
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void UpdatePositionRpc(Vector3 newPosition){
        Debug.Log("BCZ chiamata update position");
        transform.position = new Vector3(newPosition.x, newPosition.y, Camera.main.nearClipPlane - newPosition.z);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void SendRemotePointRpc(Vector3 point, Vector3 direction){
        Vector3 pointFromCamera = Camera.main.ViewportToWorldPoint(new Vector3(point.y, point.x, Camera.main.nearClipPlane));//new Vector3(point.x, point.y, Camera.main.transform.position.z);

        Debug.Log("BCZ ricevuto punto, x: " + point.x);
        Debug.Log("BCZ ricevuto punto, y: " + point.y);
        Vector3 invertedPoint = new Vector3(point.y, point.x, Camera.main.nearClipPlane);
        Ray ray = Camera.main.ViewportPointToRay(invertedPoint);//new Ray(pointFromCamera, direction*5000);//Camera.main.ScreenPointToRay(pointFromCamera);
        Debug.DrawLine(ray.origin, ray.direction*5000, Color.red, 50);
        Debug.DrawLine(transform.position, direction, Color.blue, 50);
        if (Physics.Raycast(ray, out RaycastHit hit)){
            if(hit.collider.tag.Equals("MovableObject")){
                if(hit.collider.gameObject == selectedObject){
                    selectedObject.GetComponent<Outline>().enabled = false;
                    selectedObject = null;
                }
                else if(selectedObject != null){
                    // deselect old selectedObjecr
                    selectedObject.GetComponent<Outline>().enabled = false;
                    // select new object
                    selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;
                }
                else{
                    selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Outline>().enabled = true;
                }
            }
            else{
                if(selectedObject == null)
                    Instantiate(hitObjectPrefab, hit.point, Quaternion.identity);
                else
                    selectedObject.transform.position = hit.point;
            }
            //hit.transform.gameObject.GetComponent<Renderer>().material.color = Color.red;

        }
    }
}
