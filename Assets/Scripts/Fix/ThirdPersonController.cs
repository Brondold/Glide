using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public CharacterController controller;


    [Header("Movement Parameters")]
    public float speed;
    [SerializeField] public float jumpPower;
    public float sprintSpeed;

    [Header("Rotation Parameters")]
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    [Header("Gravity Parameters")]
    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float velocity;

    [Header("Animation")]
    public Animator animator;

    void Update()
    {

        // Input Movement
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Detection if player is Moving
        if (direction.magnitude >= 0.1f)
        {
            // Rotation Player
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Detection if player is Sprinting
            if (Input.GetKey(KeyCode.LeftShift))
            {
                controller.Move(direction * sprintSpeed * Time.deltaTime);

                animator.SetBool("isSprinting", true);
                animator.SetBool("isWalking", false);
            }
            else
            {
                controller.Move(direction * speed * Time.deltaTime);

                animator.SetBool("isSprinting", false);
                animator.SetBool("isWalking", true);
            }           

        }
        if(direction.magnitude < 0.1f)
        {
            animator.SetBool("isWalking", false);
        }

        // Apply gravity
        if (!controller.isGrounded)
        {
            velocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        else
        {
            velocity = 0f;
        }

        // Input Movement 
        if (Input.GetKey(KeyCode.Space))
        {
            if(IsGrounded())
            {
                Jump();
                animator.SetBool("isJumping", true);
            }
            else
            {
                animator.SetBool("isJumping", false);
            }
        }

        controller.Move(new Vector3(0f, velocity, 0f) * Time.deltaTime);

    }

    public void Jump()
    {
        velocity = jumpPower;

    }

    private bool IsGrounded() => controller.isGrounded;
}
