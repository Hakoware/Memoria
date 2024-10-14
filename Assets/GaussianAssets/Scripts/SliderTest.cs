using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public VisualTreeAsset uiAsset;

    void OnEnable()
    {

        var root = GetComponent<UIDocument>().rootVisualElement;
        var uiInstance = uiAsset.CloneTree();
        root.Add(uiInstance);
        var sliderInt = root.Q<SliderInt>("SliderInt");
        sliderInt.RegisterValueChangedCallback(evt =>
        {
            //Debug.Log("Valor del slider: " + evt.newValue);
            //Debug.Log("A: " + evt.newValue);
        });
    }
}