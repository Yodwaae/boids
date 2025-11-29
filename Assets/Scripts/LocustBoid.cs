using UnityEngine;

using System.Collections.Generic;

public class LocustBoid : MonoBehaviour
{
    // TODO Set the functions back to private, it's just that they are harder to see when they are private and not yet called


    [Header("TO NAME")]
    // NOTE / No need to enforce min < max in editor as Unity will swap them anyway in Random.Range() if need be
    public float minVelocity;
    public float maxVelocity;
    public float closerFactor = 100f;
    public float furtherFactor;
    public float withFactor;

    
    private Vector2 velocity;
    private static List<LocustBoid> boids = new List<LocustBoid>();


    private void Start()
    {
        // Random Velocity Initialisation
        velocity.x = Random.Range(minVelocity, maxVelocity) / maxVelocity;
        velocity.y = Random.Range(minVelocity, maxVelocity) / maxVelocity;

        // Add itself to the list of boids
        boids.Add(this);
    }

    private void Update()
    {
        
    }

    // ===== HELPERS FUNCTIONS =====

    public float Distance(LocustBoid other) { return Vector2.Distance(velocity, other.velocity); }

    // ===== BOIDS THREE RULES =====

    public void MoveCloser(LocustBoid other) {

        // Initialisation
        Vector2 average = Vector2.zero;

        // Early return if not enough boids
        if (boids.Count < 1)
            return;


        // Get the sum of the position of the other boids
        foreach (LocustBoid boid in boids){

            // Skip itself
            if (other == boid) 
                continue;

            average.x = this.transform.position.x;
            average.y = this.transform.position.y;

        }

        // Compute the average and the velocity
        average /= (boids.Count - 1);
        velocity = average/ closerFactor;
    }

    public void MoveFurther(LocustBoid other) { }

    public void MoveWith(LocustBoid other) { }


}
