using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

public class ImageTrackingManager : MonoBehaviour
{
    [SerializeField]
    private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField]
    private TMP_Text distanceTxt;
    [SerializeField]
    private GameObject TrackedImagePrefab;
    private GameObject oggetto;
    private Transform displayPosition = null;
    

    void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;

    void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            //m_TrackedImageManager.enabled = false;
            displayPosition = newImage.transform;
            //oggetto = Instantiate(TrackedImagePrefab, displayPosition.position, displayPosition.rotation);
            oggetto = Instantiate(TrackedImagePrefab, newImage.transform);
            Debug.Log("BCZ immagine: " + displayPosition.position.x + " " + displayPosition.position.y + " " + displayPosition.position.z);
            Debug.Log("BCZ oggetto: " + oggetto.transform.position.x + " " + oggetto.transform.position.y + " " + oggetto.transform.position.z);

            GameObject imageTracker = GameObject.FindWithTag("ImageTracker");
            if(imageTracker)
                imageTracker.GetComponent<ImageTracker>().RpcImageTracked();
            else
                Debug.Log("BCZ non c'è l'image tracker");
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            // Handle updated event
        }

        foreach (var removedImage in eventArgs.removed)
        {
            // Handle removed event
        }
    }

    void Update(){
        if(displayPosition == null)
            return;
        
        float distance = Vector3.Distance(oggetto.transform.position, Camera.main.transform.position);
        distanceTxt.text = distance + " m " + oggetto.transform.position.x + " " + oggetto.transform.position.y + " " + oggetto.transform.position.z;
        //Debug.Log("BCZ distance: " + distance + " pos: " + oggetto.transform.position.x + " " + oggetto.transform.position.y + " " + oggetto.transform.position.z);
        if(oggetto){
            Debug.DrawLine(Camera.main.transform.position, oggetto.transform.position, Color.green);
        }
    }
}
