using UnityEngine;

namespace SunsetStroll.Collectibles
{
	[RequireComponent(typeof(Collider2D))]
	public class Coin : MonoBehaviour
	{
		[SerializeField] private float bonusScore = 50f;
		[SerializeField] private AudioClip collectClip;
		[SerializeField] private AudioSource sfxSource;
		[SerializeField] private float spinSpeed = 180f;

		private void Reset()
		{
			var col = GetComponent<Collider2D>();
			col.isTrigger = true;
			var sr = GetComponent<SpriteRenderer>();
			if (sr == null)
			{
				gameObject.AddComponent<SpriteRenderer>();
			}
			if (GetComponent<SunsetStroll.World.ScrollingMover>() == null)
			{
				gameObject.AddComponent<SunsetStroll.World.ScrollingMover>();
			}
		}

		private void Update()
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning || SunsetStroll.GameManager.Instance.IsPaused) return;
			transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning) return;
			if (other.GetComponent<SunsetStroll.Player.JoggerController>() != null)
			{
				SunsetStroll.GameManager.Instance.AddScore(bonusScore);
				if (sfxSource && collectClip) sfxSource.PlayOneShot(collectClip, 0.9f);
				Destroy(gameObject);
			}
		}
	}
}