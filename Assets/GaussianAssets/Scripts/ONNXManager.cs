using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Sentis;
using System.Collections.Generic;

public class ONNXManager : MonoBehaviour
{
    //public VisualTreeAsset uiAsset;
    public ModelAsset rainModelAsset; // rainModel
    public ModelAsset snowModelAsset; // snowModel
    private Worker worker;
    private Model runtimeModel;
    
    private FloatField precipitationField;
    
    //Particle system
    //public ParticleSystem rainParticleSystem;
    //public ParticleSystem snowParticleSystem;
    //
    public ParticleCollisionHandler particleCollisionHandler;
    public SnowParticleCollisionHandler SnowParticleCollisionHandler;
    
    void OnEnable()
    {
        // Set up the UI and slider
        var root = GetComponent<UIDocument>().rootVisualElement;
        //var uiInstance = uiAsset.CloneTree();
        //root.Add(uiInstance);
        //Dropdown
        var dropdown = root.Q<DropdownField>("dropdown");
        dropdown.choices = new List<string> { "Lluvia", "Nieve" };
        dropdown.focusable = false;
        //dropdown.value = "Lluvia"; //Default value
        
        dropdown.RegisterValueChangedCallback(evt =>
        {
            ChangePhenomenonMode(evt.newValue); //Change the phenomenon and model
            
        });
        
        //Get slider intensity 
        var sliderInt = root.Q<SliderInt>("SliderInt");
        sliderInt.focusable = false;
        sliderInt.RegisterValueChangedCallback(evt =>
        {
            float sliderValue = evt.newValue;
            //Debug.Log("Slider Value: " + sliderValue);
            Debug.Log("evento test" + dropdown.value);
            
            if (dropdown.value == "Lluvia")
            {
                RunModel(sliderValue);
            }
            else if (dropdown.value  == "Nieve")
            {
                RunSnowModel(sliderValue);
            }
            
        });

        precipitationField = root.Q<FloatField>("precipitation");
        precipitationField.focusable = false;
        /*

        // Load the runtime model using ModelAsset
        runtimeModel = ModelLoader.Load(rainModelAsset);

        // Create a worker using the runtime model, selecting the backend
        worker = new Worker(runtimeModel, BackendType.GPUCompute); // Use BackendType.CPU if GPUCompute is not available*/
    }

    void LoadModel(ModelAsset modelAsset)
    {
        // Load the runtime model using ModelAsset
        runtimeModel = ModelLoader.Load(modelAsset);
        worker?.Dispose();
        // Create a worker using the runtime model, selecting the backend
        worker = new Worker(runtimeModel, BackendType.GPUCompute); // Use BackendType.CPU if GPUCompute is not available
    }

    void ChangePrecipitationField(float newValue)
    {
        precipitationField.value = newValue;
    }

    void ChangePhenomenonMode(string phenomenonMode)
    {
        if (phenomenonMode != null && phenomenonMode == "Lluvia")
        {
            LoadModel(rainModelAsset); //Load the rain mode
            particleCollisionHandler.activeParticleSystem.gameObject.SetActive(true);
            SnowParticleCollisionHandler.GetActiveParticleSystem().gameObject.SetActive(false);
            
        }
        else if(phenomenonMode != null && phenomenonMode == "Nieve")
        {
            LoadModel(snowModelAsset);
            particleCollisionHandler.activeParticleSystem.gameObject.SetActive(false); //rain particle system deactivate
            SnowParticleCollisionHandler.GetActiveParticleSystem().gameObject.SetActive(true); //snow particle system activate
        }
    }

    void RunModel(float intensityValue)
    {
        // Create input tensors for the model
        TensorShape intensityShape = new TensorShape(1, 1); // Adjust shape to match the input
        Tensor<float> intensityTensor = new Tensor<float>(intensityShape, new float[] { intensityValue });

        // Set the inputs on the worker
        worker.SetInput("UserIntensity", intensityTensor);

        // Schedule the model for execution
        worker.Schedule(intensityTensor);

        // Retrieve the output tensor
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>; // Ensure this matches the expected type

        // Get the results from the output tensor
        float[] results = outputTensor.DownloadToArray();

        // Check the array length to avoid index errors
        if (results.Length > 2)
        {
            Debug.Log("Model Outputs : " + results);
            AdjustParticleSystemEmission(results); // Use the third position to adjust the emission rate
        }
        else
        {
            Debug.LogError("Model output array is too short. Expected at least 3 elements.");
        }
        
        ChangePrecipitationField(results[0]);

        // Release resources
        intensityTensor.Dispose();
        outputTensor.Dispose();
    }

    void RunSnowModel(float intensityValue)
    {
        // Create input tensors for the model
        TensorShape intensityShape = new TensorShape(1, 1); // Adjust shape to match the input
        Tensor<float> intensityTensor = new Tensor<float>(intensityShape, new float[] { intensityValue });

        // Set the inputs on the worker
        worker.SetInput("UserIntensity", intensityTensor);

        // Schedule the model for execution
        worker.Schedule(intensityTensor);

        // Retrieve the output tensor
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>; // Ensure this matches the expected type

        // Get the results from the output tensor
        float[] results = outputTensor.DownloadToArray();

        // Check the array length to avoid index errors
        if (results.Length > 2)
        {
            Debug.Log("Model Outputs : " + results);
            AdjustSnowParticle(results); // Use the third position to adjust the emission rate
        }
        else
        {
            Debug.LogError("Model output array is too short. Expected at least 3 elements.");
        }

        // Release resources
        intensityTensor.Dispose();
        outputTensor.Dispose();
        
    }
    
    void AdjustParticleSystemEmission(float[] data)
    {
        Debug.Log("Raw Emission Rate from Model: (emissioRate) " + data[4]);
        
        if (particleCollisionHandler != null && particleCollisionHandler.activeParticleSystem != null)
        {
            var emission = particleCollisionHandler.activeParticleSystem.emission;
            emission.rateOverTime = Mathf.Max(data[4], 0); 
            //emission.rateOverTime = emissionRate;
            
            //Force over life time
            var force = particleCollisionHandler.activeParticleSystem.forceOverLifetime;
            force.enabled = true;
            force.x = data[2];
            force.y = data[3];

        }
        
        //var emission = particleSystem.emission;
        // Set the clamped and scaled emission rate to the particle system
        
    }
    
    void AdjustSnowParticle(float[] data)
    {
        Debug.Log("Raw Emission Rate from Model: (emissioRate) " + data[4]);
        
        if (SnowParticleCollisionHandler != null && SnowParticleCollisionHandler.activeParticleSystem != null)
        {
            var emission = SnowParticleCollisionHandler.activeParticleSystem.emission;
            emission.rateOverTime = Mathf.Max(data[4], 0); 
            //emission.rateOverTime = emissionRate;
            
            //Force over life time
            var force = SnowParticleCollisionHandler.activeParticleSystem.forceOverLifetime;
            force.enabled = true;
            force.x = data[2];
            force.y = data[3];

        }
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}
