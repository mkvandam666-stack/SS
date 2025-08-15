using UnityEngine;

namespace SunsetStroll.World
{
	public class ParallaxLayer : MonoBehaviour
	{
		[SerializeField] private float parallaxRatio = 0.3f; // 0..1, smaller = farther
		[SerializeField] private float segmentWidth = 20f;
		[SerializeField] private Transform[] segments; // 2 or 3 children for looping
		[SerializeField] private bool useGameSpeed = true;
		[SerializeField] private float manualSpeed = 2f;

		private void Reset()
		{
			segments = new Transform[transform.childCount];
			for (int i = 0; i < transform.childCount; i++) segments[i] = transform.GetChild(i);
		}

		private void Update()
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning || SunsetStroll.GameManager.Instance.IsPaused) return;
			float speed = useGameSpeed ? SunsetStroll.GameManager.Instance.CurrentSpeed * parallaxRatio : manualSpeed;
			transform.Translate(Vector3.left * speed * Time.deltaTime);
			LoopSegmentsIfNeeded();
		}

		private void LoopSegmentsIfNeeded()
		{
			if (segments == null || segments.Length == 0) return;
			for (int i = 0; i < segments.Length; i++)
			{
				Transform seg = segments[i];
				if (Camera.main == null) continue;
				float leftEdge = Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect - segmentWidth;
				if (seg.position.x < leftEdge)
				{
					// move this segment to the right of the rightmost segment
					Transform rightmost = seg;
					for (int j = 0; j < segments.Length; j++)
					{
						if (segments[j].position.x > rightmost.position.x) rightmost = segments[j];
					}
					seg.position = new Vector3(rightmost.position.x + segmentWidth, seg.position.y, seg.position.z);
				}
			}
		}
	}
}