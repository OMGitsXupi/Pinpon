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
    Text puntuacionIzdaText, puntuacionDchaText;
    AudioSource sound;
    Vector3 spawnIzda, spawnDcha;

    public override void OnStartServer()
    {
        base.OnStartServer();
        rigidbody3d = base.GetComponent<Rigidbody>();

        spawnIzda = GameObject.Find("SpawnPalaIzda").transform.position + new Vector3(0f, 1f, 0.7f);
        spawnDcha = GameObject.Find("SpawnPalaDcha").transform.position + new Vector3(0f, 1f, -0.7f);
        posicionInicio = spawnIzda;
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
    private void RpcReproducirPing()
    {
        if (Random.Range(0, 2) == 0)
        {
            sound = GetComponents<AudioSource>()[0];
            sound.Play();
        }
        else
        {
            sound = GetComponents<AudioSource>()[1];
            sound.Play();
        }
    }

    [ClientRpc]
    private void RpcReproducirPong()
    {
        if (Random.Range(0, 2) == 0)
        {
            sound = GetComponents<AudioSource>()[2];
            sound.Play();
        }
        else
        {
            sound = GetComponents<AudioSource>()[3];
            sound.Play();
        }
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

    //[ServerCallback]
    void OnCollisionEnter(Collision objeto)
    {
        if (objeto.gameObject.CompareTag("mesa"))
        {
            RpcReproducirPong();
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

    [Server]
    void OnTriggerEnter(Collider objeto)
    {
        if (objeto.gameObject.CompareTag("suelo"))
        {
            RpcReproducirPong();
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
            RpcReproducirPing();
            rigidbody3d.constraints = RigidbodyConstraints.None;
            botes = 0;
            pala_script = objeto.GetComponent<moverpala>();
            Vector3 direccion = new Vector3(objeto.gameObject.GetComponent<Rigidbody>().rotation.z, 0, 0).normalized;
            Vector3 velocidad = pala_script.getVelocidad();
            velocidad.Scale(new Vector3(0f, 0f, 0.5f));
            Vector3 velocidadSubida = new Vector3(0f, subida, 0f);

            rigidbody3d.velocity = direccion * fuerza + velocidad + velocidadSubida;
            jugandoIzda = (transform.position.x > 0); //ha golpeado el de la izquierda o no
        }
    }

    [ServerCallback]
    void reiniciar()
    {
        botes = 0;
        rigidbody3d.velocity = Vector3.zero;
        if ((puntuacionDcha + puntuacionIzda) % 2 == 0)
            CambiarLado();
        transform.position = posicionInicio;
        jugandoIzda = true;
        rigidbody3d.constraints = RigidbodyConstraints.FreezePosition;

        RpcActualizarMarcador(puntuacionIzda, puntuacionDcha);
    }

    void CambiarLado() 
    {
        if (posicionInicio == spawnDcha)
            posicionInicio = spawnIzda;
        else posicionInicio = spawnDcha;
    }
}
