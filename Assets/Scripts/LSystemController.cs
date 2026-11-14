using UnityEngine;
using System.Collections.Generic;

public class LSystemController : MonoBehaviour
{
    [Header("L-System Settings")]
    [SerializeField] private string axiom = "F";
    [SerializeField, Range(0, 7)] private int iterations = 3;
    [SerializeField] private float angle = 25.7f;
    [SerializeField] private float segmentLength = 0.1f;

    private void Start()
    {
        Dictionary<char, string> rules = new Dictionary<char, string>
        {
            { 'F', "F[+F]F[-F]F" }
        };

        LSystemGenerator generator = new LSystemGenerator(rules);
        Turtle turtle = new Turtle(angle, segmentLength);

        // The generator sends each symbol straight to the turtle; no string is built
        generator.Generate(axiom, iterations, turtle.Process);

      // Build one line mesh from all segments (visible in Game view)
        BuildMesh(turtle.segments);

        Debug.Log($"Segments drawn: {turtle.segments.Count}");

        FitCamera();
    }

        private void BuildMesh(List<(Vector3 start, Vector3 end)> segments)
    {
        Vector3[] vertices = new Vector3[segments.Count * 2];
        int[] indices = new int[segments.Count * 2];

        for (int i = 0; i < segments.Count; i++)
        {
            vertices[i * 2]     = segments[i].start;
            vertices[i * 2 + 1] = segments[i].end;
            indices[i * 2]      = i * 2;
            indices[i * 2 + 1]  = i * 2 + 1;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void FitCamera()
    {
        Camera cam = Camera.main;
        Bounds b = GetComponent<MeshRenderer>().bounds;

        cam.orthographic = true;
        cam.transform.position = new Vector3(b.center.x, b.center.y, -10f);
        cam.transform.rotation = Quaternion.identity;

        float halfHeight = b.extents.y;
        float halfWidth = b.extents.x / cam.aspect;
        cam.orthographicSize = Mathf.Max(halfHeight, halfWidth) * 1.1f;   // 10% margin
    }
}