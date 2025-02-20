using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterScript : MonoBehaviour
{
    private Animator animator;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private CharacterController characterController;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 4.0f;
    private float jumpHeight = 1.5f;
    private float gravityValue = -9.81f;
    private MoveStates prevMoveState = MoveStates.Idle;

    void Start()
    {
        animator = GetComponent<Animator>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        MoveStates moveState = (MoveStates) animator.GetInteger("MoveState");
        
        groundedPlayer = characterController.isGrounded;
        if(groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        float sprintValue = sprintAction.ReadValue<float>();

        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0.0f;
        if(cameraForward != Vector3.zero)
        {
            cameraForward.Normalize();
        }
        Vector3 moveStep = playerSpeed * Time.deltaTime * (1.0f + sprintValue) * (
            moveValue.x * Camera.main.transform.right +
            moveValue.y * cameraForward
        );

        if( moveState != MoveStates.JumpStart && 
            moveState != MoveStates.Jumping &&
            moveState != MoveStates.JumpFinish)
        {
            if(moveStep.magnitude > 0)
            {
                this.transform.forward = cameraForward;
                moveState = Mathf.Abs(moveValue.x) > Mathf.Abs(moveValue.y)
                    ? (sprintValue > 0 ? MoveStates.SideRun : MoveStates.SideWalk)
                    : (sprintValue > 0 ? MoveStates.Run : MoveStates.Walk);
            }
            else
            {
                moveState = MoveStates.Idle;
            }
        }
        characterController.Move(moveStep);
        
        //Makes the player jump
        if(jumpAction.ReadValue<float>() > 0 && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);

        if (moveState != prevMoveState)
        {
            animator.SetInteger("MoveState", (int)moveState);
            prevMoveState = moveState;
        }
    }

    private void OnJumpStartAnimationEnds()
    {
        animator.SetInteger("MoveState", (int)MoveStates.Jumping);
    }

    private void OnJumpFinishAnimationEnds()
    {
        animator.SetInteger("MoveState", (int)MoveStates.Idle);
    }
}

enum MoveStates
{
    Idle       = 1,
    Walk       = 2,
    SideWalk   = 3,
    Run        = 4,
    SideRun    = 5,
    JumpStart  = 6, 
    Jumping    = 7,
    JumpFinish = 8,
}
