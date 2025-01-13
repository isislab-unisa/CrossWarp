using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class SubplaneConfig : MonoBehaviour
{
    public GameObject ancorPrefab;
    public GameObject unpositionedAncorPrefab;
    public GameObject subplanePrefab;
    public ARPlacementInteractable placementInteractable;
    public GameObject StartButton;
    public GameObject EndButton;
    public GameObject MoveOnPlaneToggle;

    // si usano i piani per posizionare gli anchor?
    public bool usePlanes = false;
    public bool isMirror = false;
    private bool isConfig = false;
    private bool canConfig = true;
    private List<GameObject> anchors = new List<GameObject>();
    private List<GameObject> createdSubplanes = new List<GameObject>();


    public void StartConfig(){
        if(canConfig){
            Debug.Log("BCZ start subplane config");
            anchors = new List<GameObject>();
            isConfig = true;
            if(usePlanes)
                placementInteractable.enabled = true;
        }
    }

    // when a plane is created the config is stopped
    public void StopConfig(){
        isConfig = false;
        placementInteractable.enabled = false;
    }

    public bool IsConfig(){
        return isConfig;
    }

    public bool IsEndedConfig(){
        return canConfig;
    }

    // when we don't want to edit the planes anymore, config ends
    public void EndConfig(){
        Debug.Log("BCZ Chiamata end config");
        StopConfig();
        canConfig = false;
        HideAllSubplanes();
        HideUI();
        Debug.Log("Disabilito PlaneManagers");
        ARPlaneManager aRPlaneManager = FindObjectOfType<XROrigin>().GetComponent<ARPlaneManager>();
        aRPlaneManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
        //aRPlaneManager.enabled = false;

        foreach (var plane in aRPlaneManager.trackables)
        {
            if(plane.alignment == PlaneAlignment.Vertical)
                plane.gameObject.SetActive(false);
        }
    }


    // when we want to edit the config after a config end
    public void EditConfig(){
        canConfig = true;
        ShowAllSubplanes();
        ShowUI();
        Debug.Log("Abilito PlaneManagers");
        ARPlaneManager aRPlaneManager = FindObjectOfType<XROrigin>().GetComponent<ARPlaneManager>();
        aRPlaneManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Vertical | UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
        //aRPlaneManager.enabled = true;

        foreach (var plane in aRPlaneManager.trackables)
        {
            plane.gameObject.SetActive(true);
        }
    }

    public void HideAllSubplanes(){
        foreach(GameObject subplane in createdSubplanes){
            subplane.GetComponent<Subplane>().HideSubplane();
        }
    }

    public void ShowAllSubplanes(){
        foreach(GameObject subplane in createdSubplanes){
            subplane.GetComponent<Subplane>().ShowSubplane();
        }
    }

    private void HideUI(){
        StartButton.SetActive(false);
        EndButton.SetActive(false);
        MoveOnPlaneToggle.SetActive(false);
    }

    private void ShowUI(){
        StartButton.SetActive(true);
        EndButton.SetActive(true);
        MoveOnPlaneToggle.SetActive(true);
    }

    public void OnAnchorPlaced(ARObjectPlacementEventArgs args){
        anchors.Add(args.placementObject);
    }

    public void UsePlanes(bool isUsingPlane){
        usePlanes = isUsingPlane;
        Debug.Log("UsePlanes: " + usePlanes);
        if(usePlanes){
            if(isConfig)
                placementInteractable.enabled = true;
        }
        else{
            placementInteractable.enabled = false;
        }
    }

    public GameObject GetSelectedSubplane(){
        if(createdSubplanes.Count <= 0)
            return null;
        return createdSubplanes[0];
    }

    public void Update(){
        /*if(isConfig){
            Debug.Log("BC is config è true");
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        /*var anchor = new GameObject("Anchor");
                        anchor.transform.position = hit.point;
                        
                        var arAnchor = anchor.AddComponent<ARAnchor>();
                        arAnchor.transform.position = hit.point;*/
                        /*
                        Debug.Log("BCZ Aggiungo un anchor");

                        GameObject anchor = Instantiate(ancorPrefab, hit.point, Quaternion.identity);
                        Debug.Log("BCZ anchor aggiunto");
                        anchors.Add(anchor);

                    }
                }
            }
        }*/
        if(isConfig && !usePlanes && Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            Debug.Log("Touch rilevato");
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchPointWorldPosition = Camera.main.ScreenToWorldPoint(touch.position);
                Vector3 creationPoint = new Vector3(touchPointWorldPosition.x, touchPointWorldPosition.y, touchPointWorldPosition.z) + Camera.main.transform.forward*0.2f;  
                GameObject anchor = Instantiate(ancorPrefab, creationPoint, Quaternion.identity);
                var placementAnchor = new GameObject("PlacementAnchor").transform;
                placementAnchor.position = creationPoint;
                placementAnchor.rotation = Quaternion.identity;
                anchor.transform.parent = placementAnchor;
                Debug.Log("anchor creato a : " + creationPoint);
                anchors.Add(anchor);
            }
        }
        if(anchors.Count == 3 && isConfig == true){
            StopConfig();
            Debug.Log("BCZ chiamo createsubplane");
            CreateSubplane();
        }
    }

    private void CreateSubplane(){
        GameObject subplane = Instantiate(subplanePrefab, anchors[0].transform.position, Quaternion.identity);
        createdSubplanes.Add(subplane);
        subplane.GetComponent<Subplane>().SetAnchors(anchors);
    }

}
