using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class ImageTracker : NetworkBehaviour
{
    public GameObject PlayerPrefab;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcImageTracked(){
        Debug.Log("BCZ Image Tracked!");
        NetworkObject spawned = null;
        spawned = Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        Runner.SetPlayerObject(Runner.LocalPlayer, spawned);
        spawned.gameObject.GetComponent<ARSphereController>().enabled = false;
        Destroy(gameObject);
    }
}
