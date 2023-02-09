using FishNet.Object.Prediction;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MoveData
{
    public bool Jump;
    public float Horizontal;
    public float Forward;
}


public struct ReconcileData
{
    public Vector3 Position;
    public float VerticalVelocity;
}

public class ServerMovementTest : NetworkBehaviour
{
    /// <summary>
    /// Audio to play when jumping.
    /// </summary>
    /// <summary>
    /// How fast to move.
    /// </summary>
    [SerializeField]
    private float _moveSpeed = 5f;

    /// <summary>
    /// CharacterController on the object.
    /// </summary>
    private CharacterController _characterController;
    /// <summary>
    /// True if a jump was queued on client-side.
    /// </summary>
    private bool _jumpQueued;
    /// <summary>
    /// Velocity of the character, synchronized.
    /// </summary>
    private float _verticalVelocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (base.IsServer || base.IsClient)
            base.TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (base.TimeManager != null)
            base.TimeManager.OnTick -= TimeManager_OnTick;
    }

    private void Update()
    {
        if (!base.IsOwner)
            return;
        //Check if the owner intends to jump.
        _jumpQueued |= Input.GetKeyDown(KeyCode.Space);
    }

    /// <summary>
    /// Called every time the TimeManager ticks.
    /// This will occur at your TickDelta, generated from the configured TickRate.
    /// </summary>
    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconcile(default, false);
            BuildActions(out MoveData md);
            Move(md, false);
        }
        if (base.IsServer)
        {
            Move(default, true);
            ReconcileData rd = new ReconcileData()
            {
                Position = transform.position,
                VerticalVelocity = _verticalVelocity
            };
            Reconcile(rd, true);
        }
    }

    /// <summary>
    /// Build MoveData that both the client and server will use in Replicate.
    /// </summary>
    /// <param name="moveData"></param>
    private void BuildActions(out MoveData moveData)
    {
        moveData = default;
        moveData.Jump = _jumpQueued;
        moveData.Horizontal = Input.GetAxisRaw("Horizontal");
        moveData.Forward = Input.GetAxisRaw("Vertical");

        //Unset queued values.
        _jumpQueued = false;
    }

    /// <summary>
    /// Runs MoveData on the client and server.
    /// </summary>
    /// <param name="asServer">True if the method is running on the server side. False if on the client side.</param>
    /// <param name="replaying">True if logic is being replayed from cached inputs. This only executes as true on the client.</param>
    [Replicate]
    private void Move(MoveData moveData, bool asServer, bool replaying = false)
    {
        float delta = (float)base.TimeManager.TickDelta;
        Vector3 movement = new Vector3(moveData.Horizontal, 0f, moveData.Forward).normalized;
        //Add moveSpeed onto movement.
        movement *= _moveSpeed;

        //If jumping move the character up one unit.
        if (moveData.Jump && _characterController.isGrounded)
        {
            //7f is our jump velocity.
            _verticalVelocity = 7f;
        }

        //Subtract gravity from the vertical velocity.
        _verticalVelocity += (Physics.gravity.y * delta);
        //Perhaps prevent the value from getting too low.
        _verticalVelocity = Mathf.Max(-20f, _verticalVelocity);

        //Add vertical velocity to the movement after movement is normalized.
        //You don't want to normalize the vertical velocity.
        movement += new Vector3(0f, _verticalVelocity, 0f);

        //Move your character!
        _characterController.Move(movement * delta);
    }

    /// <summary>
    /// Resets the client to ReconcileData.
    /// </summary>
    [Reconcile]
    private void Reconcile(ReconcileData recData, bool asServer)
    {
        /* Reset the client to the received position. It's okay to do this
         * even if there is no de-synchronization. */
        transform.position = recData.Position;
        _verticalVelocity = recData.VerticalVelocity;
    }
}