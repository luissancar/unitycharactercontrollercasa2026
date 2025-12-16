// Importa el namespace base de Unity (MonoBehaviour, GameObject, Transform, etc.)

using UnityEngine;

// Importa el Input System (InputAction, PlayerInput, callbacks, etc.)
using UnityEngine.InputSystem;

// Obliga a que el GameObject que tenga este script también tenga un CharacterController
// (Unity lo añadirá automáticamente si no existe)
[RequireComponent(typeof(CharacterController))]
public class PickAndDrop_InputSystem : MonoBehaviour
{
    // Cabecera visual en el Inspector para agrupar campos
    // Campo serializado (se ve en el Inspector aunque sea private)
    // Aquí arrastras el Transform del Empty "Mochila"
    [Header("Empty hijo del Player")] [SerializeField]
    private Transform mochila;

    // Cabecera para el bloque de Input System en el Inspector
    // Tooltip: texto de ayuda que aparece al pasar el ratón por encima en el Inspector
    // InputActionReference: referencia directa a una acción del Input System (la acción "Soltar")
    [Header("Input System")]
    [Tooltip(
        "Opcional: arrastra aquí la acción 'Soltar' (InputActionReference). Si lo dejas vacío, se busca por nombre en PlayerInput.")]
    [SerializeField]
    private InputActionReference soltarActionRef;


    // Tooltip para explicar este campo
    // Si no usas soltarActionRef, se buscará una acción que se llame exactamente como esto: "Soltar"
    [Tooltip("Nombre exacto de la acción en tu Input Actions Asset")] [SerializeField]
    private string soltarActionName = "Soltar";

    // Cabecera para ajustes de soltado
    [Header("Drop")]

    // Offset (desplazamiento) para soltar el objeto delante del player
    // (x=0, y=0.2, z=0.6) en espacio local del player
    [SerializeField]
    private Vector3 dropOffset = new Vector3(0f, 0.2f, 0.6f);

    // Guarda el objeto que llevas dentro de la mochila (null si no llevas nada)
    private GameObject objetoEnMochila;

    // Guarda la InputAction real que se va a usar para soltar
    private InputAction soltarAction;

    // Reset se llama al añadir el componente o al pulsar "Reset" en el Inspector
    // Suele usarse para autocompletar referencias
    private void Reset()
    {
        // Busca un hijo con nombre "Mochila" dentro del Player
        var t = transform.Find("Mochila");

        // Si existe, lo asigna automáticamente al campo mochila
        if (t != null) mochila = t;
    }

    // Awake se ejecuta al inicializar el objeto (antes de Start)
    private void Awake()
    {
        // 1) Si el usuario asigna un InputActionReference, lo usamos
        if (soltarActionRef != null)
        {
            // Guardamos la acción "Soltar" directamente desde la referencia
            soltarAction = soltarActionRef.action;
        }
        else
        {
            // 2) Si no hay referencia, buscamos la acción por nombre en el componente PlayerInput del Player
            var playerInput = GetComponent<PlayerInput>();

            // Si existe PlayerInput, buscamos dentro de su asset una acción llamada "Soltar"
            if (playerInput != null)
                soltarAction = playerInput.actions.FindAction(soltarActionName, throwIfNotFound: false);
        }

        // Si no encontramos la acción, avisamos por consola para que lo arregles
        if (soltarAction == null)
            Debug.LogError(
                $"No se encontró la acción '{soltarActionName}'. Asigna soltarActionRef o añade PlayerInput con esa acción.");
    }

    // OnEnable se llama cada vez que el componente se activa (o al arrancar si está activo)
    private void OnEnable()
    {
        // Solo si existe la acción (no es null)
        if (soltarAction != null)
        {
            // Nos suscribimos al evento performed (cuando se ejecuta la acción)
            soltarAction.performed += OnSoltarPerformed;

            // Activamos la acción (por si no estaba activada)
            soltarAction.Enable();
        }
    }

    // OnDisable se llama cuando el componente se desactiva
    private void OnDisable()
    {
        // Solo si existe la acción (no es null)
        if (soltarAction != null)
        {
            // Nos desuscribimos del evento (importante para evitar duplicados y fugas)
            soltarAction.performed -= OnSoltarPerformed;

            // Desactivamos la acción
            soltarAction.Disable();
        }
    }

    // Callback que se ejecuta cuando la acción "Soltar" se realiza
    // ctx contiene info del input (valor, dispositivo, fase...)
    // Aquí usamos expresión lambda para llamar directamente a Soltar()
    private void OnSoltarPerformed(InputAction.CallbackContext ctx) => Soltar();

    // Si tu objeto "Pick" tiene Collider con IsTrigger activado,
    // este método se ejecuta cuando el Player entra en el trigger
    private void OnTriggerEnter(Collider other) => TryPick(other.gameObject);

    // Si tu Player usa CharacterController y el collider NO es trigger,
    // Unity llama a este método al chocar el CharacterController con algo
    private void OnControllerColliderHit(ControllerColliderHit hit) => TryPick(hit.gameObject);

    // Intenta coger un objeto si cumple las condiciones
    private void TryPick(GameObject go)
    {
        // Si ya llevamos un objeto, no hacemos nada (mochila ocupada)
        if (objetoEnMochila != null) return;

        // Si el objeto no tiene tag "Pick", no hacemos nada
        if (!go.CompareTag("Pick")) return;

        // Si no hay mochila asignada, error y salimos
        if (mochila == null)
        {
            Debug.LogError("No hay mochila asignada.");
            return;
        }

        // Guardamos el objeto como el que está en la mochila
        objetoEnMochila = go;

        // Apagar físicas mientras está en mochila (si tiene Rigidbody)
        if (objetoEnMochila.TryGetComponent<Rigidbody>(out var rb))
        {
            // Paramos su velocidad lineal
            rb.linearVelocity = Vector3.zero;

            // Paramos su velocidad angular (rotación)
            rb.angularVelocity = Vector3.zero;

            // Lo ponemos cinemático (no le afecta la física)
            rb.isKinematic = true;
        }

        // Opcional: desactivar el collider del objeto para que no choque con el player mientras lo llevas
        if (objetoEnMochila.TryGetComponent<Collider>(out var col))
            col.enabled = false;

        // Metemos el objeto dentro de la mochila como hijo (cambia su parent)
        // worldPositionStays: false => al cambiar de padre, mantiene posición LOCAL (no mundial)
        objetoEnMochila.transform.SetParent(mochila, worldPositionStays: false);

        // Lo colocamos exactamente en el origen local del empty mochila
        objetoEnMochila.transform.localPosition = Vector3.zero;

        // Lo alineamos sin rotación local (rotación identidad)
        objetoEnMochila.transform.localRotation = Quaternion.identity;
    }

    // Suelta el objeto que haya en la mochila
    private void Soltar()
    {
        // Si no tenemos nada, no hacemos nada
        if (objetoEnMochila == null) return;

        // Quitamos el parent (ya no es hijo de la mochila)
        objetoEnMochila.transform.SetParent(null);

        // Soltar delante del player usando un offset en espacio local del player
        // TransformPoint convierte un offset local a posición mundial
        objetoEnMochila.transform.position = transform.TransformPoint(dropOffset);

        // Reactivar collider si lo desactivamos al cogerlo
        if (objetoEnMochila.TryGetComponent<Collider>(out var col))
            col.enabled = true;

        // Reactivar físicas si tiene Rigidbody
        if (objetoEnMochila.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = false;

        // Dejamos la mochila vacía
        objetoEnMochila = null;
    }
}