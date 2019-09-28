using UnityEngine;
using System.Collections;

public static class MeshOperations {

    //===================================================================
    private static Mesh scaleMesh(Mesh mesh, float scale)
    {
        Mesh scaledMesh = CloneMesh(mesh);
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            var vertex = mesh.vertices[i];
            vertex.x = vertex.x * scale;
            vertex.y = vertex.y * scale;
            vertex.z = vertex.z * scale;
            scaledMesh.vertices[i] = vertex;
        }

        scaledMesh.RecalculateNormals();
        scaledMesh.RecalculateBounds();
        ;

        return scaledMesh;
    }

    //===================================================================
    private static Mesh CloneMesh(Mesh mesh)
    {
        Mesh clone = new Mesh();
        clone.vertices = mesh.vertices;
        clone.normals = mesh.normals;
        clone.tangents = mesh.tangents;
        clone.triangles = mesh.triangles;
        clone.uv = mesh.uv;
        //clone.uv1 = mesh.uv1;
        clone.uv2 = mesh.uv2;
        clone.bindposes = mesh.bindposes;
        clone.boneWeights = mesh.boneWeights;
        clone.bounds = mesh.bounds;
        clone.colors = mesh.colors;
        clone.name = mesh.name;
        //TODO : Are we missing anything?
        return clone;
    }

    //===================================================================
    public static float VolumeOfMesh(Mesh mesh)
    {
        return VolumeOfMesh(mesh, 1);
    }

    //===================================================================
    public static float VolumeOfMesh(Mesh mesh, float scale)
    {
        float volume = 0;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]] * scale;
            Vector3 p2 = vertices[triangles[i + 1]] * scale;
            Vector3 p3 = vertices[triangles[i + 2]] * scale;
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }

        return Mathf.Abs(volume);
    }

    //===================================================================
    public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    //===================================================================
    public static float GetMeshSurfaceArea(Mesh Mesh)
    {
        return GetMeshSurfaceArea(Mesh, 1);
    }
    //===================================================================

    //===================================================================
    public static float GetMeshSurfaceArea(Mesh Mesh, float scale)
    {
        int[] triangles = Mesh.triangles;

        float area = 0;
        for (int i = 0; i<triangles.Length; i+= 3)
        {
            Vector3 v1 = Mesh.vertices[triangles[i + 0]] * scale;
            Vector3 v2 = Mesh.vertices[triangles[i + 1]] * scale;
            Vector3 v3 = Mesh.vertices[triangles[i + 2]] * scale;
            area += GetTriangleArea(v1, v2, v3);

        }

        return area;
    }

    //===================================================================
    public static float GetTriangleArea(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float a = Vector3.Distance(v1, v2);
        float b = Vector3.Distance(v2, v3);
        float c = Vector3.Distance(v1, v3);

        float halfPerimeter = (a + b + c) / 2;
        return Mathf.Sqrt(halfPerimeter * (halfPerimeter - a) * (halfPerimeter - b) * (halfPerimeter - c));
    }
    //===================================================================


}






