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
        // Create particle system
        if (referenceObject != null)
        {
            GameObject particleObject = new GameObject("SnowParticleSystem");
            // Height start
            float additionalHeight = 10f; 
            Vector3 particlePosition = referenceObject.position + Vector3.up * additionalHeight;
            
            particleObject.transform.position = referenceObject.position + Vector3.up * 10f *additionalHeight;
            // Instance
            //activeParticleSystem = Instantiate(particleSystemPrefab, particlePosition, Quaternion.identity);
            activeParticleSystem = particleObject.AddComponent<ParticleSystem>();
            // Ratotion to aling "y" axis
            activeParticleSystem.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            // Configure the particle system
            ConfigureRainParticleSystem(activeParticleSystem);
            // Activate the particle system
            activeParticleSystem.Play();
        }
        else
        {
            Debug.LogWarning("The prefab of the particle system or the reference object is not assigned");
        }
    }

    void ConfigureRainParticleSystem(ParticleSystem rainParticleSystem)
    { 
        // Rotate the particle system
        // Gravity and main module
        var mainModule = rainParticleSystem.main;
        mainModule.startSpeed = Random.Range(10.0f, 20.0f);
        mainModule.gravityModifier = 9.8f; 
        mainModule.startLifetime = Random.Range(1.0f, 2.0f); 
        mainModule.scalingMode = ParticleSystemScalingMode.Shape;
        mainModule.startSize = Random.Range(0.01f, 0.03f);
        mainModule.maxParticles = 10000; 
        //test
        //mainModule.startSize = 0.5f;
        
        // emision area (ideal big box)
        var shapeModule = rainParticleSystem.shape;
        shapeModule.shapeType = ParticleSystemShapeType.Rectangle;  // Usa una caja para cubrir un área amplia
        shapeModule.scale = new Vector3(10.0f, 10.0f, 10.0f); 
        //shapeModule.position = new Vector3(0, 5.0f, 0);
        //shapeModule.rotation = new Vector3(90, 0, 0);
        //shapeModule.rotation = new Vector3(0f, 0f, 0f);
        
        // Collison handler
        var collisionModule = rainParticleSystem.collision;
        collisionModule.enabled = true;
        collisionModule.enableDynamicColliders = true;
        collisionModule.type = ParticleSystemCollisionType.World;
        collisionModule.collidesWith = LayerMask.GetMask("Default", "Ground", "MeshColliderLayer");
        //collisionModule.collidesWith = LayerMask.GetMask("Ground");
        collisionModule.bounce = 0.0f; // Bounce
        collisionModule.lifetimeLoss = 0.5f;  
        collisionModule.dampen = 1f;
        collisionModule.quality = ParticleSystemCollisionQuality.High;


        // render (material and mesh)
        var renderer = rainParticleSystem.GetComponent<ParticleSystemRenderer>();
        //renderer.renderMode = ParticleSystemRenderMode.Billboard;
        //renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.cameraVelocityScale = 0.0f;
        renderer.velocityScale = 0.01f;
        renderer.lengthScale = 4f;

        //renderer.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx"); // Sphere render
        //renderer.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

        renderer.mesh = null;
        if (snowModelAsset != null)
        {
            renderer.material = snowModelAsset;  // assign the material
        }
        else
        {
            Debug.LogWarning("Not material assigned");
        }

    }
    
    public ParticleSystem GetActiveParticleSystem()
    {
        return activeParticleSystem;
    }
    

    
    
    
    
}
