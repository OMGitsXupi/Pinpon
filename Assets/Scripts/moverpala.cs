using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class moverpala : NetworkBehaviour
{
    Vector3 posPrevia, velocidad;
    private Vector3 screenPoint;
    private Vector3 offset;
    public float fuerza=5f;
    public float subida = 3f;

    public void Start()
    {
        
    }

    [Client]
    void OnMouseDown()
    {
        if (!gameObject.transform.parent.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
            return;

        screenPoint = Camera.current.WorldToScreenPoint(transform.position);
        offset = transform.position - Camera.current.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    [Client]
    void OnMouseDrag()
    {
        if (!gameObject.transform.parent.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
            return;

        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.current.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = new Vector3(transform.position.x, curPosition.y, curPosition.z) ;
    }
 
    void Update()
    {
        velocidad = ((transform.position - posPrevia)) / Time.deltaTime;
        posPrevia = transform.position;

        print(velocidad);
        print(gameObject.GetComponent<Rigidbody>().velocity);
    }

    public Vector3 getVelocidad()
    {
        return velocidad;
    }
}
