using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ChangeColorByVerticalState : MonoBehaviour
{
    [Header("Colores por estado")]
    [SerializeField] private Color groundedColor = Color.white;   // En suelo
    [SerializeField] private Color goingUpColor = Color.green;    // Subiendo
    [SerializeField] private Color fallingColor = Color.red;      // Cayendo

    [Header("Ajustes")]
    [SerializeField] private float verticalDeadZone = 0.01f;

    [Header("Renderer que queremos colorear (la cápsula)")]
    [SerializeField] private Renderer capsuleRenderer;   // ← aquí arrastras la Capsule

    private CharacterController controller;

    private enum VerticalState { Grounded, GoingUp, Falling }
    private VerticalState currentState = VerticalState.Grounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Si no se ha asignado en el inspector, intentamos buscarla automáticamente
        if (capsuleRenderer == null)
        {
            // Busca el primer Renderer en los hijos cuyo nombre contenga "Capsule"
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject.name.Contains("Capsule"))
                {
                    capsuleRenderer = r;
                    break;
                }
            }
        }

        if (capsuleRenderer == null)
        {
            Debug.LogWarning("No se encontró Renderer de la cápsula en " + name);
            return;
        }

        SetColor(groundedColor);
    }

    private void Update()
    {
        if (controller == null || capsuleRenderer == null) return;

        float vy = controller.velocity.y;
        bool grounded = controller.isGrounded;

        VerticalState newState;

        if (grounded)
            newState = VerticalState.Grounded;
        else if (vy > verticalDeadZone)
            newState = VerticalState.GoingUp;
        else if (vy < -verticalDeadZone)
            newState = VerticalState.Falling;
        else
            newState = currentState; // zona muerta

        if (newState != currentState)
        {
            currentState = newState;

            switch (currentState)
            {
                case VerticalState.Grounded: SetColor(groundedColor); break;
                case VerticalState.GoingUp:  SetColor(goingUpColor);   break;
                case VerticalState.Falling:  SetColor(fallingColor);   break;
            }
        }
    }

    private void SetColor(Color color)
    {
        if (capsuleRenderer == null || capsuleRenderer.material == null) return;

        if (capsuleRenderer.material.HasProperty("_Color"))
            capsuleRenderer.material.color = color;
        else if (capsuleRenderer.material.HasProperty("_BaseColor"))
            capsuleRenderer.material.SetColor("_BaseColor", color);
    }
}
