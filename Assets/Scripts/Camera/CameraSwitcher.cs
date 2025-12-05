using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // necesario para InputValue

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cámaras a alternar")]
    [Tooltip("Si se deja vacío, se buscarán automáticamente las cámaras hijas.")]
    [SerializeField] private List<Camera> cameras = new List<Camera>();

    private int currentIndex = 0;

    private void Awake()
    {
        // Si la lista está vacía en el inspector, rellenarla automáticamente
        if (cameras == null || cameras.Count == 0)
        {
            cameras = new List<Camera>(GetComponentsInChildren<Camera>());

            if (cameras.Count == 0)
            {
                Debug.LogWarning("CameraSwitcher: no se encontraron cámaras hijas en " + name);
                return;
            }
        }

        // Activamos solo la primera cámara al inicio
        SetActiveCamera(currentIndex);
    }

    /// <summary>
    /// Este método lo llama automáticamente el PlayerInput
    /// cuando se ejecuta la acción ChangeCamera (modo Send Messages).
    /// El nombre del método TIENE que ser OnChangeCamera.
    /// </summary>
    public void OnChangeCamera(InputValue value)
    {
        if (cameras == null || cameras.Count == 0) return;

        // Avanzamos al siguiente índice
        currentIndex++;
        if (currentIndex >= cameras.Count)
            currentIndex = 0;

        SetActiveCamera(currentIndex);
    }

    private void SetActiveCamera(int index)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            bool isActive = (i == index);
            if (cameras[i] != null)
            {
                // Activar/desactivar componente Camera
                cameras[i].enabled = isActive;

                // Opcional: activar/desactivar el GameObject completo
                // cameras[i].gameObject.SetActive(isActive);
            }
        }
    }
}