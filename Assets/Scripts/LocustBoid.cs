using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LocustBoid : MonoBehaviour
{
    // TODO It would be more optimised to check the distance before, build a list with it, then pass that to the three functions as arg

    // NOTE : No need to enforce min < max in editor as Unity will swap them anyway in Random.Range() if need be
    [Header("Boid Settings")]
    public float minVelocity = 1f;
    public float maxVelocity = 5f;

    public float closerFactor = 100f;   // Cohesion
    public float withFactor = 40f;      // Alignment
    public float furtherFactor = 5f;    // Separation

    public float minDistance = 20f;     
    public float neighborRadius = 200f; 

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
        // Compute the boid velocity
        MoveCloser();
        MoveWith();
        MoveFurther();

        // Clamp the velocity if greater than maxVelocity
        if (velocity.magnitude > maxVelocity)
            velocity = velocity.normalized * maxVelocity;

        // Move the boid
        transform.position += (new Vector3(velocity.x, velocity.y, 0f) * Time.deltaTime);
    }

    // ===== HELPERS FUNCTIONS =====

    public float Distance(LocustBoid other) { return Vector2.Distance(transform.position, other.transform.position); }

    // ===== BOIDS THREE RULES =====
    // TODO Refacto this, proly adding a helper func to reduce code duplication (code dup is kinda crazy rn)

    private void MoveCloser() {

        // Initialisation
        Vector2 sum = Vector2.zero;
        int count = 0;

        // Get the sum of the position of the other boids
        foreach (LocustBoid boid in boids) {

            // Skip itself
            if (boid == this) 
                continue;

            // Skip boid that are too far
            if (Distance(boid) > neighborRadius)
                continue;

            // Add the pos to the vector and increment the count
            sum.x += this.transform.position.x - boid.transform.position.x;
            sum.y += this.transform.position.y - boid.transform.position.y;
            count++;
        }
        
        // If no other boids close by, early exit
        if (count == 0)
            return;

        // Compute the average and set our velocity toward others boids
        Vector2 average = sum / count;
        velocity -= (average / closerFactor);
    }


    private void MoveWith() {

        // Initialisation
        Vector2 sum = Vector2.zero;
        int count = 0;


        // Get the sum of the position of the other boids
        foreach (LocustBoid boid in boids) {

            // Skip itself
            if (boid == this)
                continue;

            // Skip boid that are too far
            if (Distance(boid) > neighborRadius)
                continue;

            // Add the velocity to the vector and increment the count
            sum += boid.velocity;
            count++;
        }

        // If no other boids close by, early exit
        if (count == 0)
            return;

        // Compute the average and set the velocity
        Vector2 average = sum / count;
        velocity += (average  / withFactor);
    }

    private void MoveFurther()
    {

        // Initialisation
        Vector2 repulsion = Vector2.zero;
        int count = 0;

        foreach (LocustBoid boid in boids)
        {

            // Skip itself
            if (boid == this)
                continue;
             
            float distance = Distance(boid);

            //
            if (distance < minDistance)
            {

                float xDiff = (transform.position.x - boid.transform.position.x);
                float yDiff = (transform.position.y - boid.transform.position.y);

                Vector2 diff = new Vector2(xDiff, yDiff);

                if (diff != Vector2.zero)
                    repulsion += diff.normalized / distance;

                count++;
            }
        }

        // If no other boids close by, early exit
        if (count == 0)
            return;

        // Compute the average and set the velocity
        repulsion /= count;

        // FIX: must be += (because diff is already pointing away)
        velocity += (repulsion / furtherFactor);
    }


}


