using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

public class PilotCamera : NetworkBehaviour
{
    public float minX = -90f;
    public float maxX = 90f;

    public float sensitivity;
    public Camera cam;

    Vector2 look;
    float rotY = 0f;
    float rotX = 0f;

    RangerMovement move;

    public float sprintBobSpeed;
    public float runBobSpeed;
    public float sprintBobAmount;
    public float runBobAmount;
    float defaultY;
    private float timer;

    public LayerMask wallHack;
    bool isHacking;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        move = GetComponentInParent<RangerMovement>();

        defaultY = cam.transform.localPosition.y;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            cam.enabled = false;
            if (cam.gameObject.TryGetComponent(out AudioListener audioListener))
            {
                audioListener.enabled = false;
            }
        }
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>()*sensitivity;
    }

    void Update()
    {
        if (!base.IsOwner)
            return;

        if (this.transform == null)
            return;

        if (move.canMove == false)
            return;

        rotY += look.x;
        rotX += look.y;

        rotX = Mathf.Clamp(rotX, minX, maxX);

        transform.localEulerAngles = new Vector3(0, rotY, 0);
        cam.transform.localEulerAngles = new Vector3(-rotX, 0, move.tilt);

        HandleHeadBob();
        WallHack();
        MoveObject();
    }

    void MoveObject()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            var screen = GameObject.FindGameObjectWithTag("screen");
            screen.GetComponent<MoveObject>().Move();
        }
    }

    void WallHack()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isHacking)
            {
                isHacking = true;
                cam.cullingMask |= (1 << 14);
            }
            else
            {
                isHacking = false;
                var mask = cam.cullingMask;
                mask = cam.cullingMask & ~(1 << 14);
                cam.cullingMask = mask;
            }
        }


    }

    void HandleHeadBob()
    {
        if (move.isMoving && move.isGrounded && !move.isSliding)
        {
            timer += Time.deltaTime * (move.isSprinting ? sprintBobSpeed : runBobSpeed);
            cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, defaultY + Mathf.Sin(timer) * (move.isSprinting ? sprintBobAmount : runBobAmount), cam.transform.localPosition.z);
        }
    }

}
