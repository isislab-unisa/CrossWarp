using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class SubplaneConfig : MonoBehaviour
{
    public GameObject ancorPrefab;
    public GameObject unpositionedAncorPrefab;
    public GameObject subplanePrefab;
    public ARPlacementInteractable placementInteractable;
    private bool isConfig = false;
    private bool canConfig = true;
    private List<GameObject> anchors = new List<GameObject>();
    private List<GameObject> createdSubplanes = new List<GameObject>();

    public void StartConfig(){
        if(canConfig){
            Debug.Log("BCZ start subplane config");
            anchors = new List<GameObject>();
            isConfig = true;
            placementInteractable.enabled = true;
        }
    }

    public void StopConfig(){
        isConfig = false;
        placementInteractable.enabled = false;
    }

    public void EndConfig(){
        Debug.Log("BCZ Chiamata end config");
        StopConfig();
        canConfig = false;
        HideAllAnchors();
        Debug.Log("Disabilito PlaneManagers");
        ARPlaneManager aRPlaneManager = FindObjectOfType<XROrigin>().GetComponent<ARPlaneManager>();
        aRPlaneManager.enabled = false;

        foreach (var plane in aRPlaneManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
    }

    public void HideAllAnchors(){
        foreach(GameObject subplane in createdSubplanes){
            subplane.GetComponent<Subplane>().HideSubplane();
        }
    }

    public void OnAnchorPlaced(ARObjectPlacementEventArgs args){
        anchors.Add(args.placementObject);
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
        if(isConfig && Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            Debug.Log("Touch rilevato");
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchPointWorldPosition = Camera.main.ScreenToWorldPoint(touch.position);
                Vector3 creationPoint = new Vector3(touchPointWorldPosition.x, touchPointWorldPosition.y, touchPointWorldPosition.z) + Camera.main.transform.forward*0.2f;  
                GameObject anchor = Instantiate(unpositionedAncorPrefab, creationPoint, Quaternion.identity);
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
        /*Vector3 point1 = anchors[0].transform.position;
        Vector3 point2 = anchors[1].transform.position;
        Vector3 point3 = anchors[2].transform.position;

        // Proietta point2 sulla stessa X di point1 (se desideri un rettangolo verticale)
        point2 = new Vector3(point1.x, point2.y, point1.z);

        // Ora calcola point3 sulla stessa Y di point2 (o Z se in 3D)
        point3 = new Vector3(point3.x, point2.y, point3.z);

        // calcolo il primo vettore diff di A1 e A2
        Vector3 vHeight = point1 - point2;

        // calcolo il secondo vettore diff di A2 e A3
        Vector3 vWidth = point3 - point2;

        Vector3 normal = Vector3.Cross(vHeight, vWidth).normalized;

        Vector3 right = vHeight.normalized;
        Vector3 up = normal;
        Vector3 forward = Vector3.Cross(right, up);

        // Crea una rotazione basata su forward e up
        Quaternion rotation = Quaternion.LookRotation(forward, up);

        Vector3 center = point2 + (vHeight / 2) + (vWidth / 2);
        Instantiate(ancorPrefab, center, rotation);

        Debug.Log("BCZ Instanzio il subplane");
        GameObject subplane = Instantiate(subplanePrefab, center, rotation);
        Debug.Log("BCZ subplane instanziato");

        // resize del piano
        subplane.transform.localScale = new Vector3(vHeight.magnitude, 0.01f, vWidth.magnitude);
        Debug.Log("BCZ subplane localscale: " + subplane.transform.localScale);*/
        GameObject subplane = Instantiate(subplanePrefab, anchors[0].transform.position, Quaternion.identity);
        createdSubplanes.Add(subplane);
        subplane.GetComponent<Subplane>().SetAnchors(anchors);
    }

}
