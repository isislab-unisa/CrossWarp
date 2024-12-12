using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class DesktopSphereController : NetworkBehaviour
{

    public GameObject hitObjectPrefab;
    public GameObject phoneRapresentationPrefab;
    private GameObject selectedObject;
    private Dictionary<PlayerRef, GameObject> playersRapresentation;

    void Start()
    {
        playersRapresentation = new Dictionary<PlayerRef, GameObject>();
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
        GameObject phoneRapresentation;
        if(playersRapresentation.ContainsKey(callingPlayer)){
            phoneRapresentation = playersRapresentation[callingPlayer];
        }
        else{
            phoneRapresentation = Instantiate(phoneRapresentationPrefab, Vector3.zero, Quaternion.identity);
            playersRapresentation.Add(callingPlayer, phoneRapresentation);
        }

        Debug.Log("BCZ chiamata update position");
        Debug.Log("isMirror: " + isMirror);
        float z = newPosition.z;
        if(isMirror){
            z = -z;
            newRotation = Quaternion.Euler(-newRotation.eulerAngles.x, -newRotation.eulerAngles.y, newRotation.eulerAngles.z);
        }
        z += Camera.main.nearClipPlane;
        phoneRapresentation.transform.rotation = newRotation;
        phoneRapresentation.transform.position = new Vector3(newPosition.x, newPosition.y, z);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void SendRemotePointRpc(Vector3 point, Vector3 direction, bool isMirror){
        Vector3 pointFromCamera = Camera.main.ViewportToWorldPoint(new Vector3(point.x, point.y, Camera.main.nearClipPlane));

        Debug.Log("BCZ ricevuto punto, x: " + point.x);
        Debug.Log("BCZ ricevuto punto, y: " + point.y);
        Vector3 invertedPoint = new Vector3(point.x, point.y, Camera.main.nearClipPlane);
        Ray ray = Camera.main.ViewportPointToRay(invertedPoint);
        Debug.DrawLine(ray.origin, ray.direction*5000, Color.red, 50);
        if(!isMirror)
            Debug.DrawLine(transform.position, direction, Color.blue, 50);
        if (Physics.Raycast(ray, out RaycastHit hit)){
            if(hit.collider.tag.Equals("MovableObject")){
                if(hit.collider.gameObject == selectedObject){
                    Debug.Log("Colpito oggetto selezionato, deseleziono: " + selectedObject.name);
                    selectedObject.GetComponent<Outline>().enabled = false; 
                    Debug.Log("Colpito oggetto selezionato, deseleziono 2");
                    selectedObject = null;
                }
                else if(selectedObject != null){
                    // deselect old selectedObjecr
                    selectedObject.GetComponent<Outline>().enabled = false;
                    Debug.Log("Colpito oggetto non selezionato mentre ho altro selezionato, lo seleziono: " + selectedObject.name);
                    // select new object
                    selectedObject = hit.collider.gameObject;
                    Debug.Log("Colpito oggetto non selezionato mentre ho altro selezionato, lo seleziono 2");
                    selectedObject.GetComponent<Outline>().enabled = true;
                    Debug.Log("Colpito oggetto non selezionato mentre ho altro selezionato, lo seleziono 3");
                }
                else{
                    Debug.Log("Colpito oggetto non selezionato, lo seleziono");
                    selectedObject = hit.collider.gameObject;
                    Debug.Log("Colpito oggetto non selezionato, lo seleziono 2 : " + selectedObject.name);
                    selectedObject.GetComponent<Outline>().enabled = true;
                    Debug.Log("Colpito oggetto non selezionato, lo seleziono 3");
                }
            }
            else{
                Debug.Log("non ho colpito nessun oggetto selezionato");
                if(selectedObject == null)
                    //Instantiate(hitObjectPrefab, hit.point, Quaternion.identity);
                    Runner.Spawn(hitObjectPrefab, hit.point, Quaternion.identity);
                else
                    selectedObject.transform.position = hit.point;
            }
        }
    }
}
