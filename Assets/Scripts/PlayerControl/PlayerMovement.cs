using UnityEngine; // Importa el namespace principal de Unity (MonoBehaviour, Vector3, etc.).
using UnityEngine.InputSystem; // Importa el namespace del nuevo Input System (InputValue, PlayerInput, etc.).

[RequireComponent(typeof(CharacterController))] // Obliga a que el GameObject tenga un CharacterController.
public class PlayerMovement : MonoBehaviour // Declara la clase PlayerMovement, que hereda de MonoBehaviour.
{
    [Header("Movimiento")] // Agrupa las siguientes variables bajo el encabezado "Movimiento" en el inspector.
    public float moveSpeed = 5f; // Velocidad de movimiento horizontal del jugador.

    [Header("Salto / Gravedad")] // Agrupa las siguientes variables bajo "Salto / Gravedad".
    public float jumpHeight = 2f; // Altura deseada del salto (en unidades del mundo).

    public float gravity = -9.81f; // Valor de la gravedad, negativo para empujar hacia abajo.

    private CharacterController characterController; // Referencia privada al CharacterController del jugador.

    private Vector2 moveInput; // Almacena el input de movimiento (X = izquierda/derecha, Y = adelante/atrás).
    private float verticalVelocity; // Velocidad vertical actual (para salto y caída).
    private bool jumpRequested = false; // Indica si el jugador ha pedido un salto (pulsando el botón de salto).


    [SerializeField] private AudioSource audioSourceSalto;
    [SerializeField] private AudioSource audioSourcePasos;
    [SerializeField] private int minSpeed = 1; // velocidad mínima sonido pasos

    
    
    private Animaciones animacion;
    
    
    
    
    /// vertical velocity
    public float VerticalSpeed => verticalVelocity;

    private void Awake() // Awake se ejecuta cuando el objeto se inicializa, antes de Start.
    {
        characterController =
            GetComponent<CharacterController>(); // Obtiene el CharacterController adjunto al mismo GameObject.
        animacion = GameObject.FindObjectOfType<Animaciones>();
    }

    // INPUT SYSTEM (Send Messages) -> acción "Move"
    private void OnMove(InputValue value) // Método llamado automáticamente por PlayerInput cuando se activa la acción "Move".
    {
        moveInput = value.Get<Vector2>(); // Convierte el valor de entrada en un Vector2 y lo guarda en moveInput.
    }

    // INPUT SYSTEM (Send Messages) -> acción "Jump"
    private void OnJump(InputValue value) // Método llamado automáticamente por PlayerInput cuando se activa la acción "Jump".
    {
        if (value.isPressed) // Comprueba si el botón de salto está presionado.
            jumpRequested = true; // Marca que se ha solicitado un salto; se usará en el siguiente Update.
    }

    private void Update() // Update se ejecuta una vez por frame.
    {
        if (characterController == null) // Si por alguna razón no hay CharacterController...
            return; // ...no hacemos nada para evitar errores.

        HandleMovement(); // Llama al método que gestiona todo el movimiento del jugador.
        SonidoPasos();
        
        
    }

    
    
    private void HandleMovement() // Método que implementa la lógica de movimiento, salto y gravedad.
    {
        bool isGrounded = characterController.isGrounded; // Comprueba si el CharacterController está tocando el suelo.

        // Reset vertical al tocar suelo
        if (isGrounded && verticalVelocity < 0f) // Si estamos en el suelo y la velocidad vertical es hacia abajo...
            verticalVelocity = -2f; // ...ponemos un pequeño valor negativo para mantenerlo pegado al suelo.

        // Movimiento local XZ
        Vector3
            localMove = new Vector3(moveInput.x, 0f,
                moveInput.y); // Crea un vector de movimiento en el espacio local (X y Z, sin Y).

        // Convertir de local a mundo según la orientación del player (yaw la controla PlayerLook)
        Vector3 worldMove = transform
                .TransformDirection(
                    localMove); // Convierte el vector local a espacio global usando la rotación del jugador.

        // Normalizar para que la diagonal no sea más rápida
        if (worldMove.sqrMagnitude > 1f) // Si la magnitud al cuadrado es mayor que 1 (movimiento diagonal fuerte)...
            worldMove.Normalize(); // ...normaliza el vector para que la velocidad en diagonal sea uniforme.

        Vector3
            horizontalVelocity =
                worldMove * moveSpeed; // Multiplica la dirección por la velocidad para obtener la velocidad horizontal.

        // Salto
        if (isGrounded && jumpRequested) // Si está en el suelo y se ha pedido un salto...
        {
            if (audioSourceSalto != null)
                    audioSourceSalto.Play();
            
            /// animacion
            ///
           
            animacion.TriggerSalto();
            
            
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            // Calcula la velocidad vertical inicial necesaria para alcanzar la altura de salto deseada usando la fórmula de física.
            jumpRequested = false; // Resetea la petición de salto para no saltar varias veces con una sola pulsación.
        }

        // Gravedad
        verticalVelocity += gravity * Time.deltaTime; // Aplica la gravedad acumulándola en la velocidad vertical cada frame.

        Vector3 velocity = horizontalVelocity; // Comienza con la parte horizontal de la velocidad.
        velocity.y = verticalVelocity; // Añade la componente vertical (salto/caída).

        characterController.Move(velocity * Time.deltaTime);
        // Mueve el CharacterController según la velocidad total (horizontal + vertical) multiplicada por deltaTime.
    }

    private void SonidoPasos()
    {
        if (audioSourcePasos == null) return;

        // Velocidad horizontal (ignora saltos/caídas)
        Vector3 v = characterController.velocity;
        v.y = 0f;
    //    Debug.Log(v.magnitude);

        bool andando = characterController.isGrounded && v.magnitude > minSpeed;

        if (andando)
        {
            if (!audioSourcePasos.isPlaying)
                audioSourcePasos.Play();
        }
        else
        {
            if (audioSourcePasos.isPlaying)
                audioSourcePasos.Stop();
        }
    }
}