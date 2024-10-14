using UnityEngine;

public class SetupScene : MonoBehaviour
{
    public GameObject floorObject; 
    public GameObject spherePrefab; 

    void Start()
    {
        // Object config
        if (floorObject != null)
        {
            // Mesh collider
            MeshCollider meshCollider = floorObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = floorObject.AddComponent<MeshCollider>();
            }
            
            Rigidbody rb = floorObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = floorObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            floorObject.isStatic = true;
        }

        // Create sphere
        if (spherePrefab != null)
        {
            // Instance
            GameObject sphere = Instantiate(spherePrefab, new Vector3(0, 5, 0), Quaternion.identity);
            
            // Sphere collider
            SphereCollider sphereCollider = sphere.GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                sphereCollider = sphere.AddComponent<SphereCollider>();
            }

            // Rigidbody to activete the physics
            Rigidbody sphereRb = sphere.GetComponent<Rigidbody>();
            if (sphereRb == null)
            {
                sphereRb = sphere.AddComponent<Rigidbody>();
            }
            sphereRb.useGravity = true;
        }
    }
}