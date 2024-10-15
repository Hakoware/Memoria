using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneChangerUIToolkit : MonoBehaviour
{
    private Button sceneChangeButton;

    // Asigna el nombre de la escena a la que quieres cambiar
    public string sceneToLoad;

    void OnEnable()
    {
        // Obtén el UIDocument del componente
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Encuentra el botón por su nombre
        sceneChangeButton = root.Q<Button>("sceneChangeButton");

        // Añadir el evento para cuando se haga clic
        sceneChangeButton.clicked += () => ChangeScene();
    }

    // Método que cambia de escena
    void ChangeScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}