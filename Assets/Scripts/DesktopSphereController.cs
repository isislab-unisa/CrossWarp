using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class DesktopSphereController : NetworkBehaviour
{

    public GameObject hitObjectPrefab;
    public GameObject phoneRepresentationPrefab;
    private GameObject selectedObject;
    private Dictionary<PlayerRef, GameObject> playersRepresentation;

    void Start()
    {
        playersRepresentation = new Dictionary<PlayerRef, GameObject>();
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
    public void UpdatePositionRpc(Vector3 newPosition, Quaternion newRotation, bool isMirror, PlayerRef callingPlayer){
        GameObject phoneRepresentation;
        if(playersRepresentation.ContainsKey(callingPlayer)){
            phoneRepresentation = playersRepresentation[callingPlayer];
        }
        else{
            phoneRepresentation = Runner.Spawn(phoneRepresentationPrefab, Vector3.zero, Quaternion.identity).gameObject;
            playersRepresentation.Add(callingPlayer, phoneRepresentation);
            Color randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            phoneRepresentation.GetComponent<PhoneRepresentation>().interactionColor = randomColor;
        }

        Debug.Log("BCZ chiamata update position");
        Debug.Log("isMirror: " + isMirror);
        float z = newPosition.z;
        if(isMirror){
            z = -z;
            newRotation = Quaternion.Euler(-newRotation.eulerAngles.x, -newRotation.eulerAngles.y, newRotation.eulerAngles.z);
        }
        z += Camera.main.nearClipPlane;
        phoneRepresentation.transform.rotation = newRotation;
        phoneRepresentation.transform.position = new Vector3(newPosition.x, newPosition.y, z);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void SendRemotePointRpc(Vector3 point, Vector3 direction, bool isMirror, PlayerRef callingPlayer){
        Vector3 pointFromCamera = Camera.main.ViewportToWorldPoint(new Vector3(point.x, point.y, Camera.main.nearClipPlane));

        /*Debug.Log("BCZ ricevuto punto, x: " + point.x);
        Debug.Log("BCZ ricevuto punto, y: " + point.y);
        Vector3 invertedPoint = new Vector3(point.x, point.y, Camera.main.nearClipPlane);
        Ray ray = Camera.main.ViewportPointToRay(invertedPoint);
        Debug.DrawLine(ray.origin, ray.direction*5000, Color.red, 50);
        if(!isMirror)
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
                    //Instantiate(hitObjectPrefab, hit.point, Quaternion.identity);
                    Runner.Spawn(hitObjectPrefab, hit.point, Quaternion.identity);
                else
                    selectedObject.transform.position = hit.point;
            }
        }*/
        if(playersRepresentation.ContainsKey(callingPlayer)){
            playersRepresentation[callingPlayer].GetComponent<PhoneRepresentation>().SendRemotePoint(point, direction, isMirror);
        }
        else{
            Debug.LogError("Non c'è il playerRepresentation corrispondente");
        }
    }
}
