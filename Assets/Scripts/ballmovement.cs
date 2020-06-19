using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ballmovement : NetworkBehaviour
{
    public float fuerza = 5f;
    public float subida = 3f;
    private Rigidbody rigidbody3d;
    private int botes = 0;
    Vector3 posicionInicio;
    moverpala pala_script;

    public override void OnStartServer()
    {
        base.OnStartServer();
        rigidbody3d = base.GetComponent<Rigidbody>();

        posicionInicio = transform.position;

        // Lanza la pelota hacia el primer jugador
        //rigidbody3d.velocity = Vector2.right * fuerza;
    }

    [ServerCallback]
    void OnCollisionEnter(Collision objeto)
    {
        if (objeto.gameObject.CompareTag("mesa"))
        {
            botes++;
            if (botes > 2)
            {
                print("demasiados botes en la mesa");
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
            reiniciar();
        }

        if (objeto.gameObject.CompareTag("pala")) //la pala se mueve en el plano Y,Z
        {
            botes = 0;
            pala_script = objeto.GetComponent<moverpala>();
            Vector3 direccion = new Vector3(objeto.gameObject.GetComponent<Rigidbody>().rotation.z, 0, 0).normalized;
            Vector3 velocidad = pala_script.getVelocidad();
            velocidad.Scale(new Vector3(0.5f, 0.8f, 0.5f));

            rigidbody3d.velocity = direccion * fuerza + velocidad;
        }
    }

    [ServerCallback]
    void reiniciar()
    {
        botes = 0;
        rigidbody3d.velocity = Vector3.zero;
        transform.position = posicionInicio;
    }
}
