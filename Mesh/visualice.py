import open3d as o3d
import numpy as np
import argparse


def visualicePLY(ply_name):
    # upload ply
    pcd = o3d.io.read_point_cloud(f"{ply_name}.ply")
    # view the point cloud
    o3d.visualization.draw_geometries([pcd], window_name="Point cloud")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Point cloud processing")
    parser.add_argument("file_name", type=str, help="The name of the input .ply file (without extension)")

    args = parser.parse_args()
    visualicePLY(args.file_name)