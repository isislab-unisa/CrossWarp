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
    public Dictionary<PlayerRef, GameObject> playersRepresentation {get; set;}
    private int playersConfiguring = 0;
    public GameObject ImageTrackingCanvasPrefab;
    private GameObject ImageTrackingCanvasInstance;

    void Start()
    {
        if(PlatformManager.IsDesktop()){
            ImageTrackingCanvasInstance = Instantiate(ImageTrackingCanvasPrefab);
            playersRepresentation = new Dictionary<PlayerRef, GameObject>();
            // screen height in cm
            float screenHeight = (Screen.height/Screen.dpi)*2.54f;
            // frustum height in m
            float frustumHeight = screenHeight/100;
            float distance = frustumHeight * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
            Camera.main.nearClipPlane = distance;
            Transform NCPCenter = Camera.main.transform.GetChild(0);
            NCPCenter.position = new Vector3(NCPCenter.position.x, NCPCenter.position.y, Camera.main.transform.position.z + distance);
            transform.position = Camera.main.transform.position + new Vector3(0, 0, distance + 1);
            Debug.Log("BCZ frustum distance: " + distance);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RecalibrateNearClipPlaneRpc(float desiredHeight){
        float distance = desiredHeight * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Camera.main.nearClipPlane = distance;
        Transform NCPCenter = Camera.main.transform.GetChild(0);
        NCPCenter.position = new Vector3(NCPCenter.position.x, NCPCenter.position.y, Camera.main.transform.position.z + distance);
        Debug.Log("BCZ frustum distance: " + distance);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ToggleConfiguringRpc(bool playerIsConfiguring){
        Debug.Log("Players Configuring: " + playersConfiguring);
        if(playerIsConfiguring)
            playersConfiguring++;
        else if(playersConfiguring > 0)
            playersConfiguring--;
        if(playersConfiguring > 0)
            ImageTrackingCanvasInstance.SetActive(true);
        else
            ImageTrackingCanvasInstance.SetActive(false);
    }

}
