using UnityEngine;

namespace SunsetStroll.World
{
	public class ScrollingMover : MonoBehaviour
	{
		[SerializeField] private float extraSpeedMultiplier = 1f;
		[SerializeField] private float destroyX = -30f;
		[SerializeField] private bool destroyOffscreen = true;

		private void Update()
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning || SunsetStroll.GameManager.Instance.IsPaused) return;
			float speed = SunsetStroll.GameManager.Instance.CurrentSpeed * extraSpeedMultiplier;
			transform.Translate(Vector3.left * speed * Time.deltaTime);

			if (destroyOffscreen && transform.position.x < destroyX)
			{
				Destroy(gameObject);
			}
		}
	}
}