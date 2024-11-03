import open3d as o3d
import numpy as np
import argparse


# upload the clean .ply and view
def filter_by_dbscan(file_name,clean=False, eps=0.1, min_points=5):
    pcd = o3d.io.read_point_cloud(f"{file_name}.ply")
    print("DBSCAN clustering...")
    labels = np.array(pcd.cluster_dbscan(eps=eps, min_points=min_points, print_progress=True))
    if labels.size == 0:
        raise ValueError("DBSCAN could not find any cluster")
    max_label = labels.max()
    print(f"Clusters: {max_label + 1}")
    
    if max_label == -1:
        raise ValueError("Not valid cluster finded.")
    #Pick the biggest cluster
    largest_cluster_index = np.argmax(np.bincount(labels[labels >= 0]))
    pcd = pcd.select_by_index(np.where(labels == largest_cluster_index)[0])
    #Filter to delete noise
    pcd, _ = pcd.remove_statistical_outlier(nb_neighbors=20, std_ratio=2.0)

    if clean:
        o3d.io.write_point_cloud(f"clean_{file_name}.ply", pcd)
    else:
        return pcd

def meshBPA(pcd, file_name):
    o3d.visualization.draw_geometries([pcd], window_name="Clean point cloud")
    # filter to delete noise
    pcd = pcd.voxel_down_sample(voxel_size=0.005)
    pcd.estimate_normals(search_param=o3d.geometry.KDTreeSearchParamHybrid(radius=0.1, max_nn=30))
    pcd.orient_normals_consistent_tangent_plane(k=10)

    # mesh reconstruction with Ball Pivoting Algorithm
    radii = [0.005, 0.01, 0.015, 0.02]  
    mesh_bpa = o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(
        pcd, o3d.utility.DoubleVector(radii))

    # clean up
    mesh_bpa.remove_degenerate_triangles()
    mesh_bpa.remove_duplicated_triangles()
    mesh_bpa.remove_duplicated_vertices()
    mesh_bpa.remove_non_manifold_edges()
    mesh_bpa = mesh_bpa.simplify_quadric_decimation(target_number_of_triangles=50000)


    o3d.visualization.draw_geometries([mesh_bpa], window_name="BPA mesh")
    o3d.io.write_triangle_mesh(f"{file_name}BPA.obj", mesh_bpa)
    print(f"Save as {file_name}BPA.obj'.")


def meshPoisson(pcd, file_name):
    # Poisson Surface Reconstruction 
    mesh_poisson, densities = o3d.geometry.TriangleMesh.create_from_point_cloud_poisson(pcd, depth=12, scale=1.1)

    # low density vertex
    vertices_to_remove = densities < np.quantile(densities, 0.02) 
    mesh_poisson.remove_vertices_by_mask(vertices_to_remove)

    # clean up
    mesh_poisson.remove_degenerate_triangles()
    mesh_poisson.remove_duplicated_triangles()
    mesh_poisson.remove_duplicated_vertices()
    mesh_poisson.remove_non_manifold_edges()

    # save and view
    o3d.visualization.draw_geometries([mesh_poisson], window_name="Poisson mesh")
    o3d.io.write_triangle_mesh(f"{file_name}Poisson.obj", mesh_poisson)
    print(f"save as '{file_name}Poisson.obj'.")


def main(method, file_name):
    """
    - clean: Just clean the point cloud
    - poisson: Poisson method recosntruction
    - bpa: Ball pivoting method reconstruction
    """
    if method == "clean":
        filter_by_dbscan(file_name, clean=True)
    elif method == "poisson":
        pcd = filter_by_dbscan(file_name)
        meshPoisson(pcd, file_name)
    elif method == "bpa":
        pcd = filter_by_dbscan(file_name)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Point cloud processing")
    parser.add_argument("method", type=str, help="Method to use: 'clean', 'poisson', or 'bpa'")
    parser.add_argument("file_name", type=str, help="The name of the input .ply file (without extension)")

    args = parser.parse_args()
    main(args.method, args.file_name)
