using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;
using static UnityEngine.XR.Interaction.Toolkit.AR.GestureTransformationUtility;

public class CustomARTranslateInteractable : ARBaseGestureInteractable
{
    [SerializeField]
    [Tooltip("Controls whether Unity constrains the object vertically, horizontally, or free to move in all axes.")]
    GestureTransformationUtility.GestureTranslationMode m_ObjectGestureTranslationMode;

    /// <summary>
    /// Controls whether the object will be constrained vertically, horizontally, or free to move in all axis.
    /// </summary>
    public GestureTransformationUtility.GestureTranslationMode objectGestureTranslationMode
    {
        get => m_ObjectGestureTranslationMode;
        set => m_ObjectGestureTranslationMode = value;
    }

    [SerializeField]
    [Tooltip("The maximum translation distance of this object.")]
    float m_MaxTranslationDistance = 10f;

    /// <summary>
    /// The maximum translation distance of this object.
    /// </summary>
    public float maxTranslationDistance
    {
        get => m_MaxTranslationDistance;
        set => m_MaxTranslationDistance = value;
    }

    [SerializeField]
    [Tooltip("The LayerMask that Unity uses during an additional ray cast when a user touch does not hit any AR trackable planes.")]
    LayerMask m_FallbackLayerMask;

    /// <summary>
    /// The <see cref="LayerMask"/> that Unity uses during an additional ray cast
    /// when a user touch does not hit any AR trackable planes.
    /// </summary>
    public LayerMask fallbackLayerMask
    {
        get => m_FallbackLayerMask;
        set => m_FallbackLayerMask = value;
    }

    [SerializeField]
    [Tooltip("If true the ray used for raycast is generated starting from the finger, otherwise it will start from the center of the screen.")]
    bool m_RayFromFinger = true;

    public bool RayFromFinger
    {
        get => m_RayFromFinger;
        set => m_RayFromFinger = value;
    }

    [SerializeField]
    [Tooltip("The distance from the origin of the ray at which the interactable will be moved.")]
    float m_RayDistance = 0.2f;
    public float RayDistance
    {
        get => m_RayDistance;
        set => m_RayDistance = value;
    }

    const float k_PositionSpeed = 12f;
    const float k_DiffThreshold = 0.0001f;

    bool m_IsActive;

    Vector3 m_DesiredLocalPosition;
    Vector3 m_DesiredWorldPosition;
    float m_GroundingPlaneHeight;
    Vector3 m_DesiredAnchorPosition;
    Quaternion m_DesiredRotation;
    GestureTransformationUtility.Placement m_LastPlacement;
    GestureTransformationUtility.Placement m_CurrentPlacement;

