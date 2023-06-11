using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LedgeGrab : MonoBehaviour
{
    public CapsuleCollider ledgeCollider;
    public LayerMask ledgeLayer;
    public string ledgeTag = "Ledge";
    public float grabRange = 1.5f;
    public float climbSpeed = 2f;
    public float grabCooldown = 1f;

    public float m_speed = 5f;

    private bool isGrabbing;
    private float lastGrabTime;
    private Rigidbody rb;
    private Vector3 originalPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isGrabbing)
        {
            if (Input.GetMouseButtonUp(0))
            {
                // Lâcher le rebord
                isGrabbing = false;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                lastGrabTime = Time.time;
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Logic pour monter le personnage sur le rebord
                Vector3 m_input = new Vector3(Input.GetAxis("Horizontal"), 30, Input.GetAxis("Vertical"));
                rb.MovePosition(transform.position + m_input * Time.deltaTime * m_speed);
            }

            return;
        }

        if (Time.time - lastGrabTime < grabCooldown)
        {
            // Attente de la fin du cooldown avant de pouvoir s'accrocher à nouveau
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isGrabbing)
                return;
            // Utiliser Physics.OverlapSphere pour détecter les collisions avec les objets ayant le Layer "Ledge"
            //Collider[] colliders = Physics.OverlapSphere(transform.position, grabRange, ledgeLayer);
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ledge"))
        {
            if (Input.GetMouseButton(0) && Time.time - lastGrabTime >= grabCooldown)
            {
                isGrabbing = true;
                originalPosition = transform.position;
                rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ledge"))
        {
            isGrabbing = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            lastGrabTime = Time.time;
        }
    }
}