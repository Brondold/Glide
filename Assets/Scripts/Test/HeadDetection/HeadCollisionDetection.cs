using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCollisionDetection : MonoBehaviour
{
    public Transform headCheck; // Référence à l'objet vide placé au-dessus de la tête du personnage
    public float detectionRadius = 0.5f; // Rayon de détection

    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(headCheck.position, detectionRadius);

        bool isObjectDetected = false;

        foreach (Collider collider in colliders)
        {
            if (collider != null && collider.gameObject != gameObject)
            {
                Debug.Log("Objet : " + collider.gameObject.name);
                isObjectDetected = true;
            }
        }

        if (!isObjectDetected)
        {
            //Debug.Log("Pas d'objet");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(headCheck.position, detectionRadius);
    }
}
