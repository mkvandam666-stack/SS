using UnityEngine;

namespace SunsetStroll.World
{
	public class TrackLooper : MonoBehaviour
	{
		[SerializeField] private Transform[] chunks; // pre-placed chunk roots laid out in a row
		[SerializeField] private float segmentWidth = 12f;
		[SerializeField] private float recycleBuffer = 2f;

		private void Reset()
		{
			chunks = new Transform[transform.childCount];
			for (int i = 0; i < transform.childCount; i++) chunks[i] = transform.GetChild(i);
		}

		private void Update()
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning || SunsetStroll.GameManager.Instance.IsPaused) return;
			if (Camera.main == null || chunks == null || chunks.Length == 0) return;

			float leftEdge = Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect - recycleBuffer;
			// find leftmost chunk
			int leftmostIndex = 0;
			for (int i = 1; i < chunks.Length; i++)
			{
				if (chunks[i].position.x < chunks[leftmostIndex].position.x) leftmostIndex = i;
			}

			Transform leftmost = chunks[leftmostIndex];
			if (leftmost.position.x + segmentWidth < leftEdge)
			{
				// move to right of rightmost
				int rightmostIndex = 0;
				for (int i = 1; i < chunks.Length; i++)
				{
					if (chunks[i].position.x > chunks[rightmostIndex].position.x) rightmostIndex = i;
				}
				Transform rightmost = chunks[rightmostIndex];
				leftmost.position = new Vector3(rightmost.position.x + segmentWidth, leftmost.position.y, leftmost.position.z);
			}
		}
	}
}