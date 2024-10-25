using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;


public class SnowParticleCollisionHandler : MonoBehaviour
{
    public ParticleSystem particleSystemPrefab;
    // Object reference
    public Transform referenceObject;
    public Material snowModelAsset;
    public Material meshMaterial;

    // Public particle system
    public ParticleSystem activeParticleSystem;

    public GameObject meshTarget;
    void Start()
    {

        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.white;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.02f;

        if (meshTarget != null)
        {
            MeshRenderer meshRenderer = meshTarget.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshMaterial != null)
            {
                meshRenderer.material = meshMaterial;
                Debug.Log("Material assigned successfully to target object.");
            }
            else if (meshRenderer == null)
            {
                Debug.LogWarning("No MeshRenderer found on the target object.");
            }
            else
            {
                Debug.LogWarning("Material is null.");
            }
        }
        else
        {
            Debug.LogWarning("Target object is not assigned.");
        }


        // Create particle system
        if (referenceObject != null)
        {
            GameObject particleObject = new GameObject("SnowParticleSystem");
            // Height start
            float additionalHeight = 5f;
            Vector3 particlePosition = referenceObject.position + Vector3.up * additionalHeight;

            particleObject.transform.position = referenceObject.position + Vector3.up * additionalHeight;
            // Instance
            activeParticleSystem = particleObject.AddComponent<ParticleSystem>();

            activeParticleSystem.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            ConfigureSnowParticleSystem(activeParticleSystem);

            activeParticleSystem.Play();
        }
        else
        {
            Debug.LogWarning("The prefab of the particle system or the reference object is not assigned");
        }
    }

    void ConfigureSnowParticleSystem(ParticleSystem snowParticleSystem)
    {
        // Rotate the particle system
        // Gravity and main module
        var mainModule = snowParticleSystem.main;
        mainModule.startSpeed = Random.Range(0.5f, 2.0f);
        mainModule.gravityModifier = 0.05f;
        mainModule.startLifetime = Random.Range(5f, 10f);
        mainModule.scalingMode = ParticleSystemScalingMode.Shape;
        mainModule.startSize = Random.Range(0.02f, 0.05f);
        mainModule.maxParticles = 10000;
        //mainModule.simulationSpace = ParticleSystemSimulationSpace.World; 

        // Emission rate for continuous snow
        var emissionModule = snowParticleSystem.emission;
        emissionModule.enabled = true;

        // Emission area (ideal wide box)
        var shapeModule = snowParticleSystem.shape;
        shapeModule.shapeType = ParticleSystemShapeType.Rectangle;
        shapeModule.scale = new Vector3(10.0f, 10.0f, 10.0f);
        shapeModule.rotation = new Vector3(0f, 0f, 0f);

        // Collision handler
        var collisionModule = snowParticleSystem.collision;
        collisionModule.enabled = true;
        collisionModule.enableDynamicColliders = true;
        collisionModule.type = ParticleSystemCollisionType.World;
        collisionModule.collidesWith = LayerMask.GetMask("Default", "Ground", "MeshColliderLayer");
        collisionModule.bounce = 0.1f;
        collisionModule.lifetimeLoss = 0.8f;  // 
        collisionModule.dampen = 0.7f; // More dampening for softer landing
        collisionModule.quality = ParticleSystemCollisionQuality.High;

        // Renderer (material and mesh)
        var renderer = snowParticleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        //renderer.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx"); 
        renderer.cameraVelocityScale = 0.0f;
        renderer.velocityScale = 0.01f;
        renderer.lengthScale = 1f;

        if (snowModelAsset != null)
        {
            renderer.material = snowModelAsset;
            renderer.material.color = Color.white;
        }
        else
        {
            Debug.LogWarning("No material assigned");
        }

        // Add wind effect to make snow more dynamic

        var forceOverLifetime = snowParticleSystem.forceOverLifetime;
        forceOverLifetime.enabled = true;
        forceOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        forceOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

        // Add noise module for random wind variation

        var noiseModule = snowParticleSystem.noise;
        noiseModule.enabled = true;
        noiseModule.strength = new ParticleSystem.MinMaxCurve(0.1f, 0.3f); //
        noiseModule.frequency = 0.5f; // Frequency of the noise effect
        noiseModule.scrollSpeed = 0.2f; // Speed at which the noise moves
    }

    public ParticleSystem GetActiveParticleSystem()
    {
        return activeParticleSystem;
    }
}
