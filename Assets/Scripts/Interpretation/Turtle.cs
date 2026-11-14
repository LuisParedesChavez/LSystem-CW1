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

public class Turtle
{
    private TurtleState state;
    private Stack<TurtleState> savedStates;

    private float angle;
    private float length;

    public List<(Vector3 start, Vector3 end)> segments;

    public Turtle(float angle, float length)
    {
        this.angle = angle;
        this.length = length;

        state = new TurtleState(Vector2.zero, 90f);
        savedStates = new Stack<TurtleState>();

        segments = new List<(Vector3, Vector3)>();
    }

    public void Process(char symbol)
    {
        switch (symbol)
        {
            case 'F':
                MoveForward();
                break;

            case '+':
                state.heading += angle;
                break;

            case '-':
                state.heading -= angle;
                break;

            case '[':
                savedStates.Push(state);
                break;

            case ']':
                if (savedStates.Count > 0)
                    state = savedStates.Pop();
                break;
        }
    }

    private void MoveForward()
    {
        Vector2 start = state.position;

        float radians = state.heading * Mathf.Deg2Rad;

        float x = start.x + length * Mathf.Cos(radians);
        float y = start.y + length * Mathf.Sin(radians);

        Vector2 end = new Vector2(x, y);

        segments.Add((
            new Vector3(start.x, start.y, 0),
            new Vector3(end.x, end.y, 0)
        ));

        state.position = end;
    }
}