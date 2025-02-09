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
    private XRBaseInteractor xRBaseInteractor;
    private ARGestureInteractor arGestureInteractor;
    public bool isDragging;
    private float dragGestureUpdateInterval = 1f;
    private float dragGestureLastUpdate;
    private float hitOnSubplaneThreshold = 0.02f;
    private Vector3 lastHitOnSubplane;
    private DragGesture currentDragGesture;
    public bool isTwoFingerDragging;
    private float twoFingerDragGestureUpdateInterval = 1f;
    private float twoFingerDragGestureLastUpdate;
    private TwoFingerDragGesture currentTwoFingerDragGesture;
    public bool isTwisting;
    private float twistGestureUpdateInterval = 1f;
    private float twistGestureLastUpdate;
    public float twistGestureRotateRate = 2.5f;
    private TwistGesture currentTwistGesture;
    
    public bool isPinching;
    private float pinchGestureUpdateInterval = 1f;
    private float pinchGestureLastUpdate;
    public float pinchGestureScaleRate = 0.000001f;
    private PinchGesture currentPinchGesture;

    public override void Spawned(){
        if(HasStateAuthority){
            interactionColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 0.5f, 0.9f, 0.9f);
            xRBaseInteractor = FindObjectOfType<XRBaseInteractor>();
            arGestureInteractor = FindObjectOfType<ARGestureInteractor>();

            // la drag gesture inizia se slopinches è maggiore di 0.1
            arGestureInteractor.dragGestureRecognizer.slopInches = 0.075f;

            // la tap gesture viene cancellata se slopinches è maggiore di 0.05 o se il tap dura più di 0.5s
            arGestureInteractor.tapGestureRecognizer.slopInches = 0.1f;
            arGestureInteractor.tapGestureRecognizer.durationSeconds = 0.5f;

            arGestureInteractor.twoFingerDragGestureRecognizer.slopInches = 0.1f;


            arGestureInteractor.tapGestureRecognizer.onGestureStarted += TapGestureStart;
            arGestureInteractor.dragGestureRecognizer.onGestureStarted += DragGestureStart;
            arGestureInteractor.twoFingerDragGestureRecognizer.onGestureStarted += TwoFingerDragGestureStart;
            arGestureInteractor.twistGestureRecognizer.onGestureStarted += TwistGestureStart;
            arGestureInteractor.pinchGestureRecognizer.onGestureStarted += PinchGestureStart;
            
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
        if(isTwisting && currentTwistGesture != null){
            if(twistGestureLastUpdate + twistGestureUpdateInterval <= Time.deltaTime)
            {
                twistGestureLastUpdate = Time.deltaTime;
                currentTwistGesture.onUpdated += HandleTwist;
            }
        }
        if(isPinching && currentPinchGesture != null){
            if(pinchGestureLastUpdate + pinchGestureUpdateInterval <= Time.deltaTime)
            {
                pinchGestureLastUpdate = Time.deltaTime;
                currentPinchGesture.onUpdated += HandlePinch;
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
                    
                    int x = UnityEngine.Random.Range(0, hitObjectsPrefabs.Count);
                    GameObject randomPrefab = hitObjectsPrefabs[x];
                    Runner.Spawn(randomPrefab, hit.point, lookRotation);
                }
                else{
                    if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                        await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
                    Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                    networkedSelectedObject.UpdateTransform(hit.point);
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
        LayerMask layerMask = LayerMask.GetMask("MovableObjects");
        if(layerMask == -1){
            Debug.LogError("Il LayerMask MovabeleObjects non è definito");
        }
        // esclusione del layer di movableobjects
        Debug.LogWarning("CamMapPnt: " + cameraMappedPoint);
        layerMask = ~layerMask;
        Ray ray = Camera.main.ViewportPointToRay(cameraMappedPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,layerMask)){
            if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
            //Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
            networkedSelectedObject.UpdateTransform(hit.point);
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
                int x = UnityEngine.Random.Range(0, hitObjectsPrefabs.Count);
                GameObject randomPrefab = hitObjectsPrefabs[x];
                NetworkObject spawned = Runner.Spawn(randomPrefab, hit.point, lookRotation);
                // quando spawnato in locale deve avere settato il subplane attivo locale
                spawned.GetComponent<MovableObject>().UpdateTransform(hit.point);
            }
            else{
                if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                    await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
                Debug.Log("" + Runner.LocalPlayer + " hasSA: " + networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
                networkedSelectedObject.UpdateTransform(hit.point);
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
        
        if(isDragging || isTwoFingerDragging || isTwisting || isPinching)
            return;
        gesture.onFinished += HandleARTap;
    }

    public void HandleARTap(TapGesture gesture){
        Debug.LogWarning("Finita la gesture Tap. isDragging: " + isDragging);
        Debug.LogWarning("Tap isCanceled? " + gesture.isCanceled);
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        
        if(subplaneConfig.IsConfig())
            return;
        
        if(gesture.isCanceled || isDragging || isTwoFingerDragging || isTwisting || isPinching)
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
        Ray ray = Camera.main.ScreenPointToRay(dragGesture.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.GetComponent<Subplane>())
                dragGesture.onUpdated += HandleDragOnSubplane;
            else if(hit.collider.GetComponent<MovableObject>() == networkedSelectedObject)
                dragGesture.onUpdated += HandleARDrag;
        }

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
            networkedSelectedObject.UpdateTransform(hit.point);
        }
        Debug.Log($"Drag da {dragGesture.startPosition} a {dragGesture.position}");
        currentDragGesture = dragGesture;
    }

    private async void HandleDragOnSubplane(DragGesture gesture){
        if(!networkedSelectedObject)
            return;
        LayerMask layerMask = LayerMask.GetMask("MovableObjects");
        if(layerMask == -1){
            Debug.LogError("Il LayerMask MovabeleObjects non è definito");
        }
        // esclusione del layer di movableobjects
        layerMask = ~layerMask;
        Ray ray = Camera.main.ScreenPointToRay(gesture.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,layerMask))
        {
            Subplane hittedSubplane = hit.collider.GetComponent<Subplane>();
            if(hittedSubplane){
                Vector3 cameraMappedPoint = hittedSubplane.NormalizedHitPoint(hit.transform.InverseTransformPoint(hit.point));
                if(lastHitOnSubplane == null || Vector3.Distance(cameraMappedPoint, lastHitOnSubplane) >= hitOnSubplaneThreshold){
                    ContinousRaycastFromVRCameraRPC(cameraMappedPoint);
                    Debug.LogWarning("distanza: " + Vector3.Distance(cameraMappedPoint, lastHitOnSubplane));
                    lastHitOnSubplane = cameraMappedPoint;
                }
            }
        }
        currentDragGesture = gesture;
    }

    // fine drag gesture
    private void ARDragEnded(DragGesture dragGesture){
        Debug.Log("stopped drag");

        if(networkedSelectedObject){
            LayerMask layerMask = LayerMask.GetMask("MovableObjects");
            if(layerMask == -1){
                Debug.LogError("Il LayerMask MovabeleObjects non è definito");
            }
            // esclusione del layer di movableobjects
            layerMask = ~layerMask;
            Ray ray = Camera.main.ScreenPointToRay(dragGesture.position);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,layerMask))
            {
                if(hit.collider.GetComponent<Subplane>()){
                    networkedSelectedObject.StartPushInTransitionOnScreen();
                }
            }
        }

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
        Debug.Log("stopped two finger drag");
        if(!networkedSelectedObject)
            return;
        isTwoFingerDragging = false;
        currentTwoFingerDragGesture = null;
        gesture.onUpdated -= HandleTwoFingerDrag;
        gesture.onFinished -= TwoFingerEnded;
        if(isPinching || isTwisting)
            return;
        if((gesture.startPosition1 + gesture.startPosition2 / 2).y + 5f < gesture.position.y){
            Debug.LogWarning("push in");
            networkedSelectedObject.StartPushInTransition();
        }
        else if((gesture.startPosition1 + gesture.startPosition2 / 2).y + 5f > gesture.position.y){
            Debug.LogWarning("pull out");
            networkedSelectedObject.StartPullOutTransitionRPC();
        }
    }

    public async Task<bool> RequestStateAuthorityOnSelectedObject(){
        if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
            return await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
        return true;
    } 

    //TWIST GESTURE HANDLING

    private void TwistGestureStart(TwistGesture gesture){
        if(!networkedSelectedObject)
            return;
        twistGestureLastUpdate = Time.deltaTime;
        isTwisting = true;
        currentTwistGesture = gesture;
        gesture.onUpdated += HandleTwist;
        gesture.onFinished += TwistEnded;
    }

    private async void HandleTwist(TwistGesture gesture){
        //Debug.Log($"TwoFDrag da {gesture.startPosition1} a {gesture.position}");
        if(!networkedSelectedObject)
            return;
        if(isPinching)
            return;
        Debug.LogWarning("Rotation: " + gesture.deltaRotation);
        // non c'è bisogno di controllare che la platform sia mobile, le gesture vengono riconosciute solo su mobile
        // se si trova nel mondo aumentato
        if(networkedSelectedObject.worldState == MovableObjectState.inAR){
            networkedSelectedObject.UpdateRotation(-gesture.deltaRotation * twistGestureRotateRate);
        }
        // altrimenti se è nel mondo virtuale
        else if(networkedSelectedObject.worldState == MovableObjectState.inVR){
            if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
            networkedSelectedObject.UpdateRotation(-gesture.deltaRotation * twistGestureRotateRate);
        }
    }

    private void TwistEnded(TwistGesture gesture){
        Debug.Log("stopped twist");
        isTwisting = false;
        currentTwistGesture = null;
        gesture.onUpdated -= HandleTwist;
        gesture.onFinished -= TwistEnded;
    }

    //PINCH GESTURE HANDLING

    private void PinchGestureStart(PinchGesture gesture){
        if(!networkedSelectedObject)
            return;
        pinchGestureLastUpdate = Time.deltaTime;
        isPinching = true;
        currentPinchGesture = gesture;
        gesture.onUpdated += HandlePinch;
        gesture.onFinished += PinchEnded;
    }

    private async void HandlePinch(PinchGesture gesture){
        //Debug.Log($"TwoFDrag da {gesture.startPosition1} a {gesture.position}");
        if(!networkedSelectedObject)
            return;
            
        if(isTwisting)
            return;
        // non c'è bisogno di controllare che la platform sia mobile, le gesture vengono riconosciute solo su mobile
        // se si trova nel mondo aumentato
        if(networkedSelectedObject.worldState == MovableObjectState.inAR){
            Debug.LogWarning("gapDelta: " + gesture.gapDelta);
            Debug.LogWarning("resize: " + GetScaleByPinchGap(gesture.gapDelta * pinchGestureScaleRate));
            networkedSelectedObject.UpdateScale(GetScaleByPinchGap(gesture.gapDelta * pinchGestureScaleRate));
        }
        // altrimenti se è nel mondo virtuale
        else if(networkedSelectedObject.worldState == MovableObjectState.inVR){
            if(!networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                await networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
            Debug.LogWarning("gapDelta: " + gesture.gapDelta);
            Debug.LogWarning("resize: " + GetScaleByPinchGap(gesture.gapDelta * pinchGestureScaleRate));
            networkedSelectedObject.UpdateScale(GetScaleByPinchGap(gesture.gapDelta * pinchGestureScaleRate));
        }
    }

    private Vector3 GetScaleByPinchGap(float gap){
        
        float scaleValue = networkedSelectedObject.transform.localScale.x;
        float max = networkedSelectedObject.maxScale;
        float min = networkedSelectedObject.minScale;

        if(scaleValue + gap > max){
            scaleValue = max;
        }
        else if(scaleValue + gap < min){
            scaleValue = min;
        }
        else{
            scaleValue += gap;
        }

        return new Vector3(scaleValue, scaleValue, scaleValue);
    }

    private void PinchEnded(PinchGesture gesture){
        Debug.Log("stopped pinch");
        isPinching = false;
        currentPinchGesture = null;
        gesture.onUpdated -= HandlePinch;
        gesture.onFinished -= PinchEnded;
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
