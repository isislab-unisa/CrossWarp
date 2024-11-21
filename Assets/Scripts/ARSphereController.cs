using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class ARSphereController : NetworkBehaviour
{
    DesktopSphereController otherPlayer;
    Vector3 startPosition;

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
    }

    // Update is called once per frame
    void Update()
    {
        Camera main = Camera.main;
        if(Input.touchCount > 0){
            if(!CheckReferenceToDesktopObject())
                GetReferenceToDesktopObject();
            if(CheckReferenceToDesktopObject()){
                move(main.transform.position);
                //Debug.Log("BCZ Move id: " + Id);
            }
            else{
                //Debug.Log("BCZ Manca la reference all'object desktop");
            }
        }
    }

    public void move(Vector3 newPosition){
            //Debug.Log("BCZ tocco otherplayer: " + otherPlayer.transform.position);
            //Debug.Log("BCZ newposition: " + newPosition);
            Vector3 prova = new Vector3(newPosition.x*10, newPosition.y*10, otherPlayer.transform.position.z);
            otherPlayer.UpdatePositionRpc(newPosition);
            startPosition = new Vector3(0, startPosition.y + 1);
    }

    public void SendPointToDesktop(Vector3 point, Vector3 direction){
        Debug.Log("BCZ sendpoint id:" + Id);
        
        if(!CheckReferenceToDesktopObject())
            GetReferenceToDesktopObject();
        if(CheckReferenceToDesktopObject()){
            otherPlayer.SendRemotePointRpc(point, direction);
        }
        else{
            Debug.Log("BCZ Manca la reference all'object desktop");
        }
    }
}
