using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableTracking : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StopTracking(){
        transform.SetParent(null, true);
        GameObject imageTracker = GameObject.FindWithTag("ImageTracker");
            if(imageTracker)
                imageTracker.GetComponent<ImageTracker>().RpcImageTracked();
            else
                Debug.Log("BCZ non c'è l'image tracker");
    }
}
