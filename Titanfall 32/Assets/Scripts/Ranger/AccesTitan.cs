using FishNet.Object;
using FishNet.Component.Animating;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using FishNet.Connection;

public class AccesTitan : NetworkBehaviour
{
    public GameObject titan;

    [NonSerialized] public GameObject spawnedTitan;

    /*GameObject[] titanDropPoints;
    float shortestDistance = 0f;
    Transform chosenPoint;*/

    //you have two of this variable bad idea
    bool inTitan;

    PlayerHealth healthScript;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            healthScript = GetComponentInChildren<PlayerHealth>();
        }
        else
        {
            GetComponent<AccesTitan>().enabled = false;
        }
    }

    public void Update()
    {

        StartTitanFall();
        EmbarkWithTitan();
        UnembarkFromTitan();
        EjectFromTitan();

        if (inTitan && spawnedTitan.GetComponent<EnterVanguardTitan>().dead == true)
        {
            healthScript.Damage(100f, 4f);
        }
    }

    void StartTitanFall()
    {

        if (Input.GetKeyDown(KeyCode.V) && spawnedTitan == null)
        {
            //MoveTitanToDropLocation();
            SpawnTitan(titan, transform, this);
        }
    }

    /*private void MoveTitanToDropLocation()
    {
        for (int i = 0; i < titanDropPoints.Length; i++)
        {
            float distance = Vector3.Distance(titanDropPoints[i].transform.position, this.transform.position);
            if (distance < shortestDistance || shortestDistance == 0f)
            {
                shortestDistance = distance;
                chosenPoint = titanDropPoints[i].transform;
            }
        }
        //trying to move the titan to a specific drop point, does not work, would be good if you cold spawn the titan at this point
        TitanObject.transform.position = chosenPoint.transform.position;
    }*/


    void EmbarkWithTitan()
    {
        if (Input.GetKeyDown(KeyCode.F) && spawnedTitan != null && spawnedTitan.GetComponent<EnterVanguardTitan>().inRangeForEmbark && !inTitan && spawnedTitan.GetComponent<EnterVanguardTitan>().dead == false)
        {
            StartCoroutine(HandleEmbark());
        }
    }


    void UnembarkFromTitan()
    {
        if (Input.GetKeyDown(KeyCode.B) && inTitan)
        {
            UnembarkServer(spawnedTitan, gameObject);
        }
    }

    void EjectFromTitan()
    {
        if (Input.GetKeyDown(KeyCode.J) && inTitan)
        {
            ServerEject(spawnedTitan, gameObject);
        }
    }

    IEnumerator HandleEmbark()
    {
        StartCoroutine(spawnedTitan.GetComponent<EnterVanguardTitan>().EmbarkCamera());
        StartEmbarkServer(spawnedTitan, gameObject);
        //StartEmbark(gameObject, spawnedTitan,"embark");

        yield return new WaitForSeconds(2.4f);

        EndEmbarkServer(spawnedTitan, gameObject);
        //EndEmbark(spawnedTitan, gameObject);
    }

    [ServerRpc]
    public void SpawnTitan(GameObject titan, Transform player, AccesTitan script)
    {
        GameObject Titan = GameObject.Instantiate(titan, player.position + player.forward * 5f + player.up * 169f, titan.transform.rotation);
        ServerManager.Spawn(Titan, base.Owner);
        SetTitanObject(Titan, script);
    }

    [ObserversRpc]
    public void SetTitanObject(GameObject Titan, AccesTitan script)
    {
        script.spawnedTitan = Titan;
        //Titan.transform.Rotate(0, 90f, 0);
        Titan.GetComponent<EnterVanguardTitan>().StartFall();
    }

    //embark happens in entervanguardtitan

    [ServerRpc]
    public void StartEmbarkServer(GameObject titan, GameObject player)
    {
        StartEmbark(titan, player);
        
    }

    [ServerRpc]
    public void EndEmbarkServer(GameObject titan, GameObject player)
    {
        EndEmbark(titan, player);
    }

    [ObserversRpc]
    public void StartEmbark(GameObject titan, GameObject player)
    {
        StartCoroutine(titan.GetComponent<EnterVanguardTitan>().Embark());

        player.GetComponent<AccesTitan>().transform.GetChild(1).gameObject.SetActive(false);

        player.GetComponent<RangerMovement>().lookTarget = titan.GetComponent<EnterVanguardTitan>().embarkLookTarget.position;
        player.GetComponent<RangerMovement>().canMove = false;
        player.GetComponent<RangerMovement>().embarking = true;
        player.GetComponent<RangerMovement>().embarkPos = titan.GetComponent<EnterVanguardTitan>().embarkPos.position;
        player.GetComponent<RangerMovement>().GetComponent<NetworkAnimator>().SetTrigger("embark");

        /*StartCoroutine(enterSript.Embark());

        moveScript.lookTarget = enterSript.embarkLookTarget.position;
        moveScript.canMove = false;
        moveScript.embarking = true;
        moveScript.embarkPos = enterSript.embarkPos.position;
        animator.SetTrigger("embark");

        cameraObject.SetActive(false);*/
    }

    [ObserversRpc]
    public void EndEmbark(GameObject titan, GameObject player)
    {
        player.GetComponent<AccesTitan>().transform.GetChild(0).gameObject.SetActive(false);

        player.transform.parent = titan.transform;
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<RangerMovement>().embarking = false;
        inTitan = true;
        //TakeOwnershipOfTitan(titan.GetComponent<NetworkObject>());

        /*playerChild.SetActive(false);

        player.transform.parent = titan.transform;
        controller.enabled = false;
        moveScript.embarking = false;
        inTitan = true;
        TakeOwnershipOfTitan(networkOb);*/
    }

    [ServerRpc]
    public void ServerEject(GameObject titan, GameObject player)
    {
        Eject(titan, player);
    }

    [ObserversRpc]
    public void Eject(GameObject titan, GameObject player)
    {
        inTitan = false;

        player.transform.parent = null;

        player.GetComponent<AccesTitan>().transform.GetChild(0).gameObject.SetActive(true);
        player.GetComponent<AccesTitan>().transform.GetChild(1).gameObject.SetActive(true);

        player.GetComponent<AccesTitan>().GetComponentInChildren<Camera>().enabled = true;
        player.GetComponent<AccesTitan>().GetComponentInChildren<AudioListener>().enabled = true;
        titan.GetComponent<EnterVanguardTitan>().ExitTitan();
        //RemoveOwnershipOfTitan(titan.GetComponent<NetworkObject>());

        player.GetComponent<RangerMovement>().canMove = true;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<RangerMovement>().GetComponent<NetworkAnimator>().SetTrigger("exitTitan");

        titan.GetComponent<EnterVanguardTitan>().Eject();
        player.GetComponent<RangerMovement>().Eject();
    }


    [ServerRpc]
    public void UnembarkServer(GameObject titan, GameObject player)
    {
        Unembark(titan, player);
    }

    [ObserversRpc]
    public void Unembark(GameObject titan, GameObject player)
    {
        inTitan = false;

        player.transform.parent = null;

        player.GetComponent<AccesTitan>().transform.GetChild(0).gameObject.SetActive(true);
        player.GetComponent<AccesTitan>().transform.GetChild(1).gameObject.SetActive(true);

        player.GetComponent<AccesTitan>().GetComponentInChildren<Camera>().enabled = true;
        player.GetComponent<AccesTitan>().GetComponentInChildren<AudioListener>().enabled = true;
        titan.GetComponent<EnterVanguardTitan>().ExitTitan();
        RemoveOwnershipOfTitan(titan.GetComponent<NetworkObject>());

        player.GetComponent<RangerMovement>().canMove = true;
        player.transform.position = titan.transform.position + new Vector3(0f, 4f, 3f);
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<RangerMovement>().GetComponent<NetworkAnimator>().SetTrigger("exitTitan");
    }

    [ServerRpc]
    private void TakeOwnershipOfTitan(NetworkObject titan)
    {
        titan.GiveOwnership(base.Owner);
    }

    [ServerRpc]
    private void RemoveOwnershipOfTitan(NetworkObject titan)
    {
        titan.RemoveOwnership();
    }

}
