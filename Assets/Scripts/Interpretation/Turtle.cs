using UnityEngine;
using System.Collections.Generic;

public struct TurtleState
{
    public Vector2 position;
    public float heading;

    public TurtleState(Vector2 position, float heading)
    {
        this.position = position;
        this.heading = heading;
    }
}

// A place where a leaf could grow. The LeafPlacer decides which ones actually get one.
public struct LeafCandidate
{
    public Vector3 position;
    public float heading;
    public bool isTip;   // true = branch tip or bud, false = along a branch

    public LeafCandidate(Vector3 position, float heading, bool isTip)
    {
        this.position = position;
        this.heading = heading;
        this.isTip = isTip;
    }
}

// Interprets L-system symbols as turtle commands and records the geometry.
public class Turtle
{
    private TurtleState state;
    private Stack<TurtleState> savedStates;
    private Stack<bool> parentDrew;   // "has drawn" flag of each open branch

    private float angleLeft;
    private float angleRight;
    private float length;
    private bool drewInBranch;        // has the current branch drawn a segment yet?

    public List<(Vector3 start, Vector3 end)> segments;
    public List<LeafCandidate> leafCandidates;

    // Vertical extent of the tree, used to find the crown
    public float MinY { get; private set; }
    public float MaxY { get; private set; }

    public Turtle(float angleLeft, float angleRight, float length)
    {
        this.angleLeft = angleLeft;
        this.angleRight = angleRight;
        this.length = length;

        state = new TurtleState(Vector2.zero, 90f);
        savedStates = new Stack<TurtleState>();
        parentDrew = new Stack<bool>();
        segments = new List<(Vector3, Vector3)>();
        leafCandidates = new List<LeafCandidate>();
    }

    public void Process(char symbol)
    {
        switch (symbol)
        {
            case 'F':
                MoveForward();
                drewInBranch = true;

                // Every segment inside a branch is a possible side-leaf position
                if (savedStates.Count >= 1)
                {
                    var last = segments[segments.Count - 1];
                    Vector3 middle = (last.start + last.end) * 0.5f;
                    leafCandidates.Add(new LeafCandidate(middle, state.heading, false));
                }
                break;

            case '+':
                state.heading += angleLeft;
                break;

            case '-':
                state.heading -= angleRight;
                break;

            case '[':
                savedStates.Push(state);
                parentDrew.Push(drewInBranch);
                drewInBranch = false;
                break;

            case ']':
                // End of a branch: if it drew something, its end is a tip
                if (drewInBranch)
                    AddTip();

                if (savedStates.Count > 0)
                {
                    state = savedStates.Pop();
                    drewInBranch = parentDrew.Pop();
                }
                else
                {
                    Debug.LogWarning("Unbalanced ']' in L-system string.");
                }
                break;

            default:
                // Other letters (e.g. X) mark buds in node-rewriting systems
                if (char.IsLetter(symbol))
                    AddTip();
                break;
        }
    }

    // Call once after generation: the end of the main stem is also a tip
    public void Finish()
    {
        if (drewInBranch)
            AddTip();
    }

    private void AddTip()
    {
        leafCandidates.Add(new LeafCandidate(ToVector3(state.position), state.heading, true));
    }

    private static Vector3 ToVector3(Vector2 v)
    {
        return new Vector3(v.x, v.y, 0f);
    }

    private void MoveForward()
    {
        Vector2 start = state.position;

        float radians = state.heading * Mathf.Deg2Rad;

        float x = start.x + length * Mathf.Cos(radians);
        float y = start.y + length * Mathf.Sin(radians);

        Vector2 end = new Vector2(x, y);

        segments.Add((ToVector3(start), ToVector3(end)));

        state.position = end;
        MinY = Mathf.Min(MinY, y);
        MaxY = Mathf.Max(MaxY, y);
    }
}