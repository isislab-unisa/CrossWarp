using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class GestureManager : NetworkBehaviour
{
    private PhoneRepresentation phoneRepresentation;
    private SubplaneConfig subplaneConfig;
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
            arGestureInteractor = FindObjectOfType<ARGestureInteractor>();
            phoneRepresentation = GetComponent<PhoneRepresentation>();

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

    void Update()
    {
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        // se non è ancora stato creato un subplane non vogliamo gestire i touch del giocatore
        if(!subplaneConfig || !subplaneConfig.GetSelectedSubplane())
            return;
        
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

    // TAP GESTURE HANDLING

    public void TapGestureStart(TapGesture gesture){
        Debug.LogWarning("Chiamata HandleARTap");
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        // se non è ancora stato creato un subplane non vogliamo gestire i touch del giocatore
        if(!subplaneConfig || !subplaneConfig.GetSelectedSubplane())
            return;
        
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

        LayerMask layerMask = LayerMask.GetMask("MovableObjectsPhysics");
        if(layerMask == -1){
            Debug.LogError("Il LayerMask MovabeleObjectsPhysics non è definito");
        }
        layerMask = ~layerMask;

        Ray ray = Camera.main.ScreenPointToRay(gesture.startPosition);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5f);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Vector3 direction = ray.direction;
            Debug.Log("BCZ colpisco, " + hit.transform.gameObject.name);
            Subplane hittedSubplane = hit.transform.gameObject.GetComponent<Subplane>();
            if(hittedSubplane){
                Debug.Log("Hit display global: " + hit.point);
                Debug.Log("Hit display local: " + hit.transform.InverseTransformPoint(hit.point));
                Vector3 cameraMappedPoint = hittedSubplane.NormalizedHitPoint(hit.transform.InverseTransformPoint(hit.point));
                
                phoneRepresentation.RaycastFromVRCameraRPC(cameraMappedPoint, direction, subplaneConfig.isMirror);
            }
            else{
                phoneRepresentation.SendLocalPoint(hit, direction);
                Debug.Log("Cliccato fuori");
            }
        }
    }

    // DRAG GESTURE HANDLING

    // controllare se il drag inizia colpendo l'oggetto che è selezionato, se no non fare nulla
    private void DragGestureStart(DragGesture dragGesture){
        if(!phoneRepresentation.networkedSelectedObject)
            return;
        dragGestureLastUpdate = Time.deltaTime;
        isDragging = true;
        currentDragGesture = dragGesture;
        LayerMask layerMask = LayerMask.GetMask("MovableObjectsPhysics");
        if(layerMask == -1){
            Debug.LogError("Il LayerMask MovabeleObjects non è definito");
        }
        // esclusione del layer di movableobjects
        layerMask = ~layerMask;
        Ray ray = Camera.main.ScreenPointToRay(dragGesture.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,layerMask))
        {
            if(hit.collider.GetComponent<Subplane>())
                dragGesture.onUpdated += HandleDragOnSubplane;
            else if(hit.collider.GetComponent<MovableObject>() == phoneRepresentation.networkedSelectedObject)
                dragGesture.onUpdated += HandleARDrag;
        }

        dragGesture.onFinished += ARDragEnded;
        
    }

    // spostare l'oggetto selezionato nel punto colpito dal raycast
    private async void HandleARDrag(DragGesture dragGesture)
    {
        if(!phoneRepresentation.networkedSelectedObject)
            return;
        LayerMask layerMask = LayerMask.GetMask("MovableObjects", "MovableObjectsPhysics");
        if(layerMask == -1){
            Debug.LogError("Il LayerMask MovabeleObjects non è definito");
        }
        // esclusione del layer di movableobjects
        layerMask = ~layerMask;
        Ray ray = Camera.main.ScreenPointToRay(dragGesture.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,layerMask))
        {
            if(!phoneRepresentation.networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                await phoneRepresentation.networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
            Debug.Log("" + Runner.LocalPlayer + " hasSA: " + phoneRepresentation.networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority);
            phoneRepresentation.networkedSelectedObject.UpdatePosition(hit.point);
        }
        Debug.Log($"Drag da {dragGesture.startPosition} a {dragGesture.position}");
        currentDragGesture = dragGesture;
    }

    private async void HandleDragOnSubplane(DragGesture gesture){
        if(!phoneRepresentation.networkedSelectedObject)
            return;
        LayerMask layerMask = LayerMask.GetMask("MovableObjects", "MovableObjectsPhysics");
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
                    phoneRepresentation.ContinousRaycastFromVRCameraRPC(cameraMappedPoint);
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

        if(phoneRepresentation.networkedSelectedObject){
            LayerMask layerMask = LayerMask.GetMask("MovableObjects", "MovableObjectsPhysics");
            if(layerMask == -1){
                Debug.LogError("Il LayerMask MovabeleObjects non è definito");
            }
            // esclusione del layer di movableobjects
            layerMask = ~layerMask;
            Ray ray = Camera.main.ScreenPointToRay(dragGesture.position);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,layerMask))
            {
                if(hit.collider.GetComponent<Subplane>()){
                    phoneRepresentation.networkedSelectedObject.GetComponent<TransitionManager>().StartPushInTransitionOnScreen();
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
        if(!phoneRepresentation.networkedSelectedObject)
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
        if(!phoneRepresentation.networkedSelectedObject)
            return;
        isTwoFingerDragging = false;
        currentTwoFingerDragGesture = null;
        gesture.onUpdated -= HandleTwoFingerDrag;
        gesture.onFinished -= TwoFingerEnded;
        if(isPinching || isTwisting)
            return;
        if((gesture.startPosition1 + gesture.startPosition2 / 2).y + 5f < gesture.position.y){
            Debug.LogWarning("push in");
            phoneRepresentation.networkedSelectedObject.GetComponent<TransitionManager>().StartPushInTransition();
        }
        else if((gesture.startPosition1 + gesture.startPosition2 / 2).y + 5f > gesture.position.y){
            Debug.LogWarning("pull out");
            phoneRepresentation.networkedSelectedObject.GetComponent<TransitionManager>().StartPullOutTransitionRPC();
        }
    }

    //TWIST GESTURE HANDLING

    private void TwistGestureStart(TwistGesture gesture){
        if(!phoneRepresentation.networkedSelectedObject)
            return;
        twistGestureLastUpdate = Time.deltaTime;
        isTwisting = true;
        currentTwistGesture = gesture;
        gesture.onUpdated += HandleTwist;
        gesture.onFinished += TwistEnded;
    }

    private async void HandleTwist(TwistGesture gesture){
        //Debug.Log($"TwoFDrag da {gesture.startPosition1} a {gesture.position}");
        if(!phoneRepresentation.networkedSelectedObject)
            return;
        if(isPinching)
            return;
        Debug.LogWarning("Rotation: " + gesture.deltaRotation);
        // non c'è bisogno di controllare che la platform sia mobile, le gesture vengono riconosciute solo su mobile
        // se si trova nel mondo aumentato
        if(phoneRepresentation.networkedSelectedObject.worldState == MovableObjectState.inAR){
            phoneRepresentation.networkedSelectedObject.UpdateRotation(-gesture.deltaRotation * twistGestureRotateRate);
        }
        // altrimenti se è nel mondo virtuale
        else if(phoneRepresentation.networkedSelectedObject.worldState == MovableObjectState.inVR){
            if(!phoneRepresentation.networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                await phoneRepresentation.networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
            phoneRepresentation.networkedSelectedObject.UpdateRotation(-gesture.deltaRotation * twistGestureRotateRate);
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
        if(!phoneRepresentation.networkedSelectedObject)
            return;
        pinchGestureLastUpdate = Time.deltaTime;
        isPinching = true;
        currentPinchGesture = gesture;
        gesture.onUpdated += HandlePinch;
        gesture.onFinished += PinchEnded;
    }

    private async void HandlePinch(PinchGesture gesture){
        //Debug.Log($"TwoFDrag da {gesture.startPosition1} a {gesture.position}");
        if(!phoneRepresentation.networkedSelectedObject)
            return;
            
        if(isTwisting)
            return;
        // non c'è bisogno di controllare che la platform sia mobile, le gesture vengono riconosciute solo su mobile
        // se si trova nel mondo aumentato
        if(phoneRepresentation.networkedSelectedObject.worldState == MovableObjectState.inAR){
            Debug.LogWarning("gapDelta: " + gesture.gapDelta);
            Debug.LogWarning("resize: " + GetScaleByPinchGap(gesture.gapDelta * pinchGestureScaleRate));
            phoneRepresentation.networkedSelectedObject.UpdateScale(GetScaleByPinchGap(gesture.gapDelta * pinchGestureScaleRate));
        }
        // altrimenti se è nel mondo virtuale
        else if(phoneRepresentation.networkedSelectedObject.worldState == MovableObjectState.inVR){
            if(!phoneRepresentation.networkedSelectedObject.GetComponent<NetworkObject>().HasStateAuthority)
                await phoneRepresentation.networkedSelectedObject.GetComponent<NetworkObject>().WaitForStateAuthority();
            Debug.LogWarning("gapDelta: " + gesture.gapDelta);
            Debug.LogWarning("resize: " + GetScaleByPinchGap(gesture.gapDelta * pinchGestureScaleRate));
            phoneRepresentation.networkedSelectedObject.UpdateScale(GetScaleByPinchGap(gesture.gapDelta * pinchGestureScaleRate));
        }
    }

    private Vector3 GetScaleByPinchGap(float gap){
        float scaleValue = phoneRepresentation.networkedSelectedObject.transform.localScale.x;
        float max = phoneRepresentation.networkedSelectedObject.maxScale;
        float min = phoneRepresentation.networkedSelectedObject.minScale;

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
}
