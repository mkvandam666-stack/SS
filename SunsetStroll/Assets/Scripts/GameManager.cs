using UnityEngine;

namespace SunsetStroll
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance { get; private set; }

		[Header("Game Speed")] 
		[SerializeField] private float startingSpeed = 7f;
		[SerializeField] private float maxSpeed = 20f;
		[SerializeField] private float speedIncreasePerSecond = 0.25f;

		[Header("Spawning")] 
		[SerializeField] private float startingSpawnInterval = 1.25f;
		[SerializeField] private float minSpawnInterval = 0.35f;
		[SerializeField] private float spawnIntervalDecreasePerSecond = 0.03f;

		[Header("Scoring")] 
		[SerializeField] private float scorePerSecondAtBaseSpeed = 10f;

		public float CurrentSpeed { get; private set; }
		public float CurrentSpawnInterval { get; private set; }
		public float Score { get; private set; }
		public bool IsRunning { get; private set; }
		public bool IsPaused { get; private set; }

		public System.Action OnGameOver;
		public System.Action<float> OnScoreChanged;
		public System.Action<bool> OnPauseChanged;

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			ResetGame();
		}

		private void Update()
		{
			if (!IsRunning || IsPaused) return;

			// Speed ramp
			CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + speedIncreasePerSecond * Time.deltaTime);

			// Spawn interval ramp (decrease over time)
			CurrentSpawnInterval = Mathf.Max(minSpawnInterval, CurrentSpawnInterval - spawnIntervalDecreasePerSecond * Time.deltaTime);

			// Score increases with speed factor
			float speedFactor = Mathf.InverseLerp(startingSpeed, maxSpeed, CurrentSpeed);
			AddScore((scorePerSecondAtBaseSpeed * (0.6f + speedFactor)) * Time.deltaTime);
		}

		public void ResetGame()
		{
			Time.timeScale = 1f;
			IsPaused = false;
			CurrentSpeed = startingSpeed;
			CurrentSpawnInterval = startingSpawnInterval;
			Score = 0f;
			IsRunning = true;
			OnScoreChanged?.Invoke(Score);
		}

		public void GameOver()
		{
			if (!IsRunning) return;
			IsRunning = false;
			OnGameOver?.Invoke();
		}

		public void AddScore(float amount)
		{
			if (amount == 0f) return;
			Score += amount;
			if (Score < 0f) Score = 0f;
			OnScoreChanged?.Invoke(Score);
		}

		public void PauseGame()
		{
			if (IsPaused) return;
			IsPaused = true;
			Time.timeScale = 0f;
			OnPauseChanged?.Invoke(true);
		}

		public void ResumeGame()
		{
			if (!IsPaused) return;
			IsPaused = false;
			Time.timeScale = 1f;
			OnPauseChanged?.Invoke(false);
		}

		public void TogglePause()
		{
			if (IsPaused) ResumeGame(); else PauseGame();
		}
	}
}