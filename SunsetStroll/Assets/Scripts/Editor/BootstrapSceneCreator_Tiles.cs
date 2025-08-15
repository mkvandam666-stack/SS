#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace SunsetStroll.EditorTools
{
	public static class BootstrapSceneCreator_Tiles
	{
#if UNITY_EDITOR
		[MenuItem("Sunset Stroll/Create Demo Scene (Chunked Obstacles)")] 
		public static void CreateChunkedScene()
		{
			var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

			// Camera & manager
			Camera cam = Object.FindObjectOfType<Camera>();
			if (!cam)
			{
				GameObject camGO = new GameObject("Main Camera");
				cam = camGO.AddComponent<Camera>();
				cam.tag = "MainCamera";
			}
			cam.orthographic = true;
			cam.orthographicSize = 3.5f;
			cam.transform.position = new Vector3(0f, 0f, -10f);
			new GameObject("GameManager").AddComponent<SunsetStroll.GameManager>();

			// Chunks container with Scroller + TrackLooper
			GameObject chunksRoot = new GameObject("Chunks");
			chunksRoot.transform.position = Vector3.zero;
			var scroller = chunksRoot.AddComponent<SunsetStroll.World.ScrollingMover>();
			scroller.GetType().GetField("destroyOffscreen", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(scroller, false);
			var looper = chunksRoot.AddComponent<SunsetStroll.World.TrackLooper>();
			looper.GetType().GetField("segmentWidth", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(looper, 12f);

			// Build 4 chunks with baked obstacles
			int chunkCount = 4;
			Transform[] chunks = new Transform[chunkCount];
			for (int i=0;i<chunkCount;i++)
			{
				GameObject chunk = new GameObject($"Chunk_{i}");
				chunk.transform.SetParent(chunksRoot.transform);
				chunk.transform.position = new Vector3(i*12f, 0f, 0f);
				BuildChunkContent(chunk.transform, i);
				chunks[i] = chunk.transform;
			}
			looper.GetType().GetField("chunks", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(looper, chunks);

			// Player
			GameObject player = new GameObject("Jogger");
			player.transform.position = new Vector3(-2f, -1.9f, 0f);
			var srp = player.AddComponent<SpriteRenderer>();
			srp.sortingOrder = 20;
			var rb = player.AddComponent<Rigidbody2D>(); rb.freezeRotation = true; rb.gravityScale = 3f;
			var cap = player.AddComponent<CapsuleCollider2D>(); cap.size = new Vector2(0.6f, 1.4f); cap.offset = new Vector2(0f, 0.7f);
			var animator = player.AddComponent<Animator>();
			animator.runtimeAnimatorController = BootstrapSceneCreator_Builders.GetOrCreateJoggerAnimator();
			var jogger = player.AddComponent<SunsetStroll.Player.JoggerController>();
			GameObject gc = new GameObject("GroundCheck"); gc.transform.SetParent(player.transform); gc.transform.localPosition = new Vector3(0f,-0.1f,0f);
			jogger.GetType().GetField("groundCheck", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(jogger, gc.transform);
			jogger.GetType().GetField("groundLayer", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(jogger, LayerMask.GetMask("Default"));

			// Follow camera
			var follow = cam.gameObject.AddComponent<SunsetStroll.World.FollowCamera>();
			follow.GetType().GetField("target", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(follow, player.transform);

			// UI Canvas
			GameObject canvasGO = new GameObject("Canvas");
			var canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvasGO.AddComponent<CanvasScaler>();
			canvasGO.AddComponent<GraphicRaycaster>();
			GameObject textGO = new GameObject("ScoreText");
			textGO.transform.SetParent(canvasGO.transform);
			var text = textGO.AddComponent<Text>();
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.fontSize = 28;
			text.alignment = TextAnchor.UpperLeft;
			text.color = Color.white;
			text.text = "Score: 0";
			text.rectTransform.anchorMin = new Vector2(0,1);
			text.rectTransform.anchorMax = new Vector2(0,1);
			text.rectTransform.pivot = new Vector2(0,1);
			text.rectTransform.anchoredPosition = new Vector2(12, -12);

			// Pause overlay
			GameObject pauseGO = new GameObject("PauseOverlay");
			pauseGO.transform.SetParent(canvasGO.transform);
			var pauseText = pauseGO.AddComponent<Text>();
			pauseText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			pauseText.fontSize = 36;
			pauseText.alignment = TextAnchor.MiddleCenter;
			pauseText.color = new Color(1,1,1,0.9f);
			pauseText.text = "Paused\nPress Esc to resume";
			pauseText.rectTransform.anchorMin = new Vector2(0,0);
			pauseText.rectTransform.anchorMax = new Vector2(1,1);
			pauseText.rectTransform.offsetMin = Vector2.zero;
			pauseText.rectTransform.offsetMax = Vector2.zero;
			pauseText.gameObject.SetActive(false);

			// Hook score and pause updates
			SunsetStroll.GameManager.Instance.OnScoreChanged += (s)=> { if (text!=null) text.text = $"Score: {Mathf.RoundToInt(s)}"; };
			SunsetStroll.GameManager.Instance.OnPauseChanged += (p)=> { if (pauseText!=null) pauseText.gameObject.SetActive(p); };

			// Pause input
			cam.gameObject.AddComponent<PauseInputListener>();

			EditorSceneManager.MarkSceneDirty(scene);
		}

		private static void BuildChunkContent(Transform root, int seed)
		{
			// Ground visual and collider (attached to chunk root)
			GameObject ground = new GameObject("GroundVisual");
			ground.transform.SetParent(root);
			var sr = ground.AddComponent<SpriteRenderer>();
			sr.sprite = BootstrapSceneCreator_Builders.SolidSprite(new Color(0.15f,0.15f,0.18f));
			ground.transform.localScale = new Vector3(12f*50f, 1.2f*50f, 1f);
			var col = root.gameObject.AddComponent<BoxCollider2D>();
			col.size = new Vector2(12f, 0.4f); col.offset = new Vector2(0f, -2.1f);

			// Baked obstacles inside the chunk (no individual scrollers)
			Random.InitState(1234 + seed);
			for (int i=0;i<3;i++)
			{
				float x = -5f + i*4f + Random.Range(-0.5f, 0.5f);
				float y = -2.2f;
				var obs = SunsetStroll.Spawning.ObstaclePlaceholderFactory.CreateBoxObstacle($"Obs_{seed}_{i}", new Vector2(Random.Range(0.6f,1.3f), Random.Range(0.4f,1.0f)), new Color(0.8f,0.3f,0.2f));
				// remove individual scroller so movement is driven by chunk root
				var sc = obs.GetComponent<SunsetStroll.World.ScrollingMover>();
				if (sc) Object.DestroyImmediate(sc);
				obs.transform.SetParent(root);
				obs.transform.position = new Vector3(root.position.x + x, y, 0f);
			}
		}
#endif
	}

	// Helpers to share assets with the main bootstrap
	internal static class BootstrapSceneCreator_Builders
	{
#if UNITY_EDITOR
		public static RuntimeAnimatorController GetOrCreateJoggerAnimator()
		{
			string path = "Assets/Placeholders/Jogger.controller";
			var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
			if (controller) return controller;

			// Build simple controller similar to the main bootstrap
			var ac = new AnimatorController();
			ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
			ac.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
			ac.AddParameter("Slide", AnimatorControllerParameterType.Bool);
			var sm = ac.layers[0].stateMachine;
			AnimationClip run = MakeRunClip();
			AnimationClip jump = MakeJumpClip();
			AnimationClip slide = MakeSlideClip();
			var runState = sm.AddState("Run"); runState.motion = run; sm.defaultState = runState;
			var jumpState = sm.AddState("Jump"); jumpState.motion = jump;
			var slideState = sm.AddState("Slide"); slideState.motion = slide;
			var t1 = sm.AddAnyStateTransition(jumpState); t1.AddCondition(AnimatorConditionMode.If, 0, "Jump"); t1.hasExitTime = false; t1.duration = 0.05f;
			var t2 = sm.AddAnyStateTransition(slideState); t2.AddCondition(AnimatorConditionMode.If, 0, "Slide"); t2.hasExitTime = false; t2.duration = 0.05f;
			var t3 = slideState.AddTransition(runState); t3.AddCondition(AnimatorConditionMode.IfNot, 0, "Slide"); t3.hasExitTime = true; t3.exitTime = 0.05f; t3.duration = 0.05f;
			var t4 = jumpState.AddTransition(runState); t4.hasExitTime = true; t4.exitTime = 0.95f; t4.duration = 0.05f;
			if (!AssetDatabase.IsValidFolder("Assets/Placeholders")) AssetDatabase.CreateFolder("Assets", "Placeholders");
			AssetDatabase.CreateAsset(ac, path);
			AssetDatabase.AddObjectToAsset(run, ac); AssetDatabase.AddObjectToAsset(jump, ac); AssetDatabase.AddObjectToAsset(slide, ac);
			AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
			return ac as RuntimeAnimatorController;
		}

		private static AnimationClip MakeRunClip()
		{
			var clip = new AnimationClip(); clip.frameRate = 12f;
			var curve = AnimationCurve.EaseInOut(0, -0.02f, 0.5f, 0.02f);
			var curve2 = AnimationCurve.EaseInOut(0.5f, 0.02f, 1f, -0.02f);
			AnimationCurve yCurve = new AnimationCurve(curve.keys); foreach (var k in curve2.keys) yCurve.AddKey(k);
			clip.SetCurve("", typeof(Transform), "localPosition.y", yCurve); clip.wrapMode = WrapMode.Loop; return clip;
		}
		private static AnimationClip MakeJumpClip()
		{
			var clip = new AnimationClip(); clip.frameRate = 12f; clip.SetCurve("", typeof(Transform), "localPosition.y", AnimationCurve.EaseInOut(0, 0, 1, 0)); return clip;
		}
		private static AnimationClip MakeSlideClip()
		{
			var clip = new AnimationClip(); clip.frameRate = 12f; clip.SetCurve("", typeof(Transform), "localPosition.y", new AnimationCurve(new Keyframe(0, -0.1f), new Keyframe(1, -0.1f))); clip.wrapMode = WrapMode.Loop; return clip;
		}

		private class PauseInputListener : MonoBehaviour
		{
			private void Update()
			{
				if (Input.GetKeyDown(KeyCode.Escape)) SunsetStroll.GameManager.Instance.TogglePause();
			}
		}
#endif
	}
}