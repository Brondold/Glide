using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public MovementState state;

    public enum MovementState
    {
        Freeze,
        Unlimited
    }

    public bool freeze;
    public bool unlimited;

    public bool restricted;

    public CapsuleCollider crouch;
    public CapsuleCollider idle;

    [Header("Movement")]
    public float moveSpeed;
    public float sprintMultiplier;
    public float slideDistance;
    public float slideHeight;
    public float rotationSpeed;
    public float rotationDamping;

    public float groundDrag;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Stamina")]
    public float maxStamina;
    public float staminaDepletionRate;
    public float staminaRechargeRate;
    private float currentStamina;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode slideKey = KeyCode.LeftControl;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    [SerializeField] float fallTresholdVelocity = 5f;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Vector3 slideDirection;
    Vector3 slideStartPosition;
    bool isSliding;
    bool isCrouching;
    bool isWalking;
    bool isRunning;

    Rigidbody rb;

    [Header("HeadCheck")]
    public Transform headCheck; // Référence à l'objet vide placé au-dessus de la tête du personnage
    public float detectionRadius = 0.5f; // Rayon de détection
    public bool isObjectDetected = false;


    [Header("Animation")]
    public Animator animator;

    [Header("UI")]
    public TextMeshProUGUI speedText; // Référence au composant TextMeshProUGUI pour afficher la vitesse
    public TextMeshProUGUI staminaText; // Référence au composant TextMeshProUGUI pour afficher la stamina
    public GameObject mortText;

    [Header("Sound")]
    public AudioSource walkSound;
    public AudioSource runSound;
    public GameObject runSoundTest;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        bool previousGrounded = grounded;

        //Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        //Fall Damage & detection for death from Fall Damage
        if (!previousGrounded && grounded)
        {
            Debug.Log("Fall Damage" + (rb.velocity.y < -fallTresholdVelocity));

            if (rb.velocity.y < -fallTresholdVelocity)
            {
                float damage = Mathf.Abs(rb.velocity.y + fallTresholdVelocity);
                Debug.Log("Damage Dealt :" + damage);
                if (damage >= 4)
                {
                    mortText.SetActive(true);
                }
            }

        }

        //HeadCheckCollision with SphereRadius
        Collider[] colliders = Physics.OverlapSphere(headCheck.position, detectionRadius);

        isObjectDetected = false;

        foreach (Collider collider in colliders)
        {
            if (collider != null && collider.gameObject != gameObject && collider.gameObject.tag != "Player")
            {
                Debug.Log("Objet : " + collider.gameObject.name);
                isObjectDetected = true;
            }
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (grounded)
        {
            animator.SetBool("jump", false);
        }

        //Jump
        if (Input.GetKeyDown(jumpKey) || Input.GetButton("AXbox") && readyToJump && grounded && !isCrouching)
        {
            readyToJump = false;
            Jump();

            animator.SetBool("jump", true);

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Slide
        if (Input.GetKeyDown(slideKey) || Input.GetButtonDown("YXbox") && Input.GetKey(sprintKey) && grounded && currentStamina >= 20f && !isCrouching)
        {
            StartSlide();
            animator.SetBool("slide", true);
        }

        if (Input.GetKeyUp(slideKey) ||Input.GetButtonUp("YXbox") && !isObjectDetected)
        {
            EndSlide();
            animator.SetBool("slide", false);
        }
        if (Input.GetKeyUp(slideKey) || Input.GetButtonUp("YXbox") && isObjectDetected)
        {
            StartCrouch();
            animator.SetBool("slide", false);
            //animator.SetBool("crouchIdle", true);
        }

        //Crouch
        if (Input.GetKey(KeyCode.C) || Input.GetButton("XXbox") && grounded && !isSliding && !Input.GetKey(jumpKey) && !Input.GetKey(sprintKey))
        {
            StartCrouch();
            Debug.Log("Crouch");
            readyToJump = false;

            if ((targetVelocity.x != 0 || targetVelocity.z != 0) && isCrouching)
            {
                animator.SetBool("crouchIdle", false);
                animator.SetBool("crouchWalk", true);

                Debug.Log("CrouchWalking");
            }
            else
            {
                animator.SetBool("crouchIdle", true);
                animator.SetBool("crouchWalk", false);
            }


        }
        if (Input.GetKeyUp(KeyCode.C) || Input.GetButtonUp("XXbox") && grounded && !isSliding && !Input.GetKey(jumpKey) && !Input.GetKey(sprintKey) && !isObjectDetected)
        {
            EndCrouch();
            Debug.Log("StopCrouch");
            readyToJump = true;

            if (!isObjectDetected && !isCrouching)
            {
                animator.SetBool("crouchIdle", false);
                animator.SetBool("crouchWalk", false);
            }
            else
            {
                animator.SetBool("crouchIdle", false);
            }

        }

        if (!isObjectDetected && !isCrouching)
        {
            animator.SetBool("crouchIdle", false);
        }

        //Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //Detect if player is moving
        if ((targetVelocity.x != 0 || targetVelocity.z != 0) && !isCrouching && Input.GetKey(sprintKey) == false)
        {
            isWalking = true;
            animator.SetBool("walk", true);
            animator.SetBool("crouchWalk", false);
            animator.SetBool("run", false);

            runSoundTest.SetActive(false);

            Debug.Log("Walking");

            if(isWalking)
            {
                //runSound.Play();
            }
            if(!isWalking )
            {
                //runSound.Stop();
            }

        }
        else
        {
            isWalking = false;
            animator.SetBool("walk", false);

            walkSound.Play();
            //Debug.Log("NotWalking");
        }

        SpeedControl();
        Stamina();

        // Mettre à jour le texte de la vitesse dans l'UI
        speedText.text = rb.velocity.magnitude.ToString("F2");
        // Mettre à jour le texte de la stamina dans l'UI
        staminaText.text = currentStamina.ToString("F0");
    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        //Rotation Character
        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical).normalized * moveSpeed;

        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, .2f);
        }

        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        MovePlayer();

        if (Input.GetKey(KeyCode.C) == false || Input.GetButton("XXbox") == false && !isObjectDetected)
        {
            EndCrouch();
        }

        if (isSliding)
        {
            Slide();
        }
    }

    void MovePlayer()
    {
        if (restricted) return;

        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        // Calcul Direction Mouvement
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Calcul de la vitesse du personnage lors de la Course
        float targetSpeed = moveSpeed;

        if (Input.GetButton("YXbox") || Input.GetKey(sprintKey) && currentStamina != 0 && !isCrouching)
        {
            targetSpeed *= sprintMultiplier;
            currentStamina -= Time.deltaTime * staminaDepletionRate;
            Debug.Log("Sprint");

            if ((targetVelocity.x != 0 || targetVelocity.z != 0) && currentStamina > 0)
            {
                animator.SetBool("run", true);

                runSoundTest.SetActive(true);
                //walkSound.Play();               
            }
            else
            {
                Debug.Log("NoMoreStamina");
                animator.SetBool("walk", true);
                animator.SetBool("run", false);

                
            }
        }

        // Transition Vitesse Smooth
        float currentSpeed = Mathf.Lerp(rb.velocity.magnitude, targetSpeed, Time.deltaTime * 15f);

        // Verification Stamina
        if (currentStamina <= 0)
        {
            currentSpeed = moveSpeed;

            if (grounded)
            {
                rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
            }
            else if (!grounded)
            {
                rb.AddForce(moveDirection.normalized * currentSpeed * 10f * airMultiplier, ForceMode.Force);
            }
        }
        else
        {
            // Applique Force Mouvement
            if (grounded)
            {
                rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
            }
            else if (!grounded)
            {
                rb.AddForce(moveDirection.normalized * currentSpeed * 10f * airMultiplier, ForceMode.Force);
            }
        }

        //Rotation Character ?
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // Mettre à jour le texte de la vitesse dans l'UI
        speedText.text = targetSpeed.ToString("F2");
    }


    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Stamina()
    {
        if (Input.GetKey(sprintKey) || Input.GetButton("BXbox") && currentStamina > 0)
        {
            currentStamina -= Time.deltaTime * staminaDepletionRate;
        }
        else
        {
            currentStamina += Time.deltaTime * staminaRechargeRate;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void StartSlide()
    {
        if (isSliding) return;

        isSliding = true;
        slideStartPosition = transform.position;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        // Adjust the position to align with the ground
        //RaycastHit groundHit;

        idle.enabled = false;
        crouch.enabled = true;

        slideDirection = moveDirection.normalized;
    }

    private void Slide()
    {
        float distance = Vector3.Distance(slideStartPosition, transform.position);

        if (distance >= slideDistance)
        {
            EndSlide();
        }
        else
        {
            Vector3 slideMovement = slideDirection * moveSpeed * Time.deltaTime;
            RaycastHit groundHit;

            // Cast a ray from the current position to the desired slide position
            if (Physics.Raycast(transform.position, slideMovement, out groundHit, slideMovement.magnitude, whatIsGround))
            {
                // Adjust the slide movement to stop at the point of impact
                slideMovement = groundHit.point - transform.position;
            }

            // Move the character using the adjusted slide movement
            rb.MovePosition(rb.position + slideMovement);
        }
    }

    private void EndSlide()
    {
        if (!isSliding) return;

        isSliding = false;
        rb.useGravity = true;
        currentStamina -= 20f;

        // Effectuer un raycast vers le bas pour détecter le sol
        RaycastHit hit;

        if (isObjectDetected)
        {
            StartCrouch();
        }
        else
        {
            idle.enabled = true;
            crouch.enabled = false;
        }
    }

    private void StartCrouch()
    {

        if (isCrouching) return;

        isCrouching = true;

        //rb.velocity = Vector3.zero;       

        idle.enabled = false;
        crouch.enabled = true;

    }

    private void EndCrouch()
    {

        if (!isCrouching) return;

        isCrouching = false;

        idle.enabled = true;
        crouch.enabled = false;

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(headCheck.position, detectionRadius);
    }

    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.Freeze;
            rb.velocity = Vector3.zero;
        }
        else if (unlimited)
        {
            state = MovementState.Unlimited;
            moveSpeed = 999f;
            return;
        }
    }
}