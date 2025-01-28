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
    [Networked]
    public Color interactionColor {get; set;}
    [Networked]
    public MovableObject networkedSelectedObject {get; set;}
    private SubplaneConfig subplaneConfig;
    private XRBaseInteractor xRBaseInteractor;
    private ARGestureInteractor arGestureInteractor;
    public bool isDragging;
    public bool isTwoFingerDragging;
    private float dragGestureUpdateInterval = 1f;
    private float dragGestureLastUpdate;
    private DragGesture currentDragGesture;
    private float twoFingerDragGestureUpdateInterval = 1f;
    private float twoFingerDragGestureLastUpdate;
    private TwoFingerDragGesture currentTwoFingerDragGesture;

    public override void Spawned(){
        if(HasStateAuthority){
            interactionColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 0.5f, 0.9f, 0.9f);
            xRBaseInteractor = FindObjectOfType<XRBaseInteractor>();
            arGestureInteractor = FindObjectOfType<ARGestureInteractor>();

            arGestureInteractor.dragGestureRecognizer.slopInches = 0.1f;
            arGestureInteractor.twoFingerDragGestureRecognizer.slopInches = 0.1f;

            arGestureInteractor.tapGestureRecognizer.onGestureStarted += TapGestureStart;
            arGestureInteractor.dragGestureRecognizer.onGestureStarted += DragGestureStart;
            arGestureInteractor.twoFingerDragGestureRecognizer.onGestureStarted += TwoFingerDragGestureStart;
            
        }
    }

    public void Update(){
        if(isDragging && currentDragGesture != null){
            if(dragGestureLastUpdate + dragGestureUpdateInterval <= Time.deltaTime)
            {
                dragGestureLastUpdate = Time.deltaTime;
                currentDragGesture.onUpdated += HandleARDrag;
            }
        }
        if(isTwoFingerDragging && currentTwoFingerDragGesture != null){
            if(twoFingerDragGestureLastUpdate + twoFingerDragGestureUpdateInterval <= Time.deltaTime)
            {
                twoFingerDragGestureLastUpdate = Time.deltaTime;
                currentTwoFingerDragGesture.onUpdated += HandleTwoFingerDrag;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        if(subplaneConfig.isMirror)
            return;
        
        // se non è ancora stato creato un subplane non vogliamo gestire i touch del giocatore
        if(!subplaneConfig.GetSelectedSubplane())
            return;

        if (HasStateAuthority && Input.touchCount > 0)
        {
            // Touch touch = Input.GetTouch(0);
            // if (touch.phase == TouchPhase.Ended /*&& selectedObject == null*/)
            // {
            //     HandleSingleTouch(touch);
            // }
            /*else if(touch.phase == TouchPhase.Moved && selectedObject){
                HandleTranslateTouch(touch);
            }*/
            /*else if(touch.phase == TouchPhase.Ended && selectedObject){
                HandleSingleTouch(touch);
            }*/
        }
    }

    private void HandleSingleTouch(Touch touch){
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        
        if(subplaneConfig.IsConfig())
            return;

        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5f);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = ray.direction;
            Debug.Log("BCZ colpisco, " + hit.transform.gameObject.name);
            Subplane hittedSubplane = hit.transform.gameObject.GetComponent<Subplane>();
            if(hittedSubplane){
                Debug.Log("Hit display global: " + hit.point);
                Debug.Log("Hit display local: " + hit.transform.InverseTransformPoint(hit.point));
                Vector3 cameraMappedPoint = hittedSubplane.NormalizedHitPoint(hit.transform.InverseTransformPoint(hit.point));
                
                RaycastFromVRCameraRPC(cameraMappedPoint, direction, subplaneConfig.isMirror);
            }
            else{
                SendLocalPoint(hit, direction);
                Debug.Log("Cliccato fuori");
            }
        }
    }

    private void HandleTranslateTouch(Touch touch){
        if(!subplaneConfig) 
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        
        if(subplaneConfig.IsConfig())
            return;

        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5f);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = ray.direction;
            Debug.Log("BCZ colpisco, " + hit.transform.gameObject.name);
            Subplane hittedSubplane = hit.transform.gameObject.GetComponent<Subplane>();
            if(hittedSubplane){
                Debug.Log("Hit display global: " + hit.point);
                Debug.Log("Hit display local: " + hit.transform.InverseTransformPoint(hit.point));
                Vector3 cameraMappedPoint = hittedSubplane.NormalizedHitPoint(hit.transform.InverseTransformPoint(hit.point));
                /*if(aRSphereController){
                    Debug.Log("BCZ ID: " + aRSphereController.Id);
                    aRSphereController.SendPointToDesktop(cameraMappedPoint, direction);
                }*/
                // TODO sendremotepointrpc
                
                RaycastFromVRCameraRPC(cameraMappedPoint, direction, subplaneConfig.isMirror);
            }
            else{
                //aRSphereController.MoveSelectedObject(hit.point);
                // TODO local moving object
                SendLocalPoint(hit, direction);
                Debug.Log("Cliccato fuori");
            }
        }
    }

    // Funzione che viene eseguita solo sul client nel mondo VR, quindi l'oggetto PhoneRepresentation non ha StateAuthority, motivo per cui è necessaria l'RPC e non è possibile modificare la proprietà networked selectedObject
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public async void RaycastFromVRCameraRPC(Vector3 cameraMappedPoint, Vector3 direction, bool isMirror){
        if(!PlatformManager.IsDesktop())
            return;
        
        Ray ray = Camera.main.ViewportPointToRay(cameraMappedPoint);
        if (Physics.Raycast(ray, out RaycastHit hit)){
            if(hit.collider.tag.Equals("MovableObject")){
                TrySelectObject(hit.collider.GetComponent<MovableObject>());
            }
            else{
                if(networkedSelectedObject == null){
                    Vector3 directionToPlayer = transform.position - hit.point;
                    directionToPlayer = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
                    Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                    
                    Runner.Spawn(hitObjectPrefab, hit.point, lookRotation);
                }
                else{
                    if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                        await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
                    Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                    networkedSelectedObject.UpdateTransform(hit.point, false);
                }
            }
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
        //controllo non necessario, perchè è controllato nella networkupdate
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

                NetworkObject spawned = Runner.Spawn(hitObjectPrefab, hit.point, lookRotation);
                // quando spawnato in locale deve avere settato il subplane attivo locale
                spawned.GetComponent<MovableObject>().UpdateTransform(hit.point, true);
            }
            else{
                if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                    await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
                Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                networkedSelectedObject.UpdateTransform(hit.point, true);
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

    // TAP GESTURE HANDLING

    public void TapGestureStart(TapGesture gesture){
        Debug.LogWarning("Chiamata HandleARTap");
        gesture.onFinished += HandleARTap;
    }

    public void HandleARTap(TapGesture gesture){
        Debug.LogWarning("Finita la gesture Tap. isDragging: " + isDragging);
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        
        if(subplaneConfig.IsConfig())
            return;
        
        if(isDragging || isTwoFingerDragging)
            return;

        Ray ray = Camera.main.ScreenPointToRay(gesture.startPosition);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5f);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = ray.direction;
            Debug.Log("BCZ colpisco, " + hit.transform.gameObject.name);
            Subplane hittedSubplane = hit.transform.gameObject.GetComponent<Subplane>();
            if(hittedSubplane){
                Debug.Log("Hit display global: " + hit.point);
                Debug.Log("Hit display local: " + hit.transform.InverseTransformPoint(hit.point));
                Vector3 cameraMappedPoint = hittedSubplane.NormalizedHitPoint(hit.transform.InverseTransformPoint(hit.point));
                
                RaycastFromVRCameraRPC(cameraMappedPoint, direction, subplaneConfig.isMirror);
            }
            else{
                SendLocalPoint(hit, direction);
                Debug.Log("Cliccato fuori");
            }
        }
    }

    // DRAG GESTURE HANDLING

    // controllare se il drag inizia colpendo l'oggetto che è selezionato, se no non fare nulla
    private void DragGestureStart(DragGesture dragGesture){
        if(!networkedSelectedObject)
            return;
        dragGestureLastUpdate = Time.deltaTime;
        isDragging = true;
        currentDragGesture = dragGesture;
        dragGesture.onUpdated += HandleARDrag;
        dragGesture.onFinished += ARDragEnded;
    }

    // spostare l'oggetto selezionato nel punto colpito dal raycast
    private async void HandleARDrag(DragGesture dragGesture)
    {
        if(!networkedSelectedObject)
            return;
        LayerMask layerMask = LayerMask.GetMask("MovableObjects");
        if(layerMask == -1){
            Debug.LogError("Il LayerMask MovabeleObjects non è definito");
        }
        // esclusione del layer di movableobjects
        layerMask = ~layerMask;
        Ray ray = Camera.main.ScreenPointToRay(dragGesture.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,layerMask))
        {
            if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
            Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
            networkedSelectedObject.UpdateTransform(hit.point, true);
        }
        Debug.Log($"Drag da {dragGesture.startPosition} a {dragGesture.position}");
        currentDragGesture = dragGesture;
    }

    // fine drag gesture
    private void ARDragEnded(DragGesture dragGesture){
        Debug.Log($"stopped drag");
        isDragging = false;
        dragGesture.onUpdated -= HandleARDrag;
        dragGesture.onFinished -= ARDragEnded;
        currentDragGesture = null;
    }

    // TWO FINGER DRAG GESTURE HANDLING
    private void TwoFingerDragGestureStart(TwoFingerDragGesture gesture){
        if(!networkedSelectedObject)
            return;
        twoFingerDragGestureLastUpdate = Time.deltaTime;
        isTwoFingerDragging = true;
        currentTwoFingerDragGesture = gesture;
        gesture.onUpdated += HandleTwoFingerDrag;
        gesture.onFinished += TwoFingerEnded;

    }

    private void HandleTwoFingerDrag(TwoFingerDragGesture gesture){
        //Debug.Log($"TwoFDrag da {gesture.startPosition1} a {gesture.position}");
    }

    private void TwoFingerEnded(TwoFingerDragGesture gesture){
        Debug.Log($"stopped two finger drag");
        isTwoFingerDragging = false;
        currentTwoFingerDragGesture = null;
        gesture.onUpdated -= HandleTwoFingerDrag;
        gesture.onFinished -= TwoFingerEnded;
        if((gesture.startPosition1 + gesture.startPosition2 / 2).y + 5f < gesture.position.y){
            Debug.LogWarning("push in");
        }
        else if((gesture.startPosition1 + gesture.startPosition2 / 2).y + 5f > gesture.position.y){
            Debug.LogWarning("pull out");
        }
    }

    /*public void OnNetworkSelectedObjectChanged(){
        if(!HasStateAuthority)
            return;
        if(networkedSelectedObject){
            if(!networkedSelectedObject.GetComponent<ARNetworkSelectionInteractable>().isSelected){
                IXRSelectInteractable selectedInteractable = GetInteractorSelectedObject();
                if(selectedInteractable == null){
                    
                }
            }
        }
    }

    // supponiamo che ci sia un solo interactor (in caso di più interactor tipo se vogliamo gestire input con raycast, dobbiamo aggiungere logica per la gestione della disattivazione degli interactor aggiuntivi)
    private IXRSelectInteractable GetInteractorSelectedObject(){
        if(!xRBaseInteractor)
            xRBaseInteractor = FindAnyObjectByType<XRBaseInteractor>();
        return xRBaseInteractor.firstInteractableSelected;
    }

    private void SetObjectSelectedByXRI(ARNetworkSelectionInteractable interactable, XRBaseInteractor xRBaseInteractor){
        xRBaseInteractor.
    }*/
}
