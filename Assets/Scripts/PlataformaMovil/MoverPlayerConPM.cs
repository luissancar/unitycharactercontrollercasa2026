using UnityEngine;

public class MoverPlayerConPM : MonoBehaviour
{
    [Tooltip("Tag que debe tener el jugador (en el GameObject RAÍZ del CharacterController)")]
    [SerializeField] private string playerTag = "Player";

    private Transform playerTransform;
    private CharacterController playerController;

    private Vector3 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 delta = transform.position - lastPosition;

        if (playerTransform != null)
        {
            if (playerController != null)
            {
                // Separar en horizontal y vertical
                Vector3 horizontalDelta = new Vector3(delta.x, 0f, delta.z);

                float upDeltaY = Mathf.Max(delta.y, 0f);
                Vector3 verticalDelta = new Vector3(0f, upDeltaY, 0f);

                // IMPORTANTE: para CharacterController se usa Move
                playerController.Move(horizontalDelta + verticalDelta);
            }
            else
            {
                // Fallback si fuera otro tipo de objeto
                playerTransform.position += new Vector3(delta.x, Mathf.Max(delta.y, 0f), delta.z);
            }
        }

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Miramos el root, por si el collider está en un hijo
        Transform root = other.transform.root;

        Debug.Log($"Trigger ENTER con: {other.name} (tag: {other.tag}) | ROOT: {root.name} (tag: {root.tag})");

        if (!root.CompareTag(playerTag))
            return;

        playerTransform = root;
        playerController = root.GetComponent<CharacterController>();

        if (playerController == null)
        {
            Debug.LogWarning("El objeto con tag Player no tiene CharacterController en el root.");
        }
        else
        {
            Debug.Log("Player asignado a plataforma móvil.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Transform root = other.transform.root;

        if (root == playerTransform)
        {
            Debug.Log("Player salió de la plataforma.");
            playerTransform = null;
            playerController = null;
        }
    }
}
