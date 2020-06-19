﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    // Custom NetworkManager that simply assigns the correct racket positions when
    // spawning players. The built in RoundRobin spawn method wouldn't work after
    // someone reconnects (both players would be on the same side).
    [AddComponentMenu("")]
    public class NetworkManagerPinPon : NetworkManager
    {
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;
        public GameObject ball;

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position
            Transform start = numPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
            GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
            NetworkServer.AddPlayerForConnection(conn, player);

            // spawn ball if two players
            if (numPlayers == 2)
            {
                SpawnPelota(leftRacketSpawn);
            }
        }

        public void SpawnPelota(Transform jugador)
        {
            Vector3 posJugador = jugador.transform.position;
            ball = Instantiate(spawnPrefabs.Find(prefab => ball), posJugador + new Vector3(0f, 1f, 0.8f), new Quaternion(1f, 1f, 1f, 1f));
            NetworkServer.Spawn(ball);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            // destroy ball
            if (ball != null)
                NetworkServer.Destroy(ball);

            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
    }
}
