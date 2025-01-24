using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingManager : MonoBehaviour
{
    [SerializeField]
    private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField]
    private TMP_Text distanceTxt;
    [SerializeField]
    private GameObject TrackedImagePrefab;
    private GameObject trackedImageObject;
    private Transform displayPosition = null;
    
    private SubplaneConfig subplaneConfig;
    private Subplane existingSubplane;

    private Dictionary<TrackableId, GameObject> trackedAnchors = new Dictionary<TrackableId, GameObject>();

    void Start(){
        if(!m_TrackedImageManager){
            Debug.LogError("ImageTrackingManager: no reference to ARTrackedImageManager");
            return;
        }
        m_TrackedImageManager.enabled = false;
    }

    void OnEnable(){
        if(!m_TrackedImageManager){
            Debug.LogError("ImageTrackingManager: no reference to ARTrackedImageManager");
            return;
        }
        m_TrackedImageManager.trackedImagesChanged += OnChanged;
    }

    void OnDisable(){
        if(!m_TrackedImageManager){
            Debug.LogError("ImageTrackingManager: no reference to ARTrackedImageManager");
            return;
        }
        m_TrackedImageManager.trackedImagesChanged -= OnChanged;
    }

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        foreach (ARTrackedImage newImage in eventArgs.added)
        {
            displayPosition = newImage.transform;
            Debug.Log("BCZ immagine: " + newImage.transform.position);
            if(subplaneConfig && !trackedAnchors.ContainsKey(newImage.trackableId)){
                CreateSubplaneAnchorByImage(newImage);
            }
        }

        foreach (ARTrackedImage updatedImage in eventArgs.updated)
        {
            // Handle updated event
            Vector3 offset = updatedImage.size/2;
            TrackableId trackableId = updatedImage.trackableId;
            if(trackedAnchors.ContainsKey(trackableId) && trackedAnchors[trackableId]){
                //Debug.Log("updatedImage: " + updatedImage.referenceImage.name);
                if(updatedImage.referenceImage.name.Equals("upleft")){
                    offset = /*new Vector3(-offset.x, offset.y) **/ -updatedImage.transform.right * offset.x + updatedImage.transform.forward * offset.y;
                }
                else if(updatedImage.referenceImage.name.Equals("downleft")){
                    offset = /*new Vector3(-offset.x, -offset.y) */ -updatedImage.transform.right * offset.x - updatedImage.transform.forward * offset.y;
                }
                else if(updatedImage.referenceImage.name.Equals("downright")){
                    offset = /*new Vector3(offset.x, -offset.y)*/ updatedImage.transform.right * offset.x - updatedImage.transform.forward * offset.y;
                }
                //Quaternion rotation = GetRotationRelativeToTransform(updatedImage.transform);
                //Debug.LogWarning("rotation: " + rotation);
                //offset = rotation * offset;
                //Debug.LogWarning("" + updatedImage.referenceImage.name + " off: " + offset);
                if(trackedAnchors[trackableId].transform.position != updatedImage.transform.position + offset){
                    trackedAnchors[trackableId].transform.position = updatedImage.transform.position + offset;
                }
                //trackedAnchors[trackableId].transform.position = updatedImage.transform.position + updatedImage.transform.right;
            }
            else{
                CreateSubplaneAnchorByImage(updatedImage);
            }
        }

        foreach (ARTrackedImage removedImage in eventArgs.removed)
        {
            Debug.LogWarning("Rimossa immagine: " + removedImage.referenceImage.name);
        }
    }

    private Quaternion GetRotationRelativeToTransform(Transform transform1, Transform transform2){
        Quaternion rotation = Quaternion.identity;
        rotation = Quaternion.FromToRotation(transform1.right, transform2.right);
        //rotation *= Quaternion.FromToRotation(transform1.up, transform2.up);
        return rotation;
    }

    public void CreateSubplaneAnchorByImage(ARTrackedImage newImage){
        trackedImageObject = subplaneConfig.CreateSubplaneAnchor(newImage.transform.position);
        //trackedImageObject.transform.rotation *= GetRotationRelativeToTransform(trackedImageObject.transform, newImage.transform);
        trackedAnchors.Add(newImage.trackableId, trackedImageObject);
    }

    public void ResetImageTrackingConfiguration(){
        foreach(KeyValuePair<TrackableId, GameObject> entry in trackedAnchors){
            Debug.LogWarning("Distruggo: " + trackedAnchors[entry.Key]);
            Destroy(trackedAnchors[entry.Key]);
        }
        trackedAnchors.Clear();
        Debug.LogWarning("Pulito il tracked anchors: " + trackedAnchors);
        m_TrackedImageManager.enabled = true;
    }

    public void DisableTrackingConfiguration(){
        Debug.LogWarning("Disattivo imagetracking");
        if(!m_TrackedImageManager){
            Debug.LogError("ImageTrackingManager: no reference to ARTrackedImageManager");
            return;
        }
        //ResetImageTrackingConfiguration();
        m_TrackedImageManager.enabled = false;
    }

    public void ActivateTrackingConfiguration(){
        Debug.LogWarning("Attivo imagetracking");
        if(!m_TrackedImageManager){
            Debug.LogError("ImageTrackingManager: no reference to ARTrackedImageManager");
            return;
        }
        //ResetImageTrackingConfiguration();
        m_TrackedImageManager.enabled = true;
    }

}
