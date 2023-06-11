using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerScript : MonoBehaviour
{
    public float speed = 5f;              // Vitesse de déplacement du personnage
    public float rotationSpeed = 10f;     // Vitesse de rotation du personnage

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Déplacement du personnage en fonction des touches de déplacement (WASD, flèches)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        moveDirection = new Vector3(horizontalInput, 0f, verticalInput);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;

        // Application de la gravité au personnage
        moveDirection.y -= 9.8f * Time.deltaTime;

        // Déplacement du personnage
        controller.Move(moveDirection * Time.deltaTime);
    }
}
