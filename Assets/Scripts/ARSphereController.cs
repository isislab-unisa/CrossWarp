using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;

public class ARSphereController : NetworkBehaviour
{
    DesktopSphereController otherPlayer;
    Vector3 startPosition;
    private SubplaneConfig subplaneConfig;
    private XROrigin xROrigin;

    public bool CheckReferenceToDesktopObject(){
        if(otherPlayer != null)
            return otherPlayer.enabled == true;
        return false;
    }

    public void GetReferenceToDesktopObject(){
        PlayerRef otherPlayerRef;
        Debug.Log("BCZ get reference to desktop object called");
        Debug.Log("BCZ Runner? " + Runner != null);
        Debug.Log("BCZ Runner: " + Runner.ActivePlayers);
        Debug.Log("BCZ Runner.ActivePlayers: " + Runner.ActivePlayers);
        foreach(PlayerRef player in Runner.ActivePlayers){
            Debug.Log("BCZ other players: " + player);
            if(player != Runner.LocalPlayer){
                otherPlayerRef = player;
                Debug.Log("BCZ players: " + player);
                NetworkObject playerNetObj;
                if(!Runner.TryGetPlayerObject(player, out playerNetObj)){
                    Debug.Log("BCZ ERRORE NON C'è IL NETWORK OBJECT");
                }
                else{
                    // posso cambiarlo in playernetobj?
                    GameObject otherPlayerObj = Runner.GetPlayerObject(player).gameObject;
                    Debug.Log("BCZ vediamo: " + otherPlayerObj);
                    otherPlayer = otherPlayerObj.GetComponent<DesktopSphereController>();
                }
            }
        }
    }
    
    public override void Spawned(){
        GetReferenceToDesktopObject();
        startPosition = new Vector3(0, 0, 0);
        Debug.Log("BCZ start");
        FindAnyObjectByType<DisplayConnector>().aRSphereController = this;

        if(!PlatformManager.IsDesktop())
            GetComponentInChildren<MeshRenderer>().enabled = true;
    }

    // Update is called once per frame
    public override void FixedUpdateNetwork()
    {
        Camera main = Camera.main;
        if(Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            if(!CheckReferenceToDesktopObject())
                GetReferenceToDesktopObject();
            if(CheckReferenceToDesktopObject()){
                move(main.transform.position, main.transform.rotation);
                //Debug.Log("BCZ Move id: " + Id);
                /*if(touch.phase == TouchPhase.Began)
                    MoveSelectedObject(touch.position);*/
            }
            else{
                Debug.Log("BCZ Manca la reference all'object desktop");
            }
        }
    }

    public void move(Vector3 newPosition, Quaternion newRotation){
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        GameObject selectedSubplane = subplaneConfig.GetSelectedSubplane();

        if(!selectedSubplane)
            return;
            
        if(!xROrigin)
            xROrigin = FindFirstObjectByType<XROrigin>();

        // calcolo la differenza in rotazione tra il subplane e il forward globale
        Quaternion rotation = GetRotationRelativeToSelectedSubplane();

        // calcolo la posizione relativa al subplane
        // selectedSubplane.transform.position should always be the center of the subplane
        Vector3 position = newPosition - selectedSubplane.transform.position;

        // ruoto la posizione in base all'offset in rotazione
        position = rotation * position;
        newRotation *= rotation;

        //otherPlayer.UpdatePositionRpc(position, newRotation, subplaneConfig.isMirror, Runner.LocalPlayer);
        //Debug.Log("Chiamo la updateposition");
        if(HasStateAuthority)
            GetComponent<PhoneRepresentation>().UpdatePosition(position, newRotation, subplaneConfig.isMirror);
            //transform.position = new Vector3(0.5f, 0.5f, 0.5f);
        startPosition = new Vector3(0, startPosition.y + 1);
    }

    public void SendPointToDesktop(Vector3 point, Vector3 direction){
        Debug.Log("BCZ sendpoint id:" + Id);
        direction = GetRotationRelativeToSelectedSubplane() * direction;
        bool isMirror = false;
        if(subplaneConfig)
            isMirror = subplaneConfig.isMirror;
        
        if(!CheckReferenceToDesktopObject())
            GetReferenceToDesktopObject();
        if(CheckReferenceToDesktopObject()){
            otherPlayer.SendRemotePointRpc(point, direction, isMirror, Runner.LocalPlayer, GetComponent<PhoneRepresentation>());
        }
        else{
            Debug.Log("BCZ Manca la reference all'object desktop");
        }
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

    /*public void MoveSelectedObject(Vector2 point){
        if(!subplaneConfig)
            subplaneConfig = FindFirstObjectByType<SubplaneConfig>();
        if(subplaneConfig.IsConfig())
            return;
        
        /*Ray ray = Camera.main.ScreenPointToRay(point);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {*/
            /*
            PhoneRepresentation myPhoneRepresentation = GetComponent<PhoneRepresentation>();//otherPlayer.playersRepresentation[Runner.LocalPlayer].GetComponent<PhoneRepresentation>();
            myPhoneRepresentation.SendLocalPoint(point, Vector3.forward);
            myPhoneRepresentation.networkedSelectedObject.GetComponent<MovableObject>().controlledByAR = true;
        //}
    }*/ 
}
