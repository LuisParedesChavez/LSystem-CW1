using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class LSystemController : MonoBehaviour
{
    [Header("L-System Settings")]
    [SerializeField] private string axiom = "F";
    [SerializeField, Range(0, 7)] private int iterations = 3;
    [SerializeField] private float angle = 25.7f;
    [SerializeField] private float segmentLength = 0.1f;

    private Mesh treeMesh;

    private void Start()
    {
        Regenerate();
    }

    private void Regenerate()
    {
        Dictionary<char, string> rules = new Dictionary<char, string>
        {
            { 'F', "F[+F]F[-F]F" }
        };

        LSystemGenerator generator = new LSystemGenerator(rules);
        Turtle turtle = new Turtle(angle, segmentLength);

        generator.Generate(axiom, iterations, turtle.Process);

        BuildMesh(turtle.segments);

        Debug.Log($"Iterations: {iterations} | Angle: {angle} | Length: {segmentLength} | Segments: {turtle.segments.Count}");

        FitCamera();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        bool changed = false;

        // Increase or decrease iterations
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            iterations = Mathf.Min(iterations + 1, 7);
            changed = true;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            iterations = Mathf.Max(iterations - 1, 0);
            changed = true;
        }

        // Change branch angle
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            angle += 1f;
            changed = true;
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            angle -= 1f;
            changed = true;
        }

        // Change segment length
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            segmentLength += 0.01f;
            changed = true;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            segmentLength = Mathf.Max(0.01f, segmentLength - 0.01f);
            changed = true;
        }

        // Regenerate only when parameters change
        if (changed)
        {
            Regenerate();
        }
    }
    private void BuildMesh(List<(Vector3 start, Vector3 end)> segments)
    {
        // Create the mesh only once
        if (treeMesh == null)
        {
            treeMesh = new Mesh();
            treeMesh.name = "L-System Tree Mesh";
            treeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            GetComponent<MeshFilter>().sharedMesh = treeMesh;
        }

        // Remove previous geometry
        treeMesh.Clear();

        Vector3[] vertices = new Vector3[segments.Count * 2];
        int[] indices = new int[segments.Count * 2];

        for (int i = 0; i < segments.Count; i++)
        {
            vertices[i * 2] = segments[i].start;
            vertices[i * 2 + 1] = segments[i].end;

            indices[i * 2] = i * 2;
            indices[i * 2 + 1] = i * 2 + 1;
        }

        treeMesh.vertices = vertices;
        treeMesh.SetIndices(indices, MeshTopology.Lines, 0);
        treeMesh.RecalculateBounds();
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
        cam.orthographicSize = Mathf.Max(0.5f, Mathf.Max(halfHeight, halfWidth) * 1.1f);
    }
    private void OnDestroy()
    {
        if (treeMesh != null)
        {
            Destroy(treeMesh);
        }
    }
}