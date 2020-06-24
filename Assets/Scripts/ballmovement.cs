using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ballmovement : NetworkBehaviour
{
    public float fuerza = 5f;
    public float subida = 3f;
    private Rigidbody rigidbody3d;
    private int botes = 0;
    Vector3 posicionInicio;
    moverpala pala_script;
    NetworkManager manager;
    bool jugandoIzda = true;
    int puntuacionIzda = 0, puntuacionDcha = 0;
    public Text puntuacionIzdaText, puntuacionDchaText;

    public override void OnStartServer()
    {
        base.OnStartServer();
        rigidbody3d = base.GetComponent<Rigidbody>();

        posicionInicio = transform.position;


    }

    //[ServerCallback]
    void OnCollisionEnter(Collision objeto)
    {
        if (objeto.gameObject.CompareTag("mesa"))
        {
            botes++;
            if (transform.position.x < 0 && rigidbody3d.velocity.x > 0) //fallo: toca en tu mismo campo
            { 
                puntuacionIzda++;
                reiniciar();
            }
            else if (transform.position.x > 0 && rigidbody3d.velocity.x < 0)
            { 
                puntuacionDcha++;
                reiniciar();
            }

            if (botes > 1 && rigidbody3d.velocity.x > 0) // 2 toques en la mesa y se gana un punto (no debería pasar, es imposible llegar a eso)
            {
                puntuacionDcha++;
                reiniciar();
            }
            else if(botes > 1 && rigidbody3d.velocity.x < 0)
            {
                puntuacionIzda++;
                reiniciar();
            }

    //        Vector3 direccion = new Vector3(0, 1, 0).normalized;
    //        GetComponent<Rigidbody>().AddForce(direccion * fuerza);
        }

    }

    [ClientRpc]
    public void RpcSyncedPos(Vector3 syncedPos)
    {
        transform.position = syncedPos;
    }

    [Server]
    private void UpdateClientsPos()
    {
        RpcSyncedPos(transform.position);
    }

    [ClientRpc]
    private void RpcActualizarMarcador(int a, int b)
    {
        puntuacionIzdaText = GameObject.Find("puntuacionIzda").GetComponent<Text>();
        puntuacionDchaText = GameObject.Find("puntuacionDcha").GetComponent<Text>();
        puntuacionIzdaText.text = a.ToString();
        puntuacionDchaText.text = b.ToString();
    }

    void Update()
    {
        UpdateClientsPos(); //mejorar lag
    }

    [Server]
    void OnTriggerEnter(Collider objeto)
    {
        if (objeto.gameObject.CompareTag("suelo"))
        {
            if (botes == 1 && rigidbody3d.velocity.x > 0) // punto: 2 toques en la mesa y se gana un punto (no debería pasar, es imposible llegar a eso)
            {
                puntuacionDcha++;
                reiniciar();
            }
            else if (botes == 1 && rigidbody3d.velocity.x < 0)
            {
                puntuacionIzda++;
                reiniciar();
            }
            else if (botes == 0 && rigidbody3d.velocity.x > 0) // fallo: fuera
            {
                puntuacionIzda++;
                reiniciar();
            }
            else if (botes == 0 && rigidbody3d.velocity.x < 0)
            {
                puntuacionDcha++;
                reiniciar();
            }
        }

        if (objeto.gameObject.CompareTag("pala")) //la pala se mueve en el plano Y,Z
        {
            rigidbody3d.constraints = RigidbodyConstraints.None;
            botes = 0;
            pala_script = objeto.GetComponent<moverpala>();
            Vector3 direccion = new Vector3(objeto.gameObject.GetComponent<Rigidbody>().rotation.z, 0, 0).normalized;
            Vector3 velocidad = pala_script.getVelocidad();
            velocidad.Scale(new Vector3(0.5f, 0.8f, 0.5f));

            rigidbody3d.velocity = direccion * fuerza + velocidad;
            jugandoIzda = (transform.position.x > 0); //ha golpeado el de la izquierda o no
        }
    }

    [ServerCallback]
    void reiniciar()
    {
        botes = 0;
        rigidbody3d.velocity = Vector3.zero;
        transform.position = posicionInicio;
        jugandoIzda = true;
        rigidbody3d.constraints = RigidbodyConstraints.FreezePosition;

        RpcActualizarMarcador(puntuacionIzda, puntuacionDcha);
    }
}
