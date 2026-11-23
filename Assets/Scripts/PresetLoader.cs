using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

// The contents of one preset file
public class LSystemPreset
{
    public string name = "Untitled";
    public string axiom = "F";
    public int iterations = 4;
    public float angle = 25f;
    public float length = 0.1f;
    public bool leaves = true;
    public Dictionary<char, string> rules = new Dictionary<char, string>();
}

// Finds and reads preset files from StreamingAssets/Presets.
// Format: "key = value", one per line; '#' starts a comment.
public static class PresetLoader
{
    public static string[] FindPresetFiles()
    {
        string folder = Path.Combine(Application.streamingAssetsPath, "Presets");

        if (!Directory.Exists(folder))
        {
            Debug.LogWarning($"Preset folder not found: {folder}");
            return new string[0];
        }

        string[] files = Directory.GetFiles(folder, "*.txt");
        System.Array.Sort(files);   // 01_..., 02_..., so keys 1-8 match the file order
        Debug.Log($"Found {files.Length} preset files.");
        return files;
    }

    // Throws an exception if the file is broken, so the caller can keep the current tree
    public static LSystemPreset Load(string path)
    {
        LSystemPreset preset = new LSystemPreset();

        foreach (string rawLine in File.ReadAllLines(path))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#"))
                continue;

            int equals = line.IndexOf('=');
            if (equals < 0)
                continue;

            string key = line.Substring(0, equals).Trim().ToLower();
            string value = line.Substring(equals + 1).Trim();

            switch (key)
            {
                case "name":
                    preset.name = value;
                    break;
                case "axiom":
                    preset.axiom = value.Replace(" ", "");
                    break;
                case "iterations":
                    preset.iterations = int.Parse(value);
                    break;
                case "angle":
                    preset.angle = float.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "length":
                    preset.length = float.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "leaves":
                    preset.leaves = value.ToLower() == "true";
                    break;
                case "rule":
                    // Format: "X -> F[+X]F[-X]+X"
                    int arrow = value.IndexOf("->");
                    char symbol = value.Substring(0, arrow).Trim()[0];
                    string successor = value.Substring(arrow + 2).Replace(" ", "");
                    preset.rules[symbol] = successor;
                    break;
            }
        }

        return preset;
    }
}