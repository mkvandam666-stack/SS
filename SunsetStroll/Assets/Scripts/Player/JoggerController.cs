using UnityEngine;

namespace SunsetStroll.Player
{
	[RequireComponent(typeof(Collider2D))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class JoggerController : MonoBehaviour
	{
		[SerializeField] private Animator animator;
		[SerializeField] private string jumpParam = "Jump";
		[SerializeField] private string slideParam = "Slide";
		[SerializeField] private string speedParam = "Speed";

		[SerializeField] private float jumpForce = 9.5f;
		[SerializeField] private float gravityScale = 3f;
		[SerializeField] private Transform groundCheck;
		[SerializeField] private float groundCheckRadius = 0.1f;
		[SerializeField] private LayerMask groundLayer;

		[SerializeField] private AudioSource sfxSource;
		[SerializeField] private AudioClip jumpClip;
		[SerializeField] private AudioClip slideClip;
		[SerializeField] private AudioClip hitClip;

		private Rigidbody2D rb;
		private Collider2D col;
		private bool isGrounded;
		private bool isSliding;
		private float slideTimer;
		[SerializeField] private float slideDuration = 0.5f;

		private void Awake()
		{
			rb = GetComponent<Rigidbody2D>();
			col = GetComponent<Collider2D>();
			rb.freezeRotation = true;
			rb.gravityScale = gravityScale;
		}

		private void Update()
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning)
			{
				animator.SetFloat(speedParam, 0f);
				return;
			}

			animator.SetFloat(speedParam, 1f);
			HandleGroundCheck();
			HandleInput();
			HandleSlideTimer();
		}

		private void HandleGroundCheck()
		{
			if (groundCheck == null)
			{
				isGrounded = true; // fallback
				return;
			}
			isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
		}

		private void HandleInput()
		{
			if (isGrounded && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)))
			{
				Jump();
			}

			if (!isSliding && (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)))
			{
				StartSlide();
			}
		}

		private void HandleSlideTimer()
		{
			if (!isSliding) return;
			slideTimer -= Time.deltaTime;
			if (slideTimer <= 0f)
			{
				EndSlide();
			}
		}

		private void Jump()
		{
			animator.SetTrigger(jumpParam);
			rb.velocity = new Vector2(rb.velocity.x, 0f);
			rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
			if (sfxSource && jumpClip) sfxSource.PlayOneShot(jumpClip, 0.9f);
		}

		private void StartSlide()
		{
			isSliding = true;
			slideTimer = slideDuration;
			animator.SetBool(slideParam, true);
			if (sfxSource && slideClip) sfxSource.PlayOneShot(slideClip, 0.9f);
		}

		private void EndSlide()
		{
			isSliding = false;
			animator.SetBool(slideParam, false);
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (!SunsetStroll.GameManager.Instance.IsRunning) return;
			if (collision.collider.GetComponent<SunsetStroll.World.Obstacle>() != null)
			{
				if (sfxSource && hitClip) sfxSource.PlayOneShot(hitClip, 0.9f);
				SunsetStroll.GameManager.Instance.GameOver();
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (groundCheck == null) return;
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
		}
	}
}