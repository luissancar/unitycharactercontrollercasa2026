using UnityEngine;                      // Usamos el namespace de Unity para acceder a tipos básicos como MonoBehaviour, CharacterController, etc.
using UnityEngine.InputSystem;          // Usamos el nuevo Input System para poder utilizar InputValue y recibir input desde PlayerInput.

[RequireComponent(typeof(CharacterController))] // Obligamos a que el GameObject tenga un CharacterController; si no lo tiene, Unity lo añade automáticamente.
public class SlopeSlideOnly : MonoBehaviour     // Definimos la clase SlopeSlideOnly, que solo se encargará del deslizamiento en pendientes.
{
    [Header("Deslizamiento en pendiente")]      // Agrupa las siguientes variables en el inspector bajo este encabezado.
    public float slideSpeed = 4f;               // Velocidad a la que el personaje se deslizará por la pendiente.
    public float minSlopeAngleToSlide = 3f;     // Ángulo mínimo (en grados) de la pendiente a partir del cual comenzará a deslizarse.

    private CharacterController controller;     // Referencia al CharacterController que controla las colisiones y el movimiento del personaje.
    private Vector2 moveInput;                  // Vector2 que almacena el input de movimiento del jugador (X = horizontal, Y = vertical).

    void Awake()                                // Método Awake, llamado una vez cuando se instancia el objeto antes que Start.
    {
        controller = GetComponent<CharacterController>(); // Obtenemos y guardamos la referencia al CharacterController del mismo GameObject.
    }

    // Este método será llamado automáticamente por PlayerInput (Behavior = Send Messages) cuando se dispare la acción "Move".
    public void OnMove(InputValue value)        // Método público llamado OnMove que recibe un InputValue del sistema de Input.
    {
        moveInput = value.Get<Vector2>();       // Extraemos el Vector2 del InputValue y lo guardamos en moveInput para saber si hay input del jugador.
    }

    void Update()                               // Método Update, llamado una vez por frame.
    {
        if (!controller.isGrounded)             // Si el CharacterController NO está tocando el suelo...
            return;                             // ...salimos de Update, no queremos deslizar en pendiente cuando está en el aire.

        bool hasInput = moveInput.sqrMagnitude > 0.01f; // Calculamos si hay input significativo comprobando la magnitud al cuadrado del vector (más barato que usar .magnitude).
        if (hasInput)                           // Si el jugador está dando alguna entrada de movimiento...
            return;                             // ...no aplicamos deslizamiento automático, dejamos que su script de movimiento controle todo.

        if (!IsOnSlope(out RaycastHit hitInfo)) // Llamamos a IsOnSlope para comprobar si estamos sobre una pendiente, obteniendo también el RaycastHit.
            return;                             // Si no estamos en una pendiente (o no se detecta superficie), salimos y no hacemos nada.

        float angle = Vector3.Angle(hitInfo.normal, Vector3.up); // Calculamos el ángulo entre la normal del suelo y el vector hacia arriba (0° = suelo plano, >0° = pendiente).

        if (angle < minSlopeAngleToSlide)       // Si el ángulo es menor que el ángulo mínimo configurado para deslizar...
            return;                             // ...no consideramos que la pendiente sea suficiente como para deslizar, así que salimos.

        Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hitInfo.normal).normalized;
        // Calculamos la dirección de deslizamiento proyectando el vector hacia abajo (Vector3.down) sobre el plano definido por la normal de la superficie.
        // Esto nos da un vector que “sigue” la pendiente hacia abajo.

        Vector3 displacement = slideDir * slideSpeed * Time.deltaTime;
        // Calculamos el desplazamiento final en el frame actual multiplicando la dirección de deslizamiento por la velocidad y por deltaTime.

        controller.Move(displacement);          // Movemos el CharacterController según el desplazamiento calculado. Este movimiento se suma al movimiento de otros scripts que también llamen a Move.
    }

    bool IsOnSlope(out RaycastHit hit)          // Método que comprueba si el personaje está sobre una pendiente, devolviendo también información del Raycast.
    {
        Vector3 origin = controller.bounds.center;     // Definimos el origen del Raycast en el centro del CharacterController (más fiable que transform.position si el pivot no está centrado).
        float rayLength = (controller.height / 2f) + 0.5f;
        // Definimos la longitud del Raycast: la mitad de la altura del CharacterController más un pequeño margen adicional.

        if (Physics.Raycast(origin, Vector3.down, out hit, rayLength))
        // Lanzamos un Raycast hacia abajo desde el centro del CharacterController. Si golpea algo dentro de rayLength...
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            // Calculamos el ángulo entre la normal del punto de impacto y el vector hacia arriba para saber la inclinación del suelo.

            return angle > 0.01f;               // Devolvemos true si el ángulo es mayor que un valor casi cero (es decir, no es completamente plano).
        }

        return false;                           // Si el Raycast no golpea nada, devolvemos false e indicamos que no estamos sobre una pendiente válida.
    }
}

