using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheatManager : MonoBehaviour
{
    [Header("Wheat values")]
    public int initialWheatCount = 10;
    public GameObject wheatPrefab;
    public float wheatTimer = 3f;
    private float lastSpawnDatetime;

    private void Start()
    {
        // Spawn Wheat
        for (int i = 0; i < initialWheatCount; i++)
            InstantiateWheat();

        // Set timer
        lastSpawnDatetime = Time.time;
    }

    private void Update()
    {
        // Spawn Wheat if timer elapsed
        if (Time.time > lastSpawnDatetime + wheatTimer) {
            InstantiateWheat();
            lastSpawnDatetime = Time.time;
        }
    }

    public void InstantiateWheat() {

        // Get a random pos in the cam space
        Camera cam = Camera.main;
        Vector3 screenPos = new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), cam.nearClipPlane);
        Vector3 spawnPos = cam.ScreenToWorldPoint(screenPos);
        spawnPos.z = 0f;

        // Instatiate the x=wheat at the given pos
        Instantiate(wheatPrefab, spawnPos, Quaternion.identity);
    }

}
