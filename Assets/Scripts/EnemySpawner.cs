using FishNet.Object;
using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Wave
{
	public GameObject[] enemies;
	public float spawnInterval;
	public int minimumAmount;
}

public class EnemySpawner : NetworkBehaviour
{
	[Header("Spawn Management")]
    [SerializeField] private int waveDuration = 60;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Wave[] waves = new Wave[30];		
    private int currentWaveNumber = -1;

    private Wave currentWave;
	private int enemyAmount;
	private TextMeshProUGUI enemyKilledText;

	private float timeToSpawn;
	private float timeToNextWave;
	private int enemyKilled;

	void Start()
	{
		enemyKilledText = GameObject.Find("GameHUD/EnemyKilled/Amount").GetComponent<TextMeshProUGUI>();
    }

	void Update()
	{
		if (!IsServer) return;

        if (GameManager.Instance.GameStarted)
        {
			if (timeToNextWave > 0)
				timeToNextWave -= Time.deltaTime;
			else
				StartWave();

            if (timeToSpawn > 0)
                timeToSpawn -= Time.deltaTime;
            else if (enemyAmount < currentWave.minimumAmount)
			{
				SpawnEnemy();
            }
		}
	}

	public void StartWave()
	{
		if (currentWaveNumber == 29)
			currentWaveNumber = 0;
		else
			currentWaveNumber++;
		currentWave = waves[currentWaveNumber];
        timeToNextWave = waveDuration;
        SpawnEnemy();
	}

	void SpawnEnemy()
    {
        timeToSpawn = currentWave.spawnInterval;
        foreach (GameObject enemyPrefab in currentWave.enemies)
		{
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            Enemy enemy = enemyGO.GetComponent<Enemy>();
            Spawn(enemyGO);
        }
	}

	public void ZombieKilled()
    {
		enemyKilled++;
		enemyKilledText.text = enemyKilled.ToString();
    }
}
