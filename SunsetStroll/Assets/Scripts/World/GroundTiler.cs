using UnityEngine;

namespace SunsetStroll.World
{
	public class GroundTiler : MonoBehaviour
	{
		[SerializeField] private Transform[] tiles; // pre-placed ground tiles in a row
		[SerializeField] private float tileWidth = 5f;
		[SerializeField] private float recycleBuffer = 2f;

		private void Update()
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning) return;
			if (Camera.main == null || tiles == null || tiles.Length == 0) return;

			float leftEdge = Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect - recycleBuffer;
			// find leftmost tile
			int leftmostIndex = 0;
			for (int i = 1; i < tiles.Length; i++)
			{
				if (tiles[i].position.x < tiles[leftmostIndex].position.x) leftmostIndex = i;
			}

			Transform leftmost = tiles[leftmostIndex];
			if (leftmost.position.x + tileWidth < leftEdge)
			{
				// move to right of rightmost
				int rightmostIndex = 0;
				for (int i = 1; i < tiles.Length; i++)
				{
					if (tiles[i].position.x > tiles[rightmostIndex].position.x) rightmostIndex = i;
				}
				Transform rightmost = tiles[rightmostIndex];
				leftmost.position = new Vector3(rightmost.position.x + tileWidth, leftmost.position.y, leftmost.position.z);
			}
		}
	}
}