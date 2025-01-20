using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class SubplaneConfig : MonoBehaviour
{
    public enum ConfigurationMode
    {
        ImageTracking,
        OnPlane,
        InSpace
    }

    public ImageTrackingManager imageTrackingManager;
    public GameObject ancorPrefab;
    public GameObject unpositionedAncorPrefab;
    public GameObject subplanePrefab;
    public ARPlacementInteractable placementInteractable;
    public GameObject StartButton;
    public GameObject StopButton;
    public TMP_Dropdown dropdown;
    public GameObject MoveOnPlaneToggle;
    public ConfigurationMode configurationMode = ConfigurationMode.ImageTracking;
    public ConfigurationMode lastConfigurationMode = ConfigurationMode.OnPlane;

    // si usano i piani per posizionare gli anchor?
    public bool usePlanes = false;
    public bool isMirror = false;
    private bool isConfig = false;
    private bool canConfig = true;
    private List<GameObject> anchors = new List<GameObject>();
    private List<GameObject> createdSubplanes = new List<GameObject>();



    public void StartConfig(){
        /*if(!isConfig){
            ARSphereController aRSphereController = FindObjectOfType<ARSphereController>();
            DesktopSphereController desktopSphereController = aRSphereController.GetReferenceToDesktopObject();
            if(desktopSphereController)
                desktopSphereController.ToggleConfiguringRpc(true);
        }
        Debug.Log("BCZ start subplane config");
        anchors = new List<GameObject>();*/
        isConfig = true;
        OnConfigurationModeChanged();
        if(configurationMode == ConfigurationMode.ImageTracking){
            ARSphereController aRSphereController = FindObjectOfType<ARSphereController>();
            DesktopSphereController desktopSphereController = aRSphereController.GetReferenceToDesktopObject();
            if(desktopSphereController)
                desktopSphereController.ToggleConfiguringRpc(true);
        }
        ShowAllSubplanes();
        /*if(usePlanes)
            placementInteractable.enabled = true;*/
        StartButton.SetActive(false);
        StopButton.SetActive(true);
        MoveOnPlaneToggle.SetActive(true);
        dropdown.enabled = false;
    }

    // when a plane is created the config is stopped
    public void StopConfig(){
        isConfig = false;
        placementInteractable.enabled = false;
        if(configurationMode == ConfigurationMode.ImageTracking){
            ARSphereController aRSphereController = FindObjectOfType<ARSphereController>();
            DesktopSphereController desktopSphereController = aRSphereController.GetReferenceToDesktopObject();
            if(desktopSphereController)
                desktopSphereController.ToggleConfiguringRpc(false);
        }

        HideAllSubplanes();
        Debug.Log("Disabilito PlaneManagers");
        ARPlaneManager aRPlaneManager = FindObjectOfType<XROrigin>().GetComponent<ARPlaneManager>();
        aRPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        //aRPlaneManager.enabled = false;

        foreach (var plane in aRPlaneManager.trackables)
        {
            if(plane.alignment == PlaneAlignment.Vertical)
                plane.gameObject.SetActive(false);
        }

        StopButton.SetActive(false);
        StartButton.SetActive(true);
        MoveOnPlaneToggle.SetActive(false);
        dropdown.enabled = true;
    }

    public void OnConfigModeDpdChanged(){
        if(dropdown.options[dropdown.value].text == "Image Tracking"){
            configurationMode = ConfigurationMode.ImageTracking;
            Debug.LogWarning("Porco demonio: " + configurationMode);
        }
        else if(dropdown.options[dropdown.value].text == "On Plane"){
            configurationMode = ConfigurationMode.OnPlane;
            Debug.LogWarning("Porco demonio: " + configurationMode);
        }
        else if(dropdown.options[dropdown.value].text == "In Space"){
            configurationMode = ConfigurationMode.InSpace;
            Debug.LogWarning("Porco demonio: " + configurationMode);
        }
        //OnConfigurationModeChanged();
    }

    public bool IsConfig(){
        return isConfig;
    }

    public bool IsEndedConfig(){
        return canConfig;
    }

    // when we don't want to edit the planes anymore, config ends
    public void EndConfig(){
        if(canConfig){
            ARSphereController aRSphereController = FindObjectOfType<ARSphereController>();
            DesktopSphereController desktopSphereController = aRSphereController.GetReferenceToDesktopObject();
            if(desktopSphereController)
                desktopSphereController.ToggleConfiguringRpc(false);
        }
        Debug.Log("BCZ Chiamata end config");
        StopConfig();
        canConfig = false;
        HideAllSubplanes();
        HideUI();
        Debug.Log("Disabilito PlaneManagers");
        ARPlaneManager aRPlaneManager = FindObjectOfType<XROrigin>().GetComponent<ARPlaneManager>();
        aRPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        //aRPlaneManager.enabled = false;

        foreach (var plane in aRPlaneManager.trackables)
        {
            if(plane.alignment == PlaneAlignment.Vertical)
                plane.gameObject.SetActive(false);
        }
    }


    // when we want to edit the config after a config end
    public void EditConfig(){
        if(!canConfig){
            ARSphereController aRSphereController = FindObjectOfType<ARSphereController>();
            DesktopSphereController desktopSphereController = aRSphereController.GetReferenceToDesktopObject();
            if(desktopSphereController)
                desktopSphereController.ToggleConfiguringRpc(true);
        }
        canConfig = true;
        ShowAllSubplanes();
        ShowUI();
        Debug.Log("Abilito PlaneManagers");
        ARPlaneManager aRPlaneManager = FindObjectOfType<XROrigin>().GetComponent<ARPlaneManager>();
        aRPlaneManager.requestedDetectionMode = PlaneDetectionMode.Vertical | PlaneDetectionMode.Horizontal;
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
        StopButton.SetActive(false);
        MoveOnPlaneToggle.SetActive(false);
    }

    private void ShowUI(){
        StartButton.SetActive(true);
        StopButton.SetActive(true);
        MoveOnPlaneToggle.SetActive(true);
    }

    public void OnAnchorPlaced(ARObjectPlacementEventArgs args){
        anchors.Add(args.placementObject);
    }

    public void OnTrackedAnchorPlaced(GameObject trackedAnchor){
        anchors.Add(trackedAnchor);
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

    public void OnConfigurationModeChanged(){
        if(configurationMode == ConfigurationMode.ImageTracking){
            //se non esiste il subplane cambiamo la configurazione e ricominciamo da zero cancellando tutte le anchor che erano state create
            if(!GetSelectedSubplane()){
                foreach(GameObject anchor in anchors){
                    Destroy(anchor);
                }
                anchors = new List<GameObject>();
                imageTrackingManager.ResetImageTrackingConfiguration();
            }
            // se esiste andiamo a modificare le anchor che esistono già, qui l'imagetrackingmanager deve mappare le anchor che esistono già sulle immagini da rilevare
            else{
                // TODO imagetrackingmanager.SetActiveSubplane()
                imageTrackingManager.ActivateTrackingConfiguration();
            }
        }
        else if(configurationMode == ConfigurationMode.InSpace){
            imageTrackingManager.DisableTrackingConfiguration();
            // a prescindere da se stiamo configurando il subplane, il placementInteractable deve essere disattivo
            placementInteractable.enabled = false;
        }
        else{
            imageTrackingManager.DisableTrackingConfiguration();
            // se stiamo configurando il placementInteractable deve essere attivo quando ancora non ci sono 3 anchor posizionate
            if(isConfig && anchors.Count < 3)
                placementInteractable.enabled = true;
            // altrimenti disattivo, verrà attivato quando si inizia la configurazione
            else
                placementInteractable.enabled = false;
            Debug.Log("Abilito PlaneManagers");
            ARPlaneManager aRPlaneManager = FindObjectOfType<XROrigin>().GetComponent<ARPlaneManager>();
            aRPlaneManager.requestedDetectionMode = PlaneDetectionMode.Vertical | PlaneDetectionMode.Horizontal;
            //aRPlaneManager.enabled = true;

            foreach (var plane in aRPlaneManager.trackables)
            {
                plane.gameObject.SetActive(true);
            }
        }
    }

    public GameObject GetSelectedSubplane(){
        if(createdSubplanes.Count <= 0)
            return null;
        return createdSubplanes[0];
    }

    public void Update(){
        // se ci sono 3 anchor e il subplane non esiste lo creiamo, altrimenti lo possiamo modificare
        if(anchors.Count == 3 && createdSubplanes.Count == 0){
            //StopConfig();
            Debug.Log("BCZ chiamo createsubplane");
            CreateSubplane();
        }

        // se stiamo configurando ,non ci sono piani creati e stiamo configurando nello spazio allora creiamo anchor
        if(isConfig && createdSubplanes.Count == 0 && configurationMode == ConfigurationMode.InSpace && Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            Debug.Log("Touch rilevato");
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchPointWorldPosition = Camera.main.ScreenToWorldPoint(touch.position);
                Vector3 creationPoint = new Vector3(touchPointWorldPosition.x, touchPointWorldPosition.y, touchPointWorldPosition.z) + Camera.main.transform.forward*0.2f;  
                CreateSubplaneAnchor(creationPoint);
            }
        }
    }

    public GameObject CreateSubplaneAnchor(Vector3 position){
        GameObject anchor = Instantiate(ancorPrefab, position, Quaternion.identity);
        var placementAnchor = new GameObject("PlacementAnchor").transform;
        placementAnchor.position = position;
        placementAnchor.rotation = Quaternion.identity;
        anchor.transform.parent = placementAnchor;
        Debug.Log("anchor creato a : " + position);
        anchors.Add(anchor);
        return anchor;
    }

    private void CreateSubplane(){
        OrderAnchors();
        GameObject subplane = Instantiate(subplanePrefab, anchors[0].transform.position, Quaternion.identity);
        createdSubplanes.Add(subplane);
        subplane.GetComponent<Subplane>().SetAnchors(anchors);
    }

    private void OrderAnchors(){
        GameObject temp;
        for(int i = 1; i<anchors.Count; i++){
            if(anchors[0].transform.position.y < anchors[i].transform.position.y){
                temp = anchors[0];
                anchors[0] = anchors[i];
                anchors[i] = temp;
            }
            /*else{
                if(anchors[0].transform.position.x > anchors[i].transform.position.x){
                    temp = anchors[0];
                    anchors[0] = anchors[i];
                    anchors[i] = temp;
                }
            }*/
        }
        if(anchors[1].transform.position.x > anchors[2].transform.position.x){
            temp = anchors[1];
            anchors[1] = anchors[2];
            anchors[2] = temp;
        }
    }

}
