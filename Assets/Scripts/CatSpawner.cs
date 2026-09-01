using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatSpawner : MonoBehaviour
{
    [Header("Cat Prefabs")]
    public GameObject[] catPrefabs;

    [Header("Player")]
    public Transform player;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Spawn Distance")]
    public float minimumDistance = 2f;
    public float maximumDistance = 8f;

    [Header("Cat Settings")]
    public float catActiveTime = 10f;
    public float spawnDelay = 1f;

    private GameObject currentCat;
    private Coroutine spawnCoroutine;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Don't spawn automatically at game start.
        // PhotoCapture will activate this GameObject
        // when the camera round starts.
    }


    // =========================================================
    // WHEN SPAWNER IS ACTIVATED
    // =========================================================

    private void OnEnable()
    {
        // Start spawning when GameObject becomes active
        spawnCoroutine = StartCoroutine(StartSpawning());
    }


    private IEnumerator StartSpawning()
    {
        yield return new WaitForSeconds(0.5f);

        SpawnNextCat();
    }


    // =========================================================
    // SPAWN CAT
    // =========================================================

    private void SpawnNextCat()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (currentCat != null)
            return;


        if (player == null)
        {
            Debug.LogError(
                "CatSpawner: Player is not assigned!"
            );
            return;
        }


        if (catPrefabs == null ||
            catPrefabs.Length == 0)
        {
            Debug.LogError(
                "CatSpawner: No cat prefabs assigned!"
            );
            return;
        }


        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogError(
                "CatSpawner: No spawn points assigned!"
            );
            return;
        }


        Transform spawnPoint =
            GetValidSpawnPoint();


        if (spawnPoint == null)
        {
            Debug.Log(
                "🐱 No suitable spawn point found. Trying again..."
            );

            Invoke(
                nameof(SpawnNextCat),
                spawnDelay
            );

            return;
        }


        // Pick random cat
        int randomCat =
            Random.Range(
                0,
                catPrefabs.Length
            );


        currentCat = Instantiate(
            catPrefabs[randomCat],
            spawnPoint.position,
            spawnPoint.rotation
        );


        Debug.Log(
            "🐱 Cat spawned: " +
            currentCat.name
        );


        StartCoroutine(
            DisableCatAfterTime()
        );
    }


    // =========================================================
    // FIND VALID SPAWN POINT
    // =========================================================

    private Transform GetValidSpawnPoint()
    {
        List<Transform> validPoints =
            new List<Transform>();


        foreach (Transform point in spawnPoints)
        {
            if (point == null)
                continue;


            float distance =
                Vector3.Distance(
                    player.position,
                    point.position
                );


            if (distance >= minimumDistance &&
                distance <= maximumDistance)
            {
                validPoints.Add(point);
            }
        }


        if (validPoints.Count == 0)
            return null;


        int randomIndex =
            Random.Range(
                0,
                validPoints.Count
            );


        return validPoints[randomIndex];
    }


    // =========================================================
    // CAT LIFETIME
    // =========================================================

    private IEnumerator DisableCatAfterTime()
    {
        yield return new WaitForSeconds(
            catActiveTime
        );


        DisableCurrentCat();


        yield return new WaitForSeconds(
            spawnDelay
        );


        SpawnNextCat();
    }


    // =========================================================
    // DISABLE CURRENT CAT
    // =========================================================

    public void DisableCurrentCat()
    {
        if (currentCat != null)
        {
            Destroy(currentCat);

            currentCat = null;

            Debug.Log(
                "🐱 Cat removed."
            );
        }
    }


    // =========================================================
    // STOP SPAWNING
    // =========================================================

    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        CancelInvoke();

        if (currentCat != null)
        {
            Destroy(currentCat);

            currentCat = null;
        }
    }
}