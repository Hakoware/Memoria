using UnityEngine;
using UnityEngine.UIElements;

public class SlidingMenu : MonoBehaviour
{
    public RectTransform menuPanel; // El panel que se moverá
    public Button toggleButton; // El botón que activará el menú
    public float slideSpeed = 500f; // La velocidad de desplazamiento del menú
    private bool isMenuOpen = false; // Estado del menú

    private Vector2 closedPosition; // La posición cerrada del menú
    private Vector2 openPosition; // La posición abierta del menú

    void Start()
    {
        // Define las posiciones del menú cuando está abierto y cerrado
        closedPosition = new Vector2(-menuPanel.rect.width, menuPanel.anchoredPosition.y);
        openPosition = new Vector2(0, menuPanel.anchoredPosition.y);

        // Asegurarse de que el menú comienza cerrado
        menuPanel.anchoredPosition = closedPosition;

        // Añadir el evento al botón para abrir o cerrar el menú
        toggleButton.RegisterCallback<ClickEvent>(evt => ToggleMenu());
    }

    void ToggleMenu()
    {
        // Si el menú está abierto, lo cerramos. Si está cerrado, lo abrimos.
        if (isMenuOpen)
        {
            StartCoroutine(SlideMenu(closedPosition)); // Desplazar hacia la izquierda (cerrar)
        }
        else
        {
            StartCoroutine(SlideMenu(openPosition)); // Desplazar hacia la derecha (abrir)
        }

        // Cambiar el estado del menú
        isMenuOpen = !isMenuOpen;
    }

    // Desliza el menú hacia la posición deseada
    System.Collections.IEnumerator SlideMenu(Vector2 targetPosition)
    {
        while (Vector2.Distance(menuPanel.anchoredPosition, targetPosition) > 0.1f)
        {
            // Mover el menú hacia la posición deseada a la velocidad definida
            menuPanel.anchoredPosition = Vector2.MoveTowards(menuPanel.anchoredPosition, targetPosition, slideSpeed * Time.deltaTime);
            yield return null;
        }

        // Asegurarse de que el menú llegue exactamente a la posición objetivo
        menuPanel.anchoredPosition = targetPosition;
    }
}
