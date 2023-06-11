using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallDetection : MonoBehaviour
{
    public float minHeightToDie = 10f; // Hauteur minimale pour déclencher la mort
    public LayerMask groundLayer; // Couche du sol
    public Transform groundCheck; // Point de vérification pour détecter le sol (par exemple, un objet vide situé près des pieds du personnage)

    private bool isFalling = false;
    private float fallStartHeight = 0f;
    private bool isDead = false;

    private void Update()
    {
        // Vérifier si l'objet est en contact avec le sol
        bool isTouchingGround = Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer);

        if (isTouchingGround && isFalling && !isDead)
        {
            // Calculer la hauteur de la chute
            float fallHeight = fallStartHeight - hit.point.y;

            // Vérifier si la hauteur de la chute est supérieure à la hauteur minimale pour mourir
            if (fallHeight > minHeightToDie)
            {
                Die();
            }
        }

        // Si l'objet est en l'air et n'est pas en train de tomber, enregistrer la hauteur de départ de la chute
        if (!isTouchingGround && !isFalling)
        {
            isFalling = true;
            fallStartHeight = transform.position.y;
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Vous êtes mort !");
        // Ici, vous pouvez ajouter du code pour afficher un message à l'écran ou effectuer d'autres actions lorsque le personnage meurt
    }
}