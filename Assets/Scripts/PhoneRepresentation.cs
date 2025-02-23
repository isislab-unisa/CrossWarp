using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class PhoneRepresentation : NetworkBehaviour
{
    public GameObject hitObjectPrefab;
    public List<GameObject> hitObjectsPrefabs;
    [Networked]
    public Color interactionColor {get; set;}
    [Networked]
    public MovableObject networkedSelectedObject {get; set;}
    private SubplaneConfig subplaneConfig;

    public override void Spawned(){
        if(HasStateAuthority){
            interactionColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 0.5f, 0.9f, 0.9f);
        }
        if(!PlatformManager.IsDesktop()){
            GetComponentInChildren<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        move(Camera.main.transform.position, Camera.main.transform.rotation);
    }

    public void move(Vector3 newPosition, Quaternion newRotation){
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        GameObject selectedSubplane = subplaneConfig.GetSelectedSubplane();

        if(!selectedSubplane)
            return;

        // calcolo la differenza in rotazione tra il subplane e il forward globale
        Quaternion rotation = GetRotationRelativeToSelectedSubplane();

        // calcolo la posizione relativa al subplane
        Vector3 position = newPosition - selectedSubplane.transform.position;

        // ruoto la posizione in base all'offset in rotazione
        position = rotation * position;
        newRotation *= rotation;

        if(HasStateAuthority)
            UpdatePosition(position, newRotation, subplaneConfig.isMirror);
    }

    private Quaternion GetRotationRelativeToSelectedSubplane(){
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        GameObject selectedSubplane = subplaneConfig.GetSelectedSubplane();

        if(!selectedSubplane)
            return Quaternion.identity;
        
        Quaternion rotation = Quaternion.FromToRotation(selectedSubplane.transform.forward, Vector3.forward);
        return rotation;
    }

    public void UpdatePosition(Vector3 newPosition, Quaternion newRotation, bool isMirror){
        //Debug.Log("Chiamata updateposition");
        float z = newPosition.z;
        if(isMirror){
            z = -z;
            newRotation = Quaternion.Euler(-newRotation.eulerAngles.x, -newRotation.eulerAngles.y, newRotation.eulerAngles.z);
        }
        z += Camera.main.nearClipPlane;
        transform.rotation = newRotation;
        transform.position = new Vector3(newPosition.x, newPosition.y, z);
        //GetComponent<NetworkTransform>().Teleport(transform.position, newRotation);
        //Debug.Log("newposition: " + transform.position);
        //Debug.Log("stateauthority: " + HasStateAuthority);
    }

    public async void SendLocalPoint(RaycastHit hit, Vector3 direction){
        if(!HasStateAuthority)
            return;
        
        if(hit.collider.tag.Equals("MovableObject")){
            TrySelectObject(hit.collider.GetComponent<MovableObject>());
        }
        else{
            if(networkedSelectedObject == null){
                Vector3 directionToPlayer = Camera.main.transform.position - hit.point;
                directionToPlayer = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                int x = UnityEngine.Random.Range(0, hitObjectsPrefabs.Count);
                GameObject randomPrefab = hitObjectsPrefabs[x];
                NetworkObject spawned = Runner.Spawn(randomPrefab, hit.point, lookRotation);
                // quando spawnato in locale deve avere settato il subplane attivo locale
                spawned.GetComponent<MovableObject>().UpdatePosition(hit.point);
            }
            else{
                if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                    await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
                Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                networkedSelectedObject.UpdatePosition(hit.point);
            }
        }        
    }

    // true: action performed successfully
    // false: something went wrong
    public async Task<bool> TrySelectObject(MovableObject obj)
    {
        Debug.LogWarning("networked obj: " + networkedSelectedObject);
        if(networkedSelectedObject && obj.gameObject == networkedSelectedObject.gameObject){
            // deselect selected object
            networkedSelectedObject.ReleaseSelection();
            networkedSelectedObject = null;
            if(!HasStateAuthority)
                    ResetSelectedObjectRPC();
            return true;
        }
        else if(networkedSelectedObject != null){
            // deselect old selectedObject
            networkedSelectedObject.ReleaseSelection();
            // select new object
            if(await obj.TrySelectObject(this)){
                networkedSelectedObject = obj;
                Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                
                if(!HasStateAuthority)
                    UpdateSelectedObjectRPC(obj.GetComponent<NetworkObject>().Id);
                return true;
            }
        }
        else{
            // select new object
            if(await obj.TrySelectObject(this)){
                networkedSelectedObject = obj;
                Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                
                if(!HasStateAuthority)
                    UpdateSelectedObjectRPC(obj.GetComponent<NetworkObject>().Id);
                return true;
            }
        }
        return false;
    }

    public async Task<bool> RequestStateAuthorityOnSelectedObject(){
        if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
            return await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
        return true;
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public async void RequestStateAuthorityOnObjectRpc(NetworkId selectedId){
        NetworkObject obj;
        MovableObject orphanObject;
        if(Runner.TryFindObject(selectedId, out obj)){
            orphanObject = obj.gameObject.GetComponent<MovableObject>();
            if(!orphanObject.GetComponent<NetworkObject>().HasStateAuthority){
                await orphanObject.GetComponent<NetworkObject>().WaitForStateAuthority();
                Debug.LogError("lastPos: " + orphanObject.lastOffsetToSubplane);
                orphanObject.UpdateWorldState();
            }
        }
    }

    // RPCs
    // Funzione che viene eseguita solo sul client nel mondo VR, quindi l'oggetto PhoneRepresentation non ha StateAuthority, motivo per cui è necessaria l'RPC e non è possibile modificare la proprietà networked selectedObject
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RaycastFromVRCameraRPC(Vector3 cameraMappedPoint, Vector3 direction, bool isMirror){
        if(!PlatformManager.IsDesktop())
            return;
            
        LayerMask layerMask = LayerMask.GetMask("MovableObjectsPhysics");
        if(layerMask == -1){
            Debug.LogError("Il LayerMask MovabeleObjectsPhysics non è definito");
        }
        layerMask = ~layerMask;
        
        Ray ray = Camera.main.ViewportPointToRay(cameraMappedPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)){
            if(hit.collider.tag.Equals("MovableObject")){
                TrySelectObject(hit.collider.GetComponent<MovableObject>());
            }
            else{
                if(networkedSelectedObject == null){
                    Vector3 directionToPlayer = transform.position - hit.point;
                    directionToPlayer = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
                    Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                    
                    int x = UnityEngine.Random.Range(0, hitObjectsPrefabs.Count);
                    GameObject randomPrefab = hitObjectsPrefabs[x];
                    Runner.Spawn(randomPrefab, hit.point, lookRotation);
                }
                else{
                    if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                        await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
                    Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                    networkedSelectedObject.UpdatePosition(hit.point);
                }
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void ContinousRaycastFromVRCameraRPC(Vector3 cameraMappedPoint){
        if(!PlatformManager.IsDesktop())
            return;
        if(!networkedSelectedObject)
            return;
        LayerMask layerMask = LayerMask.GetMask("MovableObjects", "MovableObjectsPhysics");
        if(layerMask == -1){
            Debug.LogError("Il LayerMask MovabeleObjects e/o MovableObjectsPhysics non è definito");
        }
        // esclusione del layer di movableobjects
        Debug.LogWarning("CamMapPnt: " + cameraMappedPoint);
        layerMask = ~layerMask;
        Ray ray = Camera.main.ViewportPointToRay(cameraMappedPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,layerMask)){
            if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
            //Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
            networkedSelectedObject.UpdatePosition(hit.point);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void UpdateSelectedObjectRPC(NetworkId selectedId){
        NetworkObject obj;
        if(Runner.TryFindObject(selectedId, out obj))
            networkedSelectedObject = obj.gameObject.GetComponent<MovableObject>();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ResetSelectedObjectRPC(){
        networkedSelectedObject = null;
    }

}
