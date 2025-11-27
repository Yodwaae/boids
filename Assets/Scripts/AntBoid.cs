using UnityEngine;

using System.Collections.Generic;

public class AntBoid : MonoBehaviour
{
    // TODO Set the functions back to private, it's just that they are harder to see when they are private and not yet called


    [Header("TO NAME")]
    // NOTE / No need to enforce min < max in editor as Unity will swap them anyway in Random.Range() if need be
    public float minVelocity;
    public float maxVelocity;
    public float closerFactor;
    public float furtherFactor;
    public float withFactor;

    
    private Vector2 velocity;
    private static List<AntBoid> boids = new List<AntBoid>();


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
    public float Distance(AntBoid other) { return Vector2.Distance(velocity, other.velocity); }

    public void MoveCloser(AntBoid other) {

        if (boids.Count < 1)
            return;

        Vector2 average = Vector2.zero;

        // Get the average position of the other boids
        foreach (AntBoid boid in boids){

            // Skip itself
            if (other == boid) 
                continue;

            average.x = this.transform.position.x;
            average.y = this.transform.position.y;

        }

        average /= (boids.Count - 1);

        velocity = average/ closerFactor;
    }
    public void MoveFurther(AntBoid other) { }
    public void MoveWith(AntBoid other) { }


}
