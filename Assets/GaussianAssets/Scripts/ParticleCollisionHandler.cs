using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ParticleCollisionHandler : MonoBehaviour
{
    public ParticleSystem particleSystemPrefab;
    // Object reference
    public Transform referenceObject;
    public Material rainDropMaterial;
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
        if (referenceObject != null)
        {
            GameObject particleObject = new GameObject("RainParticleSystem");
            // Height start
            float additionalHeight = 10f; 
            Vector3 particlePosition = referenceObject.position + Vector3.up * additionalHeight;
            
            particleObject.transform.position = referenceObject.position + Vector3.up *additionalHeight;
            // Instance
            //activeParticleSystem = Instantiate(particleSystemPrefab, particlePosition, Quaternion.identity);
            activeParticleSystem = particleObject.AddComponent<ParticleSystem>();
            // Ratotion to aling "y" axis
            activeParticleSystem.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            // Configure the particle system
            ConfigureRainParticleSystem(activeParticleSystem);
            // Configure the splash
            ConfigureSplashSubEmitter(activeParticleSystem);
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
        Debug.Log("Se configura o no??");
        var mainModule = rainParticleSystem.main;
        mainModule.startSpeed = Random.Range(15.0f, 20.0f);
        mainModule.gravityModifier = 9.8f; 
        mainModule.startLifetime = Random.Range(1.0f, 2.0f); 
        mainModule.scalingMode = ParticleSystemScalingMode.Shape;
        mainModule.startSize = Random.Range(0.005f, 0.008f);
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
        renderer.lengthScale = 1f;

        //renderer.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx"); // Sphere render
        //renderer.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

        renderer.mesh = null;
        if (rainDropMaterial != null)
        {
            renderer.material = rainDropMaterial;  // assign the material
        }
        else
        {
            Debug.LogWarning("Not material assigned");
        }

    }
    
    void ConfigureSplashSubEmitter(ParticleSystem parentParticleSystem)
    {
        // Crear el sistema de partículas del splash
        ParticleSystem splashParticleSystem = new GameObject("SplashParticles").AddComponent<ParticleSystem>();
        splashParticleSystem.transform.SetParent(parentParticleSystem.transform); 
        
        // Configuración del subemitter (splash)
        var mainModule = splashParticleSystem.main;
        mainModule.startLifetime = 0.3f;
        //mainModule.startSpeed = 1.0f;
        mainModule.startSpeed = Random.Range(0.5f, 1.0f);
        mainModule.startSize = 0.01f;
        mainModule.startSize = Random.Range(0.005f, 0.01f);
        mainModule.gravityModifier = 0.5f;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        
        //Emission
        var emissionModule = splashParticleSystem.emission;
        emissionModule.rateOverTime = 0;
        //emissionModule.burstCount = 3;
        emissionModule.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });
        
        //Size over life time
        var sizeOverLifeTime = splashParticleSystem.sizeOverLifetime;
        sizeOverLifeTime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        //sizeCurve.AddKey(0.0f, 0.8f);
        //sizeCurve.AddKey(1.0f, 0.0f); 
        
        sizeCurve.AddKey(0.0f, 0.0f);  // Inicio (valor = 0 en tiempo 0)
        sizeCurve.AddKey(0.2f, 0.5f);  // Punto intermedio (aumenta)
        sizeCurve.AddKey(0.5f, 1.0f);  // Pico máximo en el medio
        sizeCurve.AddKey(0.8f, 0.5f);  // Disminuye
        sizeCurve.AddKey(1.0f, 0.0f);  // Fin (valor = 0 en tiempo 1)

        sizeOverLifeTime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

        //Shape
        var shapeModule = splashParticleSystem.shape;
        shapeModule.shapeType = ParticleSystemShapeType.Cone;
        shapeModule.angle = 45f;
        shapeModule.radius = 0.001f;
        shapeModule.position = Vector3.zero;
        //shapeModule.rotation = new Vector3(-180f, 0f, 0f); 
        shapeModule.rotation = new Vector3(30f, 0f, 0f);
        shapeModule.rotation = new Vector3(0f, 0f, 0f); 
        
        //Renderer
        var renderer = splashParticleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 0.001f;
        renderer.lengthScale = 2f;
        
        renderer.mesh = null;
        //renderer.material = rainDropMaterial; 
        if (rainDropMaterial != null)
        {
            renderer.material = rainDropMaterial;  // assign the material
        }
        else
        {
            Debug.LogWarning("Not material assigned");
        }

        // Añadir el subemitter al sistema de partículas de lluvia
        var subEmittersModule = parentParticleSystem.subEmitters;
        subEmittersModule.enabled = true;
        subEmittersModule.AddSubEmitter(splashParticleSystem, ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritNothing);
    }
    
    public ParticleSystem GetActiveParticleSystem()
    {
        return activeParticleSystem;
    }
    

    
    
    
    
}
