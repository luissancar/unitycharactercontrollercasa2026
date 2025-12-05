using UnityEngine;

public class CubePingPong : MonoBehaviour
{
    [Header("Puntos entre los que se moverá")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Velocidad de movimiento")]
    [SerializeField] private float speed = 2f;

    private Transform currentTarget;

    private void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("CubePingPong: Asigna pointA y pointB en el inspector.");
            enabled = false;
            return;
        }

        // Empezamos en A y vamos hacia B
        transform.position = pointA.position;
        currentTarget = pointB;
    }

    private void Update()
    {
        // Mover hacia el objetivo actual
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            speed * Time.deltaTime
        );

        // Si estamos muy cerca del objetivo, cambiar al otro
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.01f)
        {
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
        }
    }
}