using System.Collections.Generic;
using UnityEngine;

public class LocustManager : MonoBehaviour
{
    [Header("Boids values")]
    public int initialLocustCount = 10;
    public int maxLocustCount = 20;
    public GameObject boidPrefab;

    // Singleton
    public static LocustManager instance;
    public static List<LocustBoid> boids = new List<LocustBoid>();

    private void Awake()
    {
        // Ensure Locust Manager singleton
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Spawn Locusts
        for (int i = 0; i < initialLocustCount; i++) {

            // Get a random pos in the cam space
            Camera cam = Camera.main;
            Vector3 screenPos = new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), cam.nearClipPlane);
            Vector3 spawnPos = cam.ScreenToWorldPoint(screenPos);
            spawnPos.z = 0f;

            // Get a random Rotation (fixed on the X axis so it's always facing the cam)
            Vector3 rotationVector = new Vector3(0f, 0f, Random.Range(0f, 360f));
            Quaternion spawnRot = Quaternion.Euler(rotationVector);

            // Instatiate a locust with this pos and rot
            InstantiateLocust(spawnPos, spawnRot);
        }
    }

    public void InstantiateLocust(Vector3 spawnPos, Quaternion spawnRot) {

        // If adding a boid would exceed the max number of boids makes an early exit
        if (boids.Count + 1 > maxLocustCount)
            return;
        
        // Instatiate the boid at the given pos with the given rot
        Instantiate(boidPrefab, spawnPos, spawnRot); 
    }

}
