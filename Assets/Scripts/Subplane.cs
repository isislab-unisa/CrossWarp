using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public class Subplane : MonoBehaviour
{
    public GameObject anchorPrefab;
    private List<GameObject> anchors = new List<GameObject>();
    private GameObject center;

    // forse non serve più
    private bool isVisible = true;
    void Start()
    {
        
    }


    void Update()
    {
        /*if(anchors.Count == 3 && isVisible){
            //Debug.Log("Renderizzo subplane: " + anchors[0]);
            // da migliorare in modo che renderizza solo quando cambiano i punti
            RenderSubplane();
        }*/
    }

    public void SetAnchors(List<GameObject> selectedAnchors){
        anchors = selectedAnchors;
        foreach(GameObject anchor in anchors){
            /*Vector3 worldPosition = anchor.transform.position;
            Vector3 worldScale = anchor.transform.localScale;
            Quaternion worldRotation = anchor.transform.rotation;
            anchor.transform.parent = transform;
            anchor.transform.position = worldPosition;
            anchor.transform.localScale = worldScale;
            anchor.transform.rotation = worldRotation;*/
            //anchor.transform.SetParent(transform, true);
            anchor.GetComponent<SubplaneAnchor>().SetSubplane(transform.gameObject);
        }
        RenderSubplane();
    }

    public void OnAnchorMoved(){
        RenderSubplane();
    }

    private void RenderSubplane(){
        Vector3 point1 = anchors[0].transform.position;
        Vector3 point2 = anchors[1].transform.position;
        Vector3 point3 = anchors[2].transform.position;

        // Proietta point2 sulla stessa X di point1 
        point2 = new Vector3(point1.x, point2.y, point1.z);

        // Proietta point3 sulla stessa Y di point2 
        point3 = new Vector3(point3.x, point2.y, point3.z);

        // calcolo l'altezza
        Vector3 vHeight = point1 - point2;

        // calcolo la larghezza
        Vector3 vWidth = point3 - point2;

        // l'altezza sarà la coord. y, e la larghezza coord. x, di conseguenza l'altezza è rivolta verso vector3.up e la larghezza verso vector3.right
        Vector3 normal = Vector3.Cross(vWidth, vHeight).normalized;

        Vector3 up = vHeight.normalized;
        Vector3 forward = normal;

        // Crea una rotazione basata su forward e up
        Quaternion rotation = Quaternion.LookRotation(forward, up);

        // calcolo il centro di dove verrà spostato il subplane
        Vector3 center = point2 + (vHeight / 2) + (vWidth / 2);

        if(this.center != null)
            this.center.transform.position = center;
        else
            this.center = Instantiate(anchorPrefab, center, rotation);

        this.center.GetComponent<SubplaneAnchor>().enabled = false;
        //Debug.Log("BCZ Instanzio il subplane");
        transform.parent.position = center;
        transform.parent.rotation = rotation;
        //GameObject subplane = Instantiate(subplanePrefab, center, rotation);
        //Debug.Log("BCZ subplane instanziato");

        // resize del piano
        transform.localScale = new Vector3(vWidth.magnitude, vHeight.magnitude, 0.01f);
        //Debug.Log("SubplaneHeight: " + vHeight.magnitude);
        //Debug.Log("SubplaneWidth: " + vWidth.magnitude);

        //Debug.Log("BCZ subplane localscale: " + transform.localScale);
    }

    public void HideSubplane(){
        Debug.Log("BCZ Nascondo i subplane");
        isVisible = false;
        foreach(GameObject anchor in anchors){
            anchor.SetActive(false);
        }
        Color materialColor = GetComponent<Renderer>().material.color;
        materialColor = new Color(materialColor.r, materialColor.g, materialColor.b, 0);
        GetComponent<Renderer>().material.color = materialColor;
    }

    public void ShowSubplane(){
        Debug.Log("BCZ Moatro i subplane");
        isVisible = true;
        foreach(GameObject anchor in anchors){
            anchor.SetActive(true);
        }
        Color materialColor = GetComponent<Renderer>().material.color;
        materialColor = new Color(materialColor.r, materialColor.g, materialColor.b, 0.7f);
        GetComponent<Renderer>().material.color = materialColor;
    }

    public Vector3 NormalizedHitPoint(Vector3 localHitPoint){
        //TODO
        // Fixare il fatto che la superficie del subplane ha larghezza in z invece che in y
        float x = localHitPoint.x + 0.5f;
        float y = localHitPoint.y + 0.5f;
        /*if(localHitPoint.y >= 0)
            y = 0.5f - localHitPoint.y;
        else
            y = -localHitPoint.y + 0.5f;*/

        Debug.Log("x, y: " + x + ", " + y);
        return new Vector3(x, y, 0);
    }
}
