using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

public class EnterVanguardTitan : NetworkBehaviour
{
    public Animator titanAnimator;
    public cameraShake shaker;
 
    CharacterController controller;
    BoxCollider rangeCheck;
 
    public GameObject embarkRifle;
    public GameObject Rifle;
 
    public GameObject titanCamera;
    public GameObject embarkTitanCamera;
    public Transform groundCheck;
 
    public LayerMask groundMask;
 
    Vector3 Yvelocity;
 
    bool isFalling;
    bool isGrounded;
    public bool inRangeForEmbark;
 
    public float fallingSpeed;

    public Transform embarkPos;
    public Transform embarkLookTarget;

    public bool inTitan;

    TitanHealth healthScript;
    public bool dead;
    bool alreadyLanded;
    bool alreadyLanded2;

    public GameObject particles;
    public GameObject fall;
    public GameObject land;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        rangeCheck = GetComponent<BoxCollider>();
        healthScript = GetComponent<TitanHealth>();
 
        Rifle.SetActive(false);
    }
 
    public void StartFall()
    {
        isFalling = true;
        titanAnimator.SetTrigger("StartFall");
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, 50f, groundMask);
        if (isGrounded && !alreadyLanded)
        {
            Effects(fall, land, particles);
            alreadyLanded = true;
        }

        if (Physics.Raycast(groundCheck.position, Vector3.down, 1f, groundMask) && !alreadyLanded2)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                ShakeCamera(player);
            }
            alreadyLanded2 = true;
        }
    }

    [ObserversRpc]
    void Effects(GameObject fall, GameObject land, GameObject particles)
    {
        fall.GetComponent<AudioSource>().Stop();
        land.GetComponent<AudioSource>().Play();
        particles.GetComponent<ParticleSystem>().Stop();
    }

    [ObserversRpc]
    void ShakeCamera(GameObject player)
    {
        if (player.GetComponent<cameraShake>() != null)
            StartCoroutine(player.GetComponent<cameraShake>().Shake(0.15f, 1f));
    }
 
    public IEnumerator Embark()
    {
        isFalling = false;
        rangeCheck.enabled = false;
        titanAnimator.SetTrigger("Embark");
 
        yield return new WaitForSeconds(1.5f);
 
        embarkRifle.SetActive(false);
        Rifle.SetActive(true);
 
        yield return new WaitForSeconds(2.4f);

        inTitan = true;
    }

    public IEnumerator EmbarkCamera()
    {
        embarkTitanCamera.SetActive(true);

        yield return new WaitForSeconds(3.9f);

        embarkTitanCamera.SetActive(false);
        titanCamera.GetComponent<Camera>().enabled = true;
        titanCamera.GetComponent<AudioListener>().enabled = true;
    }

    void OnTriggerEnter()
    {
        inRangeForEmbark = true;
    }
 
    void OnTriggerExit()
    {
        inRangeForEmbark = false;
    }
 
    public void ExitTitan()
    {
        //Player_ShowRPC();
        //playerCamera.SetActive(true);
        titanCamera.GetComponent<Camera>().enabled = false;
        titanCamera.GetComponent<AudioListener>().enabled = false;
        inTitan = false;
		rangeCheck.enabled = true;
        isFalling = false;
        //player.transform.parent = null;

    }

    public void Eject()
    {
        titanCamera.SetActive(false);
        inTitan = false;
        rangeCheck.enabled = true;
        isFalling = false;
        healthScript.Die();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFalling)
        {
            CheckGround();
            Fall();
        }
        if (healthScript.dead)
        {
            dead = true;
        }
    }
 
    void Fall()
    {
        Yvelocity.y += fallingSpeed * Time.deltaTime;
        controller.Move( Yvelocity * Time.deltaTime );
    }

}
