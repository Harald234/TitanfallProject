using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
//using static UnityEditor.FilePathAttribute;
using Unity.VisualScripting;

public class BotMaster : NetworkBehaviour
{
    public Camera cam;
    RaycastHit hit;

    bool isFollowing;
    bool isLooking;

    public GameObject bot;
    List<GameObject> spawnedBots = new List<GameObject> { };
    int i = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            GetComponent<BotMaster>().enabled = false;
        }
    }


    // Update is called once per frame
    void Update()
    {
        HandleInput();

        if (isFollowing)
        {
            ServerFollowPlayer(transform, spawnedBots);
        }

        if (isLooking)
        {
            ServerLook(transform, spawnedBots);
        }
    }

    void SpawnBot()
    {
        ServerSpawnBot(bot, transform, this);
    }


    [ServerRpc]
    void ServerSpawnBot(GameObject bot, Transform player, BotMaster script)
    {
        GameObject botSpawn = Instantiate(bot, player.position + player.forward * 5f, Quaternion.identity);
        base.Spawn(botSpawn, base.Owner);

        SetBot(botSpawn, script);
    }

    [ObserversRpc]
    void SetBot(GameObject bot, BotMaster script)
    {
        script.spawnedBots.Add(bot);
        bot.GetComponent<ControllBot>().Move(transform.position);
    }


    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            SpawnBot();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (isFollowing == true)
                isFollowing = false;
            else
                isFollowing = true;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            isFollowing = false;
            ServerMoveBot(cam.transform, spawnedBots);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            ServerStay(spawnedBots);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ServerFindRandomPoint(spawnedBots);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isLooking == true)
                isLooking = false;
            else
                isLooking = true;
        }
    }


    [ServerRpc]
    void ServerFindRandomPoint(List<GameObject> spawnedBots)
    {
        FindRandomPoint(spawnedBots);
    }

    [ObserversRpc]
    void FindRandomPoint(List<GameObject> spawnedBots)
    {
        for (int i = 0; i < spawnedBots.Count; i++)
        {
            if (spawnedBots[i].GetComponent<ControllBot>().findRandomPoint == false)
                spawnedBots[i].GetComponent<ControllBot>().findRandomPoint = true;
            else
                spawnedBots[i].GetComponent<ControllBot>().findRandomPoint = false;
        }
    }


    [ServerRpc]
    void ServerLook(Transform player, List<GameObject> spawnedBots)
    {
        BotLook(player, spawnedBots);
    }

    [ObserversRpc]
    void BotLook(Transform player, List<GameObject> spawnedBots)
    {
        for (int i = 0; i < spawnedBots.Count; i++)
        {
            spawnedBots[i].GetComponent<ControllBot>().LookAtPlayer(player.position);
        }
    }



    [ServerRpc]
    void ServerMoveBot(Transform camera, List<GameObject> spawnedBots)
    {
        MoveBot(camera, spawnedBots);
    }

    [ObserversRpc]
    void MoveBot(Transform camera, List<GameObject> spawnedBots)
    {
        Physics.Raycast(camera.position, camera.forward, out hit, 100f);
        Vector3 location = hit.point;
        if (location != null)
        {

            for (int i = 0; i < spawnedBots.Count; i++)
            {
                spawnedBots[i].GetComponent<ControllBot>().Move(location);
            }

        }
    }


    [ServerRpc]
    void ServerFollowPlayer(Transform player, List<GameObject> spawnedBots)
    {
        FollowPlayer(player, spawnedBots);
    }

    [ObserversRpc]
    void FollowPlayer(Transform player, List<GameObject> spawnedBots)
    {
        for (int i = 0; i < spawnedBots.Count; i++)
        {
            spawnedBots[i].GetComponent<ControllBot>().Move(player.position);
        }
    }


    [ServerRpc]
    void ServerStay(List<GameObject> spawnedBots)
    {
        Stay(spawnedBots);
    }

    [ObserversRpc]
    void Stay(List<GameObject> spawnedBots)
    {
        for (int i = 0; i < spawnedBots.Count; i++)
        {
            if (spawnedBots[i].GetComponent<ControllBot>().stay == false)
                spawnedBots[i].GetComponent<ControllBot>().stay = true;
            else
                spawnedBots[i].GetComponent<ControllBot>().stay = false;
        }
    }
}
