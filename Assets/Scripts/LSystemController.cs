using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.IO;

// Holds the current L-system, handles hotkeys and the camera, and connects the other scripts:
// PresetLoader -> LSystemGenerator -> Turtle -> LeafPlacer -> MeshBuilder
public class LSystemController : MonoBehaviour
{
    [Header("L-System Settings")]
    [SerializeField] private string axiom = "F";
    [SerializeField, Range(0, 7)] private int iterations = 3;
    [SerializeField] private float angleLeft = 25.7f;
    [SerializeField] private float angleRight = 25.7f;
    [SerializeField] private float segmentLength = 0.1f;
    [SerializeField] private bool autoFitCamera = true;

    [Header("Leaves")]
    [SerializeField] private MeshFilter leafFilter;                       // the child "Leaves" object
    [SerializeField] private LeafSettings leafSettings = new LeafSettings();

    private Dictionary<char, string> rules = new Dictionary<char, string>
    {
        { 'F', "F[+F]F[-F]F" }
    };

    private string presetName = "Default";
    private string[] presetFiles = new string[0];
    private int currentPreset = -1;
    private int fullIterations = 5;      // the preset's own iterations = "fully grown"
    private int segmentCount;
    private int leafCount;
    private Mesh treeMesh;
    private Mesh leafMesh;

    // Keys 1-8 load preset files 1-8
    private static readonly Key[] presetKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
        Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8
    };

    private void Start()
    {
        treeMesh = MeshBuilder.CreateMesh("L-System Tree Mesh");
        GetComponent<MeshFilter>().sharedMesh = treeMesh;

        if (leafFilter != null)
        {
            leafMesh = MeshBuilder.CreateMesh("L-System Leaf Mesh");
            leafFilter.sharedMesh = leafMesh;
        }

        presetFiles = PresetLoader.FindPresetFiles();

        if (presetFiles.Length > 0)
            LoadPreset(0);
        else
            Regenerate();
    }

    private void LoadPreset(int index)
    {
        if (index < 0 || index >= presetFiles.Length)
        {
            Debug.LogWarning($"No preset file for key {index + 1}.");
            return;
        }

        try
        {
            LSystemPreset preset = PresetLoader.Load(presetFiles[index]);

            presetName = preset.name;
            axiom = preset.axiom;
            iterations = preset.iterations;
            fullIterations = Mathf.Max(1, preset.iterations);
            angleLeft = angleRight = preset.angle;
            segmentLength = preset.length;
            leafSettings.show = preset.leaves;
            rules = preset.rules;

            currentPreset = index;
            Regenerate();
        }
        catch (System.Exception e)
        {
            // A broken file keeps the current tree instead of crashing
            Debug.LogError($"Could not load {Path.GetFileName(presetFiles[index])}: {e.Message}");
        }
    }

    private void Regenerate()
    {
        // 1. Rewrite the rules recursively and feed each symbol to the turtle
        LSystemGenerator generator = new LSystemGenerator(rules);
        Turtle turtle = new Turtle(angleLeft, angleRight, segmentLength);
        generator.Generate(axiom, iterations, turtle.Process);
        turtle.Finish();

        // 2. Choose leaves: younger trees (fewer iterations than the preset) get fewer
        float maturity = Mathf.Clamp01(iterations / (float)fullIterations);
        List<Leaf> leaves = LeafPlacer.Place(turtle.leafCandidates, turtle.MinY, turtle.MaxY,
                                             turtle.segments.Count, maturity, leafSettings,
                                             out float leafSpacing);

        // 3. Build the meshes. Leaf size follows the spacing between leaves, so every model looks similarly filled
        MeshBuilder.BuildLines(treeMesh, turtle.segments);
        if (leafMesh != null)
            MeshBuilder.BuildLeaves(leafMesh, leaves, leafSpacing * leafSettings.scale);

        segmentCount = turtle.segments.Count;
        leafCount = leaves.Count;

        if (autoFitCamera)
            FitCamera();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
            return;

        // Preset files: keys 1-8, R reloads the current file
        for (int i = 0; i < presetKeys.Length; i++)
        {
            if (kb[presetKeys[i]].wasPressedThisFrame)
                LoadPreset(i);
        }

        if (kb.rKey.wasPressedThisFrame && currentPreset >= 0)
            LoadPreset(currentPreset);

        bool changed = false;
        float step = kb.shiftKey.isPressed ? 5f : 1f;   // hold Shift for bigger steps

        // Iterations: Up / Down
        if (kb.upArrowKey.wasPressedThisFrame)   { iterations = Mathf.Min(iterations + 1, 7); changed = true; }
        if (kb.downArrowKey.wasPressedThisFrame) { iterations = Mathf.Max(iterations - 1, 0); changed = true; }

        // Both angles: Left / Right arrows
        if (kb.rightArrowKey.wasPressedThisFrame) { angleLeft += step; angleRight += step; changed = true; }
        if (kb.leftArrowKey.wasPressedThisFrame)  { angleLeft -= step; angleRight -= step; changed = true; }

        // Left angle only: A / D. Right angle only: J / L
        if (kb.dKey.wasPressedThisFrame) { angleLeft += step; changed = true; }
        if (kb.aKey.wasPressedThisFrame) { angleLeft -= step; changed = true; }
        if (kb.lKey.wasPressedThisFrame) { angleRight += step; changed = true; }
        if (kb.jKey.wasPressedThisFrame) { angleRight -= step; changed = true; }

        // Segment length: W / S (visible when the camera is locked)
        if (kb.wKey.wasPressedThisFrame) { segmentLength += 0.01f; changed = true; }
        if (kb.sKey.wasPressedThisFrame) { segmentLength = Mathf.Max(0.01f, segmentLength - 0.01f); changed = true; }

        // Leaves
        LeafSettings ls = leafSettings;
        if (kb.vKey.wasPressedThisFrame) { ls.show = !ls.show; changed = true; }                                         // on / off
        if (kb.eKey.wasPressedThisFrame) { ls.scale = Mathf.Min(ls.scale * 1.2f, 5f); changed = true; }                  // bigger
        if (kb.qKey.wasPressedThisFrame) { ls.scale = Mathf.Max(ls.scale / 1.2f, 0.2f); changed = true; }                // smaller
        if (kb.xKey.wasPressedThisFrame) { ls.leavesPerSegment = Mathf.Min(ls.leavesPerSegment * 1.25f, 2f); changed = true; }   // denser
        if (kb.zKey.wasPressedThisFrame) { ls.leavesPerSegment = Mathf.Max(ls.leavesPerSegment / 1.25f, 0.02f); changed = true; } // sparser
        if (kb.tKey.wasPressedThisFrame) { ls.crownStart = Mathf.Min(ls.crownStart + 0.05f, 0.9f); changed = true; }     // crown higher
        if (kb.gKey.wasPressedThisFrame) { ls.crownStart = Mathf.Max(ls.crownStart - 0.05f, 0f); changed = true; }       // crown lower
        if (kb.nKey.wasPressedThisFrame) { ls.seed = Random.Range(0, 100000); changed = true; }                          // new arrangement

        // Camera auto-fit on / off: C
        if (kb.cKey.wasPressedThisFrame) { autoFitCamera = !autoFitCamera; changed = true; }

        angleLeft = Mathf.Clamp(angleLeft, 0f, 180f);
        angleRight = Mathf.Clamp(angleRight, 0f, 180f);

        if (changed)
            Regenerate();
    }

    // Centres an orthographic camera on the tree and zooms to fit it
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

    // On-screen info and hotkey help (useful for the video)
    private void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Label(new Rect(10, 10, 1100, 200),
            $"{presetName}   n = {iterations}/{fullIterations}   left = {angleLeft:F1}°   right = {angleRight:F1}°   " +
            $"length = {segmentLength:F2}   segments = {segmentCount:N0}   leaves = {leafCount:N0}   " +
            $"density = {leafSettings.leavesPerSegment:F2}   leaf size x{leafSettings.scale:F2}   " +
            $"crown from {leafSettings.crownStart * 100f:F0}%   seed = {leafSettings.seed}   " +
            $"camera = {(autoFitCamera ? "auto" : "locked")}\n" +
            "1-8 presets   R reload   Up/Down iterations   Left/Right angle (Shift x5)   A/D left angle   J/L right angle   W/S length\n" +
            "V leaves   Q/E leaf size   Z/X leaf density   G/T crown height   N new leaves   C camera lock");
    }

    private void OnDestroy()
    {
        if (treeMesh != null)
            Destroy(treeMesh);
        if (leafMesh != null)
            Destroy(leafMesh);
    }
}