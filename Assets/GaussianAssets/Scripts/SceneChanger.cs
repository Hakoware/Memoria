using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneChangerUIToolkit : MonoBehaviour
{
    private Button sceneChangeButton;
    
    public string sceneToLoad;

    void OnEnable()
    {
        
        var root = GetComponent<UIDocument>().rootVisualElement;
        sceneChangeButton = root.Q<Button>("sceneChangeButton");
        sceneChangeButton.focusable = false;
        sceneChangeButton.clicked += () => ChangeScene();
        
    }
    void ChangeScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}