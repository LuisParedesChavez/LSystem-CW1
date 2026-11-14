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

        // Temporary drawing, visible in the Scene view only
        foreach (var segment in turtle.segments)
        {
            Debug.DrawLine(segment.start, segment.end, Color.white, 100f);
        }

        Debug.Log($"Segments drawn: {turtle.segments.Count}");
    }
}