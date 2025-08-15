using UnityEngine;

namespace SunsetStroll.Spawning
{
	public class CoinSpawner : MonoBehaviour
	{
		[SerializeField] private Transform spawnPoint;
		[SerializeField] private GameObject coinPrefab;
		[SerializeField] private Vector2 yRange = new Vector2(-1.7f, 0.8f);
		[SerializeField] private float spawnInterval = 2.5f;
		[SerializeField] private Vector2 xJitterRange = new Vector2(0f, 1.5f);

		private float timer;

		private void Update()
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning || SunsetStroll.GameManager.Instance.IsPaused) return;
			timer -= Time.deltaTime;
			if (timer <= 0f)
			{
				Spawn();
				timer = spawnInterval;
			}
		}

		private void Spawn()
		{
			if (coinPrefab == null) return;
			float y = Random.Range(yRange.x, yRange.y);
			Vector3 pos = (spawnPoint ? spawnPoint.position : transform.position);
			pos.y = y;
			pos.x += Random.Range(xJitterRange.x, xJitterRange.y);
			Instantiate(coinPrefab, pos, Quaternion.identity);
		}
	}
}