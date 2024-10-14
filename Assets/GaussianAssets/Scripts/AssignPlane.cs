using UnityEngine;

public class AlignPlaneWithMesh : MonoBehaviour
{
    public MeshCollider meshCollider;  
    public GameObject plane; 

    void Start()
    {
        if (meshCollider != null && plane != null)
        {
            // bounds of the mesh
            Bounds bounds = meshCollider.bounds;
            
            Debug.Log("bounds:"+ bounds.min.y);

            //Plane position
            Vector3 newPlanePosition = plane.transform.position;
            //above the lowest point of the mesh
            newPlanePosition.y = bounds.min.y; 

            // Move the plane
            plane.transform.position = newPlanePosition;
        }
        else
        {
            Debug.LogError("MeshCollider o Plane not assigned.");
        }
    }
}
