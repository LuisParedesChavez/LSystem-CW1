using UnityEngine;
using System.Collections.Generic;

// One leaf to draw
public struct Leaf
{
    public Vector3 position;
    public float heading;
    public float scale;

    public Leaf(Vector3 position, float heading, float scale)
    {
        this.position = position;
        this.heading = heading;
        this.scale = scale;
    }
}

// Leaf settings, shown in the Inspector as one group
[System.Serializable]
public class LeafSettings
{
    public bool show = true;
    public float scale = 1.2f;                                  // leaf length relative to the average spacing between leaves
    [Range(0.02f, 2f)] public float leavesPerSegment = 0.3f;    // target density, same for every model
    [Range(0f, 0.9f)] public float crownStart = 0.35f;          // no leaves below this fraction of the height
    public int seed = 1;                                        // same seed = same leaves
}

// Chooses which leaf candidates actually get a leaf, and how big leaves should be.
public static class LeafPlacer
{
    private const float CrownFade = 0.15f;   // height fraction over which leaves fade in
    private const float SideWeight = 0.5f;   // side positions are half as likely as tips

    public static List<Leaf> Place(List<LeafCandidate> candidates, float minY, float maxY,
                                   int segmentCount, float maturity, LeafSettings settings,
                                   out float spacing)
    {
        List<Leaf> leaves = new List<Leaf>();
        spacing = 0f;

        if (!settings.show || candidates.Count == 0)
            return leaves;

        float height = Mathf.Max(maxY - minY, 0.0001f);

        // 1. Weight each candidate: 0 below the crown, fading in to 1 above it
        float[] weights = new float[candidates.Count];
        float totalWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            float t = (candidates[i].position.y - minY) / height;   // 0 = base, 1 = top
            float fade = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(settings.crownStart, settings.crownStart + CrownFade, t));

            weights[i] = fade * (candidates[i].isTip ? 1f : SideWeight);
            totalWeight += weights[i];
        }

        if (totalWeight <= 0f)
            return leaves;

        // 2. Target number of leaves depends on tree size and age, not on the number of tips
        float target = settings.leavesPerSegment * segmentCount * maturity * maturity;
        float chanceScale = target / totalWeight;

        // 3. Pick leaves with seeded randomness: random tilt and size for a natural look
        System.Random rng = new System.Random(settings.seed);

        for (int i = 0; i < candidates.Count; i++)
        {
            if (weights[i] <= 0f)
                continue;

            if (rng.NextDouble() >= chanceScale * weights[i])
                continue;

            LeafCandidate c = candidates[i];

            if (c.isTip)
            {
                float tilt = Range(rng, -25f, 25f);
                leaves.Add(new Leaf(c.position, c.heading + tilt, Range(rng, 0.7f, 1.3f)));
            }
            else
            {
                float side = rng.NextDouble() < 0.5 ? 1f : -1f;
                float angle = side * Range(rng, 35f, 65f);
                leaves.Add(new Leaf(c.position, c.heading + angle, Range(rng, 0.5f, 0.9f)));
            }
        }

        // 4. Average spacing between leaves: sqrt(crown area / number of leaves).
        //    Leaf size follows it, so sparse crowns get bigger leaves and dense crowns smaller ones.
        if (leaves.Count > 0)
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            float minLeafY = float.MaxValue, maxLeafY = float.MinValue;

            foreach (Leaf leaf in leaves)
            {
                minX = Mathf.Min(minX, leaf.position.x);
                maxX = Mathf.Max(maxX, leaf.position.x);
                minLeafY = Mathf.Min(minLeafY, leaf.position.y);
                maxLeafY = Mathf.Max(maxLeafY, leaf.position.y);
            }

            float crownArea = Mathf.Max((maxX - minX) * (maxLeafY - minLeafY), 0.0001f);
            spacing = Mathf.Sqrt(crownArea / leaves.Count);
        }

        return leaves;
    }

    private static float Range(System.Random rng, float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }
}