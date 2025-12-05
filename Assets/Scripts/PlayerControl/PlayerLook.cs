using UnityEngine;                     // Importa el namespace principal de Unity.
using UnityEngine.InputSystem;         // Importa el namespace del nuevo Input System.

public class PlayerLook : MonoBehaviour // Declara la clase PlayerLook, que hereda de MonoBehaviour.
{
    [Header("Referencias")]                     // Agrupa estos campos bajo el encabezado "Referencias" en el inspector.
    public Transform cameraTransform;           // Referencia a la Transform de la cámara (normalmente hija del player).

    [Header("Mirar (ratón)")]                   // Agrupa estos campos bajo "Mirar (ratón)" en el inspector.
    public float mouseSensitivity = 120f;       // Sensibilidad del ratón (grados por segundo).
    public float minPitch = -40f;               // Límite mínimo de inclinación vertical de la cámara (mirar abajo).
    public float maxPitch = 70f;                // Límite máximo de inclinación vertical de la cámara (mirar arriba).

    private Vector2 lookInput;                  // Almacena el input de mirar (X = horizontal, Y = vertical).
    private float cameraPitch;                  // Ángulo actual de pitch acumulado para la cámara.

    private void Awake()                        // Awake se llama al inicializar el objeto, antes de Start.
    {
        if (cameraTransform == null && Camera.main != null) // Si no se ha asignado la cámara pero existe Camera.main...
            cameraTransform = Camera.main.transform;        // ...usa la cámara principal como referencia.
    }

    private void Start()                        // Start se ejecuta antes del primer frame de Update.
    {
        // Dejar el player sin inclinación en X/Z (solo yaw inicial)
        float yaw = transform.eulerAngles.y;    // Toma el ángulo de rotación Y actual del jugador (dirección horizontal).
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        // Fija la rotación del jugador a (X=0, Y=yaw, Z=0), eliminando inclinaciones hacia arriba/abajo o laterales.

        // Cámara mirando recto
        cameraPitch = 0f;                       // Inicializa el pitch de la cámara a 0 (mirando recto).
        lookInput = Vector2.zero;              // Resetea el input de mirar a cero para evitar rotaciones iniciales.

        if (cameraTransform != null)           // Si hay cámara asignada...
            cameraTransform.localRotation = Quaternion.identity;
            // ...resetea su rotación local a (0,0,0) para que empiece mirando recto.
    }

    // INPUT SYSTEM (Send Messages) -> acción "Look"
    private void OnLook(InputValue value)       // Método llamado automáticamente por PlayerInput cuando se activa la acción "Look".
    {
        lookInput = value.Get<Vector2>();       // Convierte el valor de entrada a Vector2 y lo guarda en lookInput.
    }

    private void Update()                       // Update se llama una vez por frame.
    {
        if (cameraTransform == null)            // Si no hay cámara asignada...
            return;                             // ...salimos para evitar errores.

        HandleLook();                           // Gestiona la rotación del jugador y la cámara según el input.
    }

    private void HandleLook()                   // Método que implementa la lógica de mirar con el ratón/stick.
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime; // Calcula la rotación horizontal según el input y la sensibilidad.
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime; // Calcula la rotación vertical según el input y la sensibilidad.

        // Girar el cuerpo del player (yaw)
        transform.Rotate(0f, mouseX, 0f);       // Rota el transform del jugador alrededor del eje Y (izquierda/derecha).

        // Acumular pitch de la cámara
        cameraPitch -= mouseY;                  // Resta mouseY para que mover el ratón hacia arriba mire hacia arriba.
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch); // Limita el pitch entre minPitch y maxPitch.

        // Aplicar rotación a la cámara solo en X
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        // Aplica la rotación local a la cámara: X = pitch calculado, Y = 0 (la lleva el player), Z = 0 (sin inclinación lateral).
    }
}
