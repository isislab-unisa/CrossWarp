using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public class Subplane : MonoBehaviour
{
    public GameObject anchorPrefab;
    private List<GameObject> anchors = new List<GameObject>();
    private GameObject center;
    private bool isVisible = true;
    void Start()
    {
        
    }


    void Update()
    {
        if(anchors.Count == 3 && isVisible){
            //Debug.Log("Renderizzo subplane: " + anchors[0]);
            // da migliorare in modo che renderizza solo quando cambiano i punti
            RenderSubplane();
        }
    }

    public void SetAnchors(List<GameObject> selectedAnchors){
        anchors = selectedAnchors;
        RenderSubplane();
    }

    private void RenderSubplane(){
        Vector3 point1 = anchors[0].transform.position;
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

        if(this.center != null)
            this.center.transform.position = center;
        else
            this.center = Instantiate(anchorPrefab, center, rotation);

        //Debug.Log("BCZ Instanzio il subplane");
        transform.position = center;
        transform.rotation = rotation;
        //GameObject subplane = Instantiate(subplanePrefab, center, rotation);
        //Debug.Log("BCZ subplane instanziato");

        // resize del piano
        transform.localScale = new Vector3(vHeight.magnitude, 0.01f, vWidth.magnitude);
        //Debug.Log("SubplaneHeight: " + vHeight.magnitude);
        //Debug.Log("SubplaneWidth: " + vWidth.magnitude);

        //Debug.Log("BCZ subplane localscale: " + transform.localScale);
    }

    public void HideSubplane(){
        Debug.Log("BCZ Nascondo plane");
        isVisible = false;
        foreach(GameObject anchor in anchors){
            anchor.SetActive(false);
        }
        Color materialColor = GetComponent<Renderer>().material.color;
        materialColor = new Color(materialColor.r, materialColor.g, materialColor.b, 0.1f);
    }

    public Vector3 NormalizedHitPoint(Vector3 localHitPoint){
        //TODO
        // Fixare il fatto che la superficie del subplane ha larghezza in z invece che in y
        float x = localHitPoint.x + 0.5f;
        float z = 0;
        if(localHitPoint.z >= 0)
            z = 0.5f - localHitPoint.z;
        else
            z = -localHitPoint.z + 0.5f;

        Debug.Log("x, z: " + x + ", " + z);
        return new Vector3(x, z, 0);
    }
}
