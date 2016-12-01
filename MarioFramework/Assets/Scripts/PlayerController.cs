using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private enum CharacterStates
    {
        Idle,
        Walk,
        Jog,
        Run,
        Jump,
        Noof
    }

    public float AirSpeed = 1.0f;
    public float DirectionSmoothSpeed = 10.0f;
    public float Acceleration = 10.0f;
    public float Decceleration = 10.0f;
    public float Gravity = 20.0f;
    public float JumpHeight = 20.0f;

    public float SpeedIdleMax = 0.2f;
    public float SpeedWalkMax	= 3.0f;
    public float SpeedJogMax = 5.0f;
    public float SpeedRunMax = 8.0f;

    public AnimationClip[] AnimClips = new AnimationClip[(int)CharacterStates.Noof];

    private CharacterController characterController = null;
    private Animation anim = null;
    private CollisionFlags collisionFlags;
    private CharacterStates characterState = CharacterStates.Idle;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 inAirVelocity = Vector3.zero;

   private ParticleSystem dustParticles = null;

    private float moveSpeed = 0.0f;
    private float verticalSpeed = 0.0f;
    private bool isJumping = false;


    // Use this for initialization
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animation>();
        dustParticles = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        // forward vector relative to the camera along the x-z plane
        Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        Vector3 targetDirection = horizontal * right + vertical * forward;

        if (IsGrounded())
        {
            if (targetDirection != Vector3.zero)                                        // store currentSpeed and direction separately
            {
                moveDirection = Vector3.Lerp(moveDirection, targetDirection, DirectionSmoothSpeed * Time.deltaTime);
                moveDirection = moveDirection.normalized;
            }

            float targetSpeed = targetDirection.magnitude;
            targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f) * SpeedRunMax;

            if(targetSpeed > SpeedIdleMax)
                moveSpeed += (Acceleration * Time.deltaTime);
            else
                moveSpeed -= (Decceleration * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && !isJumping)
            {
                isJumping = true;
                inAirVelocity.y = JumpHeight;
            }
            else
            {
                inAirVelocity = new Vector3(0, -0.1f, 0);
                isJumping = false;
            }
            verticalSpeed = 0.0f;
        }
        else
        {
            inAirVelocity += targetDirection.normalized * Time.deltaTime * AirSpeed;
            verticalSpeed -= Gravity * Time.deltaTime;
        }
        moveSpeed = Mathf.Clamp(moveSpeed, 0.0f, SpeedRunMax);

        //if (moveSpeed < SpeedIdleMax)
        //    moveSpeed = 0.0f;
        Vector3 movement = moveDirection * moveSpeed + new Vector3(0, verticalSpeed, 0) + inAirVelocity;



        // Set character final rotation
        if(moveDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(moveDirection);

        // Move the character
        collisionFlags = characterController.Move(movement);

        // Update the character state
        UpdateStates();

        // Set the current animation clip to play
        anim.CrossFade(AnimClips[(int)characterState].name);
    }
    private bool IsGrounded()
    {                                               
        return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
    }

    private void UpdateStates()
    {
        if (moveSpeed <= SpeedIdleMax)
        {
            characterState = CharacterStates.Idle;
        }
        else if (moveSpeed < SpeedWalkMax)
        {
            characterState = CharacterStates.Walk;
        }
        else if (moveSpeed < SpeedJogMax)
        {
            characterState = CharacterStates.Jog;
        }
        else if (moveSpeed <= SpeedRunMax)
        {
            characterState = CharacterStates.Run;
        }

        if(isJumping)
        {
            characterState = CharacterStates.Jump;
        }
    }
}
