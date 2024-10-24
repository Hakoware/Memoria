using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneChangerUIToolkit : MonoBehaviour
{
    private Button sceneChangeButton;
    
    //public string sceneToLoad;

    void OnEnable()
    {
        
        var root = GetComponent<UIDocument>().rootVisualElement;
        /*
        sceneChangeButton = root.Q<Button>("sceneChangeButton");
        sceneChangeButton.focusable = false;
        sceneChangeButton.clicked += () => ChangeScene();*/
        
        //Dropdown
        var dropdown = root.Q<DropdownField>("Scene");
        dropdown.choices = new List<string> {"Tree", "Playground", "Bench"};
        dropdown.focusable = false;

        dropdown.RegisterValueChangedCallback(e =>
        {
            ChangeScene(e.newValue);
        });

    }
    void ChangeScene(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}