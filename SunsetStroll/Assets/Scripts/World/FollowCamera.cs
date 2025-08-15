using UnityEngine;

namespace SunsetStroll.World
{
	[RequireComponent(typeof(Camera))]
	public class FollowCamera : MonoBehaviour
	{
		[SerializeField] private Transform target;
		[SerializeField] private Vector2 offset = new Vector2(0f, 0f);
		[SerializeField] private bool followY = false;
		[SerializeField] private float smoothTime = 0.12f;
		private Vector3 velocity;

		private void LateUpdate()
		{
			if (target == null) return;
			Vector3 pos = transform.position;
			float y = followY ? target.position.y + offset.y : pos.y;
			Vector3 desired = new Vector3(offset.x, y, pos.z);
			transform.position = Vector3.SmoothDamp(pos, desired, ref velocity, smoothTime);
		}
	}
}