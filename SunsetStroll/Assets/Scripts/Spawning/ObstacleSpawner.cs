using UnityEngine;

namespace SunsetStroll.Spawning
{
	public class ObstacleSpawner : MonoBehaviour
	{
		[SerializeField] private Transform spawnPoint;
		[SerializeField] private GameObject[] obstaclePrefabs;
		[SerializeField] private Vector2 yRange = new Vector2(-2f, -1.25f);
		[SerializeField] private float spawnVariance = 0.2f;

		private float timer;

		private void Update()
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning) return;
			timer -= Time.deltaTime;
			if (timer <= 0f)
			{
				Spawn();
				float interval = SunsetStroll.GameManager.Instance.CurrentSpawnInterval;
				float variance = Random.Range(-spawnVariance, spawnVariance);
				timer = Mathf.Max(0.1f, interval + variance);
			}
		}

		private void Spawn()
		{
			if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;
			GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
			float y = Random.Range(yRange.x, yRange.y);
			Vector3 pos = (spawnPoint ? spawnPoint.position : transform.position);
			pos.y = y;
			Instantiate(prefab, pos, Quaternion.identity);
		}
	}
}