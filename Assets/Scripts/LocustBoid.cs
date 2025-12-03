using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum LocustStates
{
    None,
    Young,
    Mature,
    Old,
}


public class LocustBoid : MonoBehaviour
{
    // TODO It would be more optimised to check the distance before, build a list with it, then pass that to the three functions as arg

    // NOTE : No need to enforce min < max in editor as Unity will swap them anyway in Random.Range() if need be
    [Header("Boid Settings")]
    public float minVelocity = 1f;
    private float maxVelocity = 5f;

    public float closerFactor = 50f;   // Cohesion
    public float withFactor = 20f;      // Alignment
    public float furtherFactor = 1.5f;    // Separation

    public float minDistance = .8f;     
    public float neighborRadius = 2.5f;

    public float avoidDistance = 1.5f;
    public float avoidFactor = 500f;
    public float[] avoidRaycastAngles = { 0f, -15f, 15f };

    private Vector2 velocity;


    [Header("State Machine Settings")]
    public float youngTimer = 10f;
    public float youngMaxVelocity = 8f;
    public Material youngMaterial;

    public float matureTimer = 30f;
    public float matureMaxVelocity = 5f;
    public Material matureMaterial;


    public float oldTimer = 10f;
    public float oldMaxVelocity = 3f;
    public Material oldMaterial;

    private float timeStamp; // TODO Change name
    private float nextStateTimer;
    private LocustStates state;


    private void Start()
    {
        // Random Velocity Initialisation
        velocity.x = Random.Range(minVelocity, maxVelocity);
        velocity.y = Random.Range(minVelocity, maxVelocity);

        // Add itself to the list of boids
        LocustManager.boids.Add(this);

        // Init state
        ChangeState();
    }

    // TODO Useful helper function ? Shouldn't I just a reference to the main material in Start and manually do the change ?
    private void ChangeMainMaterial(Material newMaterial) { GetComponent<MeshRenderer>().material = newMaterial; }

    private void ChangeState() {

        // Register the time the change happened
        timeStamp = Time.time;

        switch (state) {

            // None -> Young (Initialisation)
            case LocustStates.None:
                state = LocustStates.Young;
                maxVelocity = youngMaxVelocity;
                nextStateTimer = youngTimer;
                ChangeMainMaterial(youngMaterial);
                break;

            // Young -> Mature
            case LocustStates.Young:
                state = LocustStates.Mature;
                maxVelocity = matureMaxVelocity;
                nextStateTimer = matureTimer;
                ChangeMainMaterial(matureMaterial);
                break;

            // Mature -> Old
            case LocustStates.Mature:
                state = LocustStates.Old;
                maxVelocity = oldMaxVelocity;
                nextStateTimer = oldTimer;
                ChangeMainMaterial(oldMaterial);
                break;

            // Old -> Death
            case LocustStates.Old:
                LocustManager.boids.Remove(this);
                Destroy(gameObject);
                break;
        }
    }


    private void Update()
    {
        // Compute the boid velocity
        MoveCloser();
        MoveWith();
        MoveFurther();
        AvoidWalls();

        //
        if (Time.time > timeStamp + nextStateTimer)
            ChangeState();

        // Clamp the velocity between min and max velocity
        if (velocity.magnitude > maxVelocity)
            velocity = velocity.normalized * maxVelocity;
        else if (velocity.magnitude < minVelocity)
            velocity = velocity.normalized * minVelocity;

            // Move the boid
            transform.position += (new Vector3(velocity.x, velocity.y, 0f) * Time.deltaTime);

        // Rotate the boids
        if (velocity.sqrMagnitude > 0.0001f)
            transform.right = velocity;

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
        foreach (LocustBoid boid in LocustManager.boids) {

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

        // Get the sum of the velocity of the other boids
        foreach (LocustBoid boid in LocustManager.boids) {

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

    private void MoveFurther(){

        // Initialisation
        Vector2 repulsion = Vector2.zero;
        int count = 0;

        foreach (LocustBoid boid in LocustManager.boids) {

            // Skip itself
            if (boid == this)
                continue;
             
            // Compute the distance to the other boid
            float distance = Distance(boid);

            // If the neighbor is too close
            if (distance < minDistance){

                // Compute the direction pointing away from the other
                float xDiff = (transform.position.x - boid.transform.position.x);
                float yDiff = (transform.position.y - boid.transform.position.y);
                Vector2 diff = new Vector2(xDiff, yDiff);

                // Normalise and weight the repulsion
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
        velocity += (repulsion / furtherFactor);
    }

    private void AvoidWalls() {

        // Get the direction the boid is moving towards
        Vector3 baseDirection = new Vector3(velocity.x, velocity.y, 0f).normalized;

        foreach(float angle in avoidRaycastAngles) {

            // Compute the direction based on the angle and based direction
            Vector3 direction = Quaternion.Euler(0 ,0, angle) * baseDirection;

            if (Physics.Raycast(transform.position, baseDirection, out RaycastHit hit, avoidDistance)) {

                // Push the boid away from the obstacle based on the normal
                Vector3 normal = hit.normal;
                velocity += new Vector2(normal.x, normal.y) * avoidFactor * Time.deltaTime;

                // We stop as soon as we got a hit
                break;
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject);
        EatCrop();
    }

    private void EatCrop() {

        // Spawn a new locust close to this one
        Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);
        LocustManager.instance.InstantiateLocust(transform.position + offset, transform.rotation);
    }

}


