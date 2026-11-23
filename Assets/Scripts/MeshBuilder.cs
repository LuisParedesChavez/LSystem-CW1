using UnityEngine;
using System.Collections.Generic;

// Converts turtle output into Unity meshes. Each mesh is one object and one draw call.
public static class MeshBuilder
{
    // 32-bit indices: large L-systems go over the default 65,535-vertex limit
    public static Mesh CreateMesh(string name)
    {
        Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        return mesh;
    }

    // Branches: two vertices per segment, drawn as lines
    public static void BuildLines(Mesh mesh, List<(Vector3 start, Vector3 end)> segments)
    {
        mesh.Clear();

        Vector3[] vertices = new Vector3[segments.Count * 2];
        int[] indices = new int[segments.Count * 2];

        for (int i = 0; i < segments.Count; i++)
        {
            vertices[i * 2] = segments[i].start;
            vertices[i * 2 + 1] = segments[i].end;
            indices[i * 2] = i * 2;
            indices[i * 2 + 1] = i * 2 + 1;
        }

        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
    }

    // Leaves: one diamond (4 vertices) per leaf, both faces so they show from either side
    public static void BuildLeaves(Mesh mesh, List<Leaf> leaves, float baseSize)
    {
        mesh.Clear();

        Vector3[] vertices = new Vector3[leaves.Count * 4];
        int[] triangles = new int[leaves.Count * 12];

        for (int i = 0; i < leaves.Count; i++)
        {
            float size = baseSize * leaves[i].scale;
            float radians = leaves[i].heading * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);   // along the leaf
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);                            // sideways
            Vector3 p = leaves[i].position + new Vector3(0f, 0f, -0.01f);             // slightly in front of the lines

            int v = i * 4;
            vertices[v]     = p;                                                  // base
            vertices[v + 1] = p + dir * (size * 0.4f) + perp * (size * 0.25f);   // left
            vertices[v + 2] = p + dir * size;                                     // tip
            vertices[v + 3] = p + dir * (size * 0.4f) - perp * (size * 0.25f);   // right

            int t = i * 12;
            // Front faces
            triangles[t]     = v; triangles[t + 1]  = v + 1; triangles[t + 2]  = v + 2;
            triangles[t + 3] = v; triangles[t + 4]  = v + 2; triangles[t + 5]  = v + 3;
            // Back faces (same triangles, reversed)
            triangles[t + 6] = v; triangles[t + 7]  = v + 2; triangles[t + 8]  = v + 1;
            triangles[t + 9] = v; triangles[t + 10] = v + 3; triangles[t + 11] = v + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}