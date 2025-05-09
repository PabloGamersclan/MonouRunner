
using System;
using UnityEngine;

public class ObstaclePool : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform obstacleParent;
    [SerializeField] private int poolSize = 5;
    [SerializeField] private float spawnTime = 2.5f;
    [SerializeField] private float minYPosition = -2;
    [SerializeField] private float maxYPosition = 3;
    [SerializeField] private float xSpawnPosition = 12;

    private float timeElapsed;
    private int obstacleCount;
    private GameObject[] obstacles;
    

    void Start()
    {
        obstacles = new GameObject[poolSize];

        for(int i = 0; i < poolSize; i++)
        {
            obstacles[i] = Instantiate(obstaclePrefab, obstacleParent);
            obstacles[i].SetActive(false);
        }
    }


    void Update()
    {
        timeElapsed += Time.deltaTime;
        if(timeElapsed > spawnTime && GameManager.Intance.IsPlaying)
        {
            SpawnObstacle();
        }
    }

    private void SpawnObstacle()
    {
        float ySpawnPostion = UnityEngine.Random.Range(minYPosition, maxYPosition);
        Vector2 spawnPosition = new Vector2(xSpawnPosition, ySpawnPostion);
        obstacles[obstacleCount].transform.position = spawnPosition;
        timeElapsed = 0f;

        if (!obstacles[obstacleCount].activeSelf)
        {
            obstacles[obstacleCount].SetActive(true);
        }
 
        obstacleCount++;

        if(obstacleCount == poolSize)
        {
            obstacleCount = 0;
        }
    }
}
