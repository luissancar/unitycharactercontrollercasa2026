using UnityEngine;                     // Importa el namespace principal de Unity (MonoBehaviour, Vector3, etc.).
using UnityEngine.InputSystem;         // Importa el namespace del nuevo Input System (InputValue, PlayerInput, etc.).

[RequireComponent(typeof(CharacterController))] // Obliga a que el GameObject tenga un CharacterController (si no, Unity lo añade).
public class PlayerCon : MonoBehaviour          // Define la clase PlayerCon, que hereda de MonoBehaviour.
{
    [Header("Referencias")]                     // Agrupa estos campos bajo el encabezado "Referencias" en el inspector.
    public CharacterController characterController; // Referencia pública al CharacterController (puede asignarse a mano).
    public Transform cameraTransform;           // Referencia a la Transform de la cámara (normalmente hija del player).

    [Header("Movimiento")]                      // Encabezado "Movimiento" en el inspector.
    public float moveSpeed = 5f;                // Velocidad de movimiento horizontal del jugador.

    [Header("Mirar (ratón)")]                   // Encabezado "Mirar (ratón)" en el inspector.
    public float mouseSensitivity = 120f;       // Sensibilidad del ratón (grados por segundo).
    public float minPitch = -40f;               // Límite mínimo de inclinación vertical de la cámara (mirar hacia abajo).
    public float maxPitch = 70f;                // Límite máximo de inclinación vertical de la cámara (mirar hacia arriba).

    [Header("Salto / Gravedad")]                // Encabezado "Salto / Gravedad" en el inspector.
    public float jumpHeight = 2f;               // Altura que queremos que alcance el salto.
    public float gravity = -9.81f;              // Valor de la gravedad (negativo para empujar hacia abajo).

    private Vector2 moveInput;                  // Almacena el input de movimiento (X = izquierda/derecha, Y = adelante/atrás).
    private Vector2 lookInput;                  // Almacena el input de mirar (ratón o stick derecho).

    private float verticalVelocity;             // Velocidad vertical actual (salto/caída).
    private float cameraPitch = 0f;             // Ángulo actual de pitch de la cámara (mirar arriba/abajo).

    private bool jumpRequested = false;         // Indica si se ha pedido un salto (cuando el jugador pulsa el botón de salto).

    private void Awake()                        // Awake se llama al instanciar el objeto, antes de Start.
    {
        if (characterController == null)        // Si no se ha asignado un CharacterController desde el inspector...
            characterController = GetComponent<CharacterController>(); // ...busca el componente en el mismo GameObject.

        if (cameraTransform == null && Camera.main != null) // Si no se ha asignado cámara, pero hay una cámara principal...
            cameraTransform = Camera.main.transform;        // ...usa la cámara principal como cámara del jugador.
    }

    private void Start()                        // Start se llama antes del primer frame de Update.
    {
        // Deja solo el yaw inicial que tenga el player en la escena,
        // y anula cualquier inclinación en X y Z para que no mire al suelo.
        float yaw = transform.eulerAngles.y;    // Guarda la rotación Y actual del player (dirección en la que está mirando).
        transform.rotation = Quaternion.Euler(0f, yaw, 0f); // Fija la rotación del player a (X=0, Y=yaw, Z=0).

        // La cámara empieza mirando recto (sin inclinación hacia arriba o abajo).
        cameraPitch = 0f;                       // Resetea el pitch interno a 0 (mirar recto).
        lookInput = Vector2.zero;              // Resetea el input de mirar para evitar rotaciones iniciales raras.

        if (cameraTransform != null)           // Si tenemos una referencia válida a la cámara...
        {
            // Rotación local de la cámara recta, sin inclinaciones raras.
            cameraTransform.localRotation = Quaternion.Euler(0f, 0f, 0f); // La cámara empieza con rotación local (0,0,0).
        }
    }

    // --- INPUT SYSTEM callbacks (Send Messages) ---
    // Estos métodos se llaman automáticamente si PlayerInput está en modo "Send Messages" y
    // las acciones del mapa de input se llaman igual (Move, Look, Jump).

    private void OnMove(InputValue value)       // Método llamado cuando se recibe input de la acción "Move".
    {
        moveInput = value.Get<Vector2>();       // Convierte el InputValue en un Vector2 y lo guarda como moveInput.
    }

    private void OnLook(InputValue value)       // Método llamado cuando se recibe input de la acción "Look".
    {
        lookInput = value.Get<Vector2>();       // Convierte el InputValue en un Vector2 y lo guarda como lookInput.
    }

    private void OnJump(InputValue value)       // Método llamado cuando se recibe input de la acción "Jump".
    {
        if (value.isPressed)                    // Si el botón de salto está presionado en este frame...
            jumpRequested = true;               // ...marcamos que se ha solicitado un salto (se usará en HandleMovement).
    }

    private void Update()                       // Update se llama una vez por frame.
    {
        if (characterController == null || cameraTransform == null) // Si falta alguna referencia crítica...
            return;                             // ...no hacemos nada para evitar errores.

        HandleLook();                           // Gestiona la rotación del jugador y la cámara (mirar con ratón).
        HandleMovement();                       // Gestiona movimiento, salto y gravedad del jugador.
    }

    // Rotación tipo shooter: ratón gira al player (yaw) + inclina cámara (pitch).
    private void HandleLook()                   // Método que procesa el input de mirar.
    {
        // Input del ratón / stick: se escala por sensibilidad y deltaTime.
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime; // Rotación horizontal por frame.
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime; // Rotación vertical por frame.

        // Girar el cuerpo del player en Y (yaw).
        transform.Rotate(0f, mouseX, 0f);       // Rota el transform del player alrededor del eje Y (izquierda/derecha).

        // Acumular pitch (mirar arriba/abajo), invirtiendo mouseY para la sensación típica de FPS.
        cameraPitch -= mouseY;                  // Resta mouseY para que mover el ratón hacia arriba mire hacia arriba.
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch); // Limita el pitch entre minPitch y maxPitch.

        // Aplica la rotación local a la cámara solo en X (pitch).
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        // Y=0 porque la rotación horizontal ya la lleva el player, Z=0 para evitar inclinaciones laterales.
    }

    private void HandleMovement()               // Método que procesa movimiento, salto y gravedad.
    {
        bool isGrounded = characterController.isGrounded; // Comprueba si el CharacterController está tocando el suelo.

        // Reset vertical al tocar suelo.
        if (isGrounded && verticalVelocity < 0f) // Si está en el suelo y la velocidad vertical es hacia abajo...
            verticalVelocity = -2f;              // ...fija un pequeño valor negativo para pegar al player al suelo.

        // --- MOVIMIENTO HORIZONTAL LOCAL (tipo shooter 3ª persona) ---
        Vector3 localMove = new Vector3(moveInput.x, 0f, moveInput.y); // Crea vector de movimiento local (X,Z) sin componente Y.

        // Convertir de espacio local a mundo según la orientación del player.
        Vector3 worldMove = transform.TransformDirection(localMove); // Pasa el movimiento de coordenadas locales a mundiales.

        // Normalizar para que la diagonal no sea más rápida.
        if (worldMove.sqrMagnitude > 1f)        // Si la magnitud al cuadrado es mayor que 1 (diagonal con valor >1)...
            worldMove.Normalize();              // ...normaliza para tener velocidad homogénea en todas direcciones.

        // Aplicar velocidad horizontal.
        Vector3 horizontalVelocity = worldMove * moveSpeed; // Multiplica la dirección por la velocidad de movimiento.

        // --- SALTO ---
        if (isGrounded && jumpRequested)        // Si estamos en el suelo y se ha pedido un salto...
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            // Calcula la velocidad vertical inicial necesaria para alcanzar la altura de salto deseada.
            jumpRequested = false;              // Consume la petición de salto para no saltar múltiples veces.
        }

        // --- GRAVEDAD ---
        verticalVelocity += gravity * Time.deltaTime; // Aplica la gravedad a la velocidad vertical cada frame.

        Vector3 velocity = horizontalVelocity;  // Empieza con la velocidad horizontal.
        velocity.y = verticalVelocity;          // Añade la componente vertical (salto/caída).

        characterController.Move(velocity * Time.deltaTime);
        // Mueve el CharacterController según la velocidad total (horizontal + vertical) escalada por deltaTime.
        // Otros scripts pueden añadir movimientos extra llamando a Move en el mismo frame.
    }
}

