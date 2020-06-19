using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class spawnJugador : NetworkBehaviour
{

    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.enabled = !false;
        if (!isLocalPlayer)
        {
            if (cam != null) cam.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
