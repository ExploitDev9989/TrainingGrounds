using UnityEngine;

public class TargetMover : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 moveAxis = Vector3.right;   // direction the target moves (right = left/right movement)
    public float amplitude = 2f;               // how far the target moves from its starting position
    public float speed = 1.5f;                 // how fast the target moves back and forth

    private Vector3 startPos;                  // stores the starting position of the target

    void Start()
    {
        // save the starting position so the target moves around this point
        startPos = transform.position;

        // normalize the axis so the direction is consistent
        // (prevents movement speed from changing if axis length is larger than 1)
        moveAxis = moveAxis.normalized;
    }

    void Update()
    {
        // calculate movement offset using a sine wave
        // Mathf.Sin creates smooth back-and-forth motion over time
        float offset = Mathf.Sin(Time.time * speed) * amplitude;

        // move the target along the chosen axis relative to its starting position
        transform.position = startPos + moveAxis * offset;
    }
}