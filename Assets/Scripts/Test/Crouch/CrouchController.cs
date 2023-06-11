using UnityEngine;

public class CrouchController : MonoBehaviour
{
    public float crouchSpeed = 2f;
    public float normalSpeed = 5f;
    public float crouchColliderHeight = 0.5f;
    public float normalColliderHeight = 1f;

    private bool isCrouching = false;
    private CapsuleCollider capsuleCollider;
    private Vector3 originalCenter;
    private float originalHeight;

    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        originalCenter = capsuleCollider.center;
        originalHeight = capsuleCollider.height;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (!isCrouching)
                StartCrouch();
        }
        else
        {
            if (isCrouching)
                StopCrouch();
        }
    }

    private void StartCrouch()
    {
        isCrouching = true;
        capsuleCollider.height = crouchColliderHeight;
        capsuleCollider.center = new Vector3(originalCenter.x, originalCenter.y - (originalHeight - crouchColliderHeight) / 2f, originalCenter.z);
        transform.position = new Vector3(transform.position.x, transform.position.y - (originalHeight - crouchColliderHeight) / 2f, transform.position.z);
        // Réglez la vitesse du personnage pendant l'accroupissement si nécessaire
        // Par exemple, vous pouvez utiliser transform.Translate pour ajuster la position en fonction de la vitesse
        // transform.Translate(Vector3.forward * crouchSpeed * Time.deltaTime);
    }

    private void StopCrouch()
    {
        isCrouching = false;
        capsuleCollider.height = normalColliderHeight;
        capsuleCollider.center = originalCenter;
        transform.position = new Vector3(transform.position.x, transform.position.y + (originalHeight - normalColliderHeight) / 2f, transform.position.z);
        // Réglez la vitesse du personnage après s'être relevé si nécessaire
        // Par exemple, vous pouvez utiliser transform.Translate pour ajuster la position en fonction de la vitesse
        // transform.Translate(Vector3.forward * normalSpeed * Time.deltaTime);
    }
}