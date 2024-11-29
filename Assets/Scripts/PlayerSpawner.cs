using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;
    public GameObject ImagePrefab;


    public void PlayerJoined(PlayerRef player)
    {
        GameObject objToSpawn = PlayerPrefab;
        NetworkObject spawned = null;
        if (player == Runner.LocalPlayer)
        {
            if(PlatformManager.IsDesktop()){
                /*Runner.Spawn(ImagePrefab,  new Vector3(0, 1, 0), Quaternion.identity);
                return;*/
                spawned = Runner.Spawn(objToSpawn, new Vector3(0, 1, 0), Quaternion.identity);
                Runner.SetPlayerObject(Runner.LocalPlayer, spawned);
                Debug.Log("BCZ disabilito AR controller");
                spawned.gameObject.GetComponent<ARSphereController>().enabled = false;
                return;
            }
            spawned = Runner.Spawn(objToSpawn, new Vector3(0, 1, 0), Quaternion.identity);
            Runner.SetPlayerObject(Runner.LocalPlayer, spawned);
            Debug.Log("BCZ disabilito desktop controller");
            spawned.gameObject.GetComponent<DesktopSphereController>().enabled = false;
        }
    }
}
