using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Animaciones : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;

    [Header("Opcional")]
    [Tooltip("Velocidad máxima usada para normalizar el movimiento")]
    [SerializeField] private float velocidadMax = 1f;

    private Vector3 movimientoLocal;
    
    private PlayerMovement playerMovement;
    

    void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        playerMovement=GetComponent<PlayerMovement>();
    }

    void Update()
    {
        ActualizarMovimiento();
        ActualizarSuelo();
    }

    // ---------- MÉTODOS PÚBLICOS ----------

    /// <summary>
    /// Dispara la animación de salto (llamado desde otro script)
    /// </summary>
    public void TriggerSalto()
    {
     //   Debug.Log("Saltot");
        animator.SetTrigger("Saltar");
    }

    // ---------- MÉTODOS PRIVADOS ----------

    void ActualizarMovimiento()
    {
        // Velocidad en espacio local
        Vector3 velocidad = characterController.velocity;
        movimientoLocal = transform.InverseTransformDirection(velocidad);

        float x = movimientoLocal.x;
        float y = movimientoLocal.z;
        float z = playerMovement.VerticalSpeed;

        // Normalización opcional
        if (velocidadMax > 0f)
        {
            x /= velocidadMax;
            y /= velocidadMax;
        }

        animator.SetFloat("X", x);
        animator.SetFloat("Y", y);
        animator.SetFloat("Z", z);
    }

    void ActualizarSuelo()
    {
        animator.SetBool("EnSuelo", characterController.isGrounded);
    }
}