    /// <inheritdoc />
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            UpdatePosition();
    }

    /// <inheritdoc />
    protected override bool CanStartManipulationForGesture(DragGesture gesture)
    {
        // If the gesture isn't targeting this item, don't start manipulating.
        return gesture.targetObject != null && gesture.targetObject == gameObject;
    }

    /// <inheritdoc />
    protected override void OnStartManipulation(DragGesture gesture)
    {
        
        // non mi serve
        //m_GroundingPlaneHeight = transform.parent.position.y;
        m_CurrentPlacement = new Placement();
        m_CurrentPlacement.hasHoveringPosition = false;
        m_CurrentPlacement.hasPlane = false;
        m_CurrentPlacement.hasPlacementPosition = true;
        m_CurrentPlacement.placementPosition = transform.position;
    }

    /// <inheritdoc />
    protected override void OnContinueManipulation(DragGesture gesture)
    {
        // non bisogna avere per forza il parent, non mi serve
        /*if (transform.parent == null)
        {
            Debug.LogError("Translation Interactable needs a parent object.", this);
            return;
        }*/

        m_IsActive = true;

        m_LastPlacement = m_CurrentPlacement;

        var desiredPlacement = GetPlacementPosition(gesture.position);
        //m_LastPlacement = desiredPlacement;
        m_DesiredRotation = desiredPlacement.placementRotation;
        m_DesiredAnchorPosition = desiredPlacement.placementPosition;
        m_DesiredWorldPosition = desiredPlacement.placementPosition;

        /*var desiredPlacement = xrOrigin != null
            ? GestureTransformationUtility.GetBestPlacementPosition(
                transform.parent.position, gesture.position, m_GroundingPlaneHeight, 0.03f,
                maxTranslationDistance, objectGestureTranslationMode, xrOrigin, fallbackLayerMask: m_FallbackLayerMask)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility.
            : GestureTransformationUtility.GetBestPlacementPosition(
                transform.parent.position, gesture.position, m_GroundingPlaneHeight, 0.03f,
                maxTranslationDistance, objectGestureTranslationMode, arSessionOrigin, fallbackLayerMask: m_FallbackLayerMask);
#pragma warning restore 618

        if (desiredPlacement.hasHoveringPosition && desiredPlacement.hasPlacementPosition)
        {
            // If desired position is lower than current position, don't drop it until it's finished.
            m_DesiredLocalPosition = transform.parent.InverseTransformPoint(desiredPlacement.hoveringPosition);
            m_DesiredAnchorPosition = desiredPlacement.placementPosition;

            m_GroundingPlaneHeight = desiredPlacement.updatedGroundingPlaneHeight;

            // Rotate if the plane direction has changed.
            if (((desiredPlacement.placementRotation * Vector3.up) - transform.up).magnitude > k_DiffThreshold)
                m_DesiredRotation = desiredPlacement.placementRotation;
            else
                m_DesiredRotation = transform.rotation;

            if (desiredPlacement.hasPlane)
                m_LastPlacement = desiredPlacement;
        }*/
    }

    private Placement GetPlacementPosition(Vector2 gesturePosition){
        Placement result = default(Placement);
        
        if(m_RayFromFinger){
            Ray ray = Camera.main.ScreenPointToRay(gesturePosition);
            Debug.Log("ray.direction: " + ray.direction);
            Vector3 placementPosition = ray.GetPoint(m_RayDistance); //ray.direction;
            
            Debug.Log("placementPosition: " + placementPosition);
            result.hasHoveringPosition = false;
            result.hasPlacementPosition = true;
            result.hasPlane = false;
            result.placementPosition = placementPosition;
            result.placementRotation = Quaternion.identity;
        }
        else{
            Vector3 placementPosition = Camera.main.transform.position + Camera.main.transform.forward*m_RayDistance;
            
            Debug.Log("placementPosition: " + placementPosition);
            result.hasHoveringPosition = false;
            result.hasPlacementPosition = true;
            result.hasPlane = false;
            result.placementPosition = placementPosition;
            result.placementRotation = Quaternion.identity;
        }

        return result;
    }

    /// <inheritdoc />
    protected override void OnEndManipulation(DragGesture gesture)
    {
        if (!m_LastPlacement.hasPlacementPosition)
            return;

        // non mi interessano le anchor in questo momento
        //var oldAnchor = transform.parent.gameObject;
        // forse in rotation ci va m_desiredRotation
        var desiredPose = new Pose(m_DesiredWorldPosition, m_LastPlacement.placementRotation);

        //-------------------- TODO distanza massima

        /*var desiredLocalPosition = transform.parent.InverseTransformPoint(desiredPose.position);
        
        if (desiredLocalPosition.magnitude > maxTranslationDistance)
            desiredLocalPosition = desiredLocalPosition.normalized * maxTranslationDistance;
        desiredPose.position = transform.parent.TransformPoint(desiredLocalPosition);
        */

        /*var anchor = new GameObject("PlacementAnchor").transform;
        anchor.position = m_LastPlacement.placementPosition;
        anchor.rotation = m_LastPlacement.placementRotation;
        transform.parent = anchor;*/

        //Destroy(oldAnchor);

        m_DesiredLocalPosition = Vector3.zero;

        // Rotate if the plane direction has changed.
        if (((desiredPose.rotation * Vector3.up) - transform.up).magnitude > k_DiffThreshold)
            m_DesiredRotation = desiredPose.rotation;
        else
            m_DesiredRotation = transform.rotation;

        // Make sure position is updated one last time.
        m_IsActive = true;
    }

    void UpdatePosition()
    {
        if (!m_IsActive)
            return;

        // Lerp position.
        var oldWorldPosition = transform.position;
        var newWorldPosition = Vector3.Lerp(
            oldWorldPosition, m_DesiredWorldPosition, Time.deltaTime * k_PositionSpeed);

        var diffLength = (m_DesiredWorldPosition - newWorldPosition).magnitude;
        if (diffLength < k_DiffThreshold)
        {
            newWorldPosition = m_DesiredWorldPosition;
            m_IsActive = false;
        }

        transform.position = newWorldPosition;
        m_CurrentPlacement.placementPosition = transform.position;

        // Lerp rotation.
        var oldRotation = transform.rotation;
        var newRotation =
            Quaternion.Lerp(oldRotation, m_DesiredRotation, Time.deltaTime * k_PositionSpeed);
        transform.rotation = newRotation;
    }

}
