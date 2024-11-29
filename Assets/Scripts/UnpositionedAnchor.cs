using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnpositionedAnchor : MonoBehaviour
{
    public GameObject linePrefab;
    private bool _isSelected;

    void Start()
    {
        GameObject xAxis = Instantiate(linePrefab, transform);
        Debug.Log("xAxis.transform.position: " + xAxis.transform.localPosition);
        xAxis.transform.localPosition = xAxis.transform.localPosition + new Vector3(0.5f, 0, 0);
        Debug.Log("xAxis.transform.position dopo: " + xAxis.transform.localPosition);
        xAxis.transform.Rotate(transform.forward, 90);
        xAxis.GetComponent<MeshRenderer>().material.color = Color.red;

        GameObject yAxis = Instantiate(linePrefab, transform);
        Debug.Log("yAxis.transform.position: " + yAxis.transform.localPosition);
        yAxis.transform.localPosition = yAxis.transform.localPosition + new Vector3(0, 0.5f, 0);
        Debug.Log("yAxis.transform.position dopo: " + yAxis.transform.localPosition);
        yAxis.GetComponent<MeshRenderer>().material.color = Color.green;

        GameObject zAxis = Instantiate(linePrefab, transform);
        Debug.Log("zAxis.transform.position: " + zAxis.transform.localPosition);
        zAxis.transform.localPosition = zAxis.transform.localPosition + new Vector3(0, 0, 0.5f);
        Debug.Log("zAxis.transform.position dopo: " + zAxis.transform.localPosition);
        zAxis.transform.Rotate(transform.right, 90);
        zAxis.GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    void Update()
    {

    }
}
