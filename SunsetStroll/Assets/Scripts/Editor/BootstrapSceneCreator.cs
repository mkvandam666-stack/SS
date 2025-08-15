#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SunsetStroll.EditorTools
{
	public static class BootstrapSceneCreator
	{
#if UNITY_EDITOR
		[MenuItem("Sunset Stroll/Create Demo Scene")] 
		public static void CreateDemoScene()
		{
			var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
			scene.name = "SunsetStroll_Demo";

			// Camera setup
			Camera cam = Object.FindObjectOfType<Camera>();
			if (cam == null)
			{
				GameObject camGO = new GameObject("Main Camera");
				cam = camGO.AddComponent<Camera>();
				cam.tag = "MainCamera";
			}
			cam.orthographic = true;
			cam.orthographicSize = 3.5f;
			cam.transform.position = new Vector3(0f, 0f, -10f);

			// GameManager
			new GameObject("GameManager").AddComponent<SunsetStroll.GameManager>();

			// Background root
			GameObject bgRoot = new GameObject("Background");

			// Sky gradient (simple big quad)
			CreateParallaxBand(bgRoot.transform, "Sky", -6f, new Color(0.98f, 0.6f, 0.4f), new Color(0.5f, 0.2f, 0.6f), 0.05f, 40f);
			CreateBuildingLayer(bgRoot.transform, "FarBuildings", 0.15f, 30f, 3f, new Color(0.15f,0.1f,0.2f));
			CreateBuildingLayer(bgRoot.transform, "MidBuildings", 0.3f, 24f, 2.5f, new Color(0.2f,0.15f,0.25f));
			CreateLampLayer(bgRoot.transform, "NearLamps", 0.6f, 20f, new Color(1f,0.9f,0.7f));

			// Ground tiles
			GameObject groundRoot = new GameObject("Ground");
			var groundScroll = groundRoot.AddComponent<SunsetStroll.World.ScrollingMover>();
			groundScroll.GetType().GetField("extraSpeedMultiplier", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(groundScroll, 1f);
			groundScroll.GetType().GetField("destroyX", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(groundScroll, -9999f);
			groundScroll.GetType().GetField("destroyOffscreen", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(groundScroll, false);
			var tiler = groundRoot.AddComponent<SunsetStroll.World.GroundTiler>();
			float tileWidth = 8f;
			int tileCount = 6;
			Transform[] tiles = new Transform[tileCount];
			for (int i = 0; i < tileCount; i++)
			{
				GameObject tile = CreateRect("GroundTile_"+i, new Vector2(tileWidth, 1.2f), new Color(0.15f,0.15f,0.18f));
				var sr = tile.GetComponentInChildren<SpriteRenderer>();
				sr.sortingOrder = 10;
				BoxCollider2D col = tile.AddComponent<BoxCollider2D>();
				col.offset = new Vector2(0f, 0.4f);
				col.size = new Vector2(tileWidth, 0.4f);
				tile.transform.position = new Vector3(-tileWidth*2 + i*tileWidth, -2.5f, 0f);
				tiles[i] = tile.transform;
			}
			tiler.GetType().GetField("tiles", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(tiler, tiles);
			tiler.GetType().GetField("tileWidth", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(tiler, tileWidth);

			// Player
			GameObject player = new GameObject("Jogger");
			player.transform.position = new Vector3(-2f, -1.9f, 0f);
			var srp = player.AddComponent<SpriteRenderer>();
			srp.sortingOrder = 20;
			var rb = player.AddComponent<Rigidbody2D>();
			rb.freezeRotation = true;
			rb.gravityScale = 3f;
			var col = player.AddComponent<CapsuleCollider2D>();
			col.size = new Vector2(0.6f, 1.4f);
			col.offset = new Vector2(0f, 0.7f);
			var animator = player.AddComponent<Animator>();

			// Placeholder jogging animation controller
			RuntimeAnimatorController controller = BuildJoggerAnimator();
			animator.runtimeAnimatorController = controller;

			var jogger = player.AddComponent<SunsetStroll.Player.JoggerController>();
			jogger.GetType().GetField("animator", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(jogger, animator);

			// Ground check
			GameObject gc = new GameObject("GroundCheck");
			gc.transform.SetParent(player.transform);
			gc.transform.localPosition = new Vector3(0f, -0.1f, 0f);
			jogger.GetType().GetField("groundCheck", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(jogger, gc.transform);
			LayerMask groundMask = LayerMask.GetMask("Default");
			jogger.GetType().GetField("groundLayer", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(jogger, groundMask);

			// Camera follow wiring
			var follow = cam.gameObject.AddComponent<SunsetStroll.World.FollowCamera>();
			follow.GetType().GetField("target", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(follow, player.transform);
			follow.GetType().GetField("offset", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(follow, new Vector2(0f, 0f));
			follow.GetType().GetField("followY", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(follow, false);

			// Spawner
			GameObject spawnerGO = new GameObject("ObstacleSpawner");
			spawnerGO.transform.position = new Vector3(12f, -1.5f, 0f);
			var spawner = spawnerGO.AddComponent<SunsetStroll.Spawning.ObstacleSpawner>();
			spawner.GetType().GetField("spawnPoint", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(spawner, spawnerGO.transform);
			GameObject[] prefabs = new GameObject[5];
			prefabs[0] = SunsetStroll.Spawning.ObstaclePlaceholderFactory.CreateBoxObstacle("TrafficCone", new Vector2(0.6f, 0.8f), new Color(1f,0.5f,0.1f));
			prefabs[1] = SunsetStroll.Spawning.ObstaclePlaceholderFactory.CreateBoxObstacle("Pothole", new Vector2(1.2f, 0.2f), new Color(0.2f,0.2f,0.2f));
			prefabs[2] = SunsetStroll.Spawning.ObstaclePlaceholderFactory.CreateBoxObstacle("Barrier", new Vector2(1.4f, 0.9f), new Color(0.9f,0.2f,0.2f));
			prefabs[3] = SunsetStroll.Spawning.ObstaclePlaceholderFactory.CreateBoxObstacle("GarbageBin", new Vector2(0.9f, 1.1f), new Color(0.2f,0.6f,0.2f));
			prefabs[4] = SunsetStroll.Spawning.ObstaclePlaceholderFactory.CreateBoxObstacle("Dog", new Vector2(1.0f, 0.6f), new Color(0.6f,0.4f,0.2f));
			spawner.GetType().GetField("obstaclePrefabs", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(spawner, prefabs);

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
			text.rectTransform.anchorMin = new Vector2(0,1);
			text.rectTransform.anchorMax = new Vector2(0,1);
			text.rectTransform.pivot = new Vector2(0,1);
			text.rectTransform.anchoredPosition = new Vector2(12, -12);

			// Hook score updates
			SunsetStroll.GameManager.Instance.OnScoreChanged += (s)=> { if (text!=null) text.text = $"Score: {Mathf.RoundToInt(s)}"; };

			EditorSceneManager.MarkSceneDirty(scene);
		}

		private static GameObject CreateRect(string name, Vector2 size, Color color)
		{
			GameObject root = new GameObject(name);
			GameObject visual = new GameObject(name + "_Visual");
			visual.transform.SetParent(root.transform);
			visual.transform.localPosition = Vector3.zero;
			var sr = visual.AddComponent<SpriteRenderer>();
			sr.sprite = BuildSolidSprite(color);
			// scale visual so sprite covers desired world size (sprite is 0.02 units at PPU 100)
			visual.transform.localScale = new Vector3(size.x * 50f, size.y * 50f, 1f);
			return root;
		}

		private static Sprite BuildSolidSprite(Color color)
		{
			Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			tex.SetPixels(new Color[] { color, color, color, color });
			tex.Apply();
			return Sprite.Create(tex, new Rect(0,0,2,2), new Vector2(0.5f,0.5f), 100f);
		}

		private static void CreateParallaxBand(Transform parent, string name, float y, Color top, Color bottom, float ratio, float width)
		{
			GameObject root = new GameObject(name);
			root.transform.SetParent(parent);
			var layer = root.AddComponent<SunsetStroll.World.ParallaxLayer>();
			layer.GetType().GetField("parallaxRatio", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(layer, ratio);
			float segW = width;
			layer.GetType().GetField("segmentWidth", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(layer, segW);
			int segments = 3;
			Transform[] segs = new Transform[segments];
			for (int i=0;i<segments;i++)
			{
				GameObject g = CreateGradientQuad($"{name}_Seg_{i}", top, bottom, new Vector2(segW, 12f));
				g.transform.SetParent(root.transform);
				g.transform.position = new Vector3((i-1)*segW, y, 5f);
				segs[i] = g.transform;
			}
			layer.GetType().GetField("segments", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(layer, segs);
		}

		private static GameObject CreateGradientQuad(string name, Color top, Color bottom, Vector2 size)
		{
			GameObject go = new GameObject(name);
			var sr = go.AddComponent<SpriteRenderer>();
			sr.sprite = BuildSolidSprite(Color.Lerp(top,bottom,0.5f));
			sr.sortingOrder = -10;
			// scale to requested size (sprite is 0.02 units square at PPU=100)
			go.transform.localScale = new Vector3(size.x * 50f, size.y * 50f, 1f);
			return go;
		}

		private static void CreateBuildingLayer(Transform parent, string name, float ratio, float width, float height, Color color)
		{
			GameObject root = new GameObject(name);
			root.transform.SetParent(parent);
			var layer = root.AddComponent<SunsetStroll.World.ParallaxLayer>();
			layer.GetType().GetField("parallaxRatio", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(layer, ratio);
			layer.GetType().GetField("segmentWidth", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(layer, width);
			int segments = 3;
			Transform[] segs = new Transform[segments];
			for (int i=0;i<segments;i++)
			{
				GameObject g = new GameObject($"{name}_Seg_{i}");
				var parentSr = g.AddComponent<SpriteRenderer>();
				parentSr.sprite = BuildSolidSprite(new Color(0,0,0,0));
				parentSr.sortingOrder = -5;
				g.transform.SetParent(root.transform);
				g.transform.position = new Vector3((i-1)*width, -0.5f, 2f);
				for (int b=0;b<6;b++)
				{
					float w = Random.Range(1f, 2.2f);
					float h = Random.Range(height*0.6f, height*1.1f);
					GameObject bgo = CreateRect($"B{b}", new Vector2(w, h), color);
					bgo.transform.SetParent(g.transform);
					bgo.transform.localPosition = new Vector3(-width/2 + 1f + b* (width/6f), -2.5f + h/2f, 0f);
				}
				segs[i] = g.transform;
			}
			layer.GetType().GetField("segments", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(layer, segs);
		}

		private static void CreateLampLayer(Transform parent, string name, float ratio, float width, Color glow)
		{
			GameObject root = new GameObject(name);
			root.transform.SetParent(parent);
			var layer = root.AddComponent<SunsetStroll.World.ParallaxLayer>();
			layer.GetType().GetField("parallaxRatio", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(layer, ratio);
			layer.GetType().GetField("segmentWidth", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(layer, width);
			int segments = 3;
			Transform[] segs = new Transform[segments];
			for (int i=0;i<segments;i++)
			{
				GameObject g = new GameObject($"{name}_Seg_{i}");
				var parentSr = g.AddComponent<SpriteRenderer>();
				parentSr.sprite = BuildSolidSprite(new Color(0,0,0,0));
				parentSr.sortingOrder = 0;
				g.transform.SetParent(parent);
				g.transform.position = new Vector3((i-1)*width, -1.3f, 1f);
				for (int l=0;l<4;l++)
				{
					GameObject pole = CreateRect($"Pole{l}", new Vector2(0.1f, 1.8f), new Color(0.1f,0.1f,0.12f));
					pole.transform.SetParent(g.transform);
					pole.transform.localPosition = new Vector3(-width/2 + 1.5f + l* (width/4f), -1.6f + 0.9f, 0f);
					GameObject lamp = CreateRect($"Lamp{l}", new Vector2(0.4f, 0.2f), glow);
					lamp.transform.SetParent(g.transform);
					lamp.transform.localPosition = new Vector3(-width/2 + 1.5f + l* (width/4f), -0.5f, 0f);
				}
				segs[i] = g.transform;
			}
			layer.GetType().GetField("segments", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(layer, segs);
		}

		private static RuntimeAnimatorController BuildJoggerAnimator()
		{
			// Build simple controller with Speed float, Jump trigger, Slide bool
			var controller = new UnityEditor.Animations.AnimatorController();
			controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
			controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
			controller.AddParameter("Slide", AnimatorControllerParameterType.Bool);

			var rootStateMachine = controller.layers[0].stateMachine;
			AnimationClip run = MakeRunClip();
			AnimationClip jump = MakeJumpClip();
			AnimationClip slide = MakeSlideClip();
			var runState = rootStateMachine.AddState("Run");
			runState.motion = run;
			var jumpState = rootStateMachine.AddState("Jump");
			jumpState.motion = jump;
			var slideState = rootStateMachine.AddState("Slide");
			slideState.motion = slide;
			rootStateMachine.defaultState = runState;

			var t1 = rootStateMachine.AddAnyStateTransition(jumpState);
			t1.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Jump");
			t1.hasExitTime = false; t1.duration = 0.05f;

			var t2 = rootStateMachine.AddAnyStateTransition(slideState);
			t2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Slide");
			t2.hasExitTime = false; t2.duration = 0.05f;

			var t3 = slideState.AddTransition(runState);
			t3.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "Slide");
			t3.hasExitTime = true; t3.exitTime = 0.05f; t3.duration = 0.05f;

			var t4 = jumpState.AddTransition(runState);
			t4.hasExitTime = true; t4.exitTime = 0.95f; t4.duration = 0.05f;

			if (!AssetDatabase.IsValidFolder("Assets/Placeholders")) AssetDatabase.CreateFolder("Assets", "Placeholders");
			AssetDatabase.CreateAsset(controller, "Assets/Placeholders/Jogger.controller");
			AssetDatabase.AddObjectToAsset(run, controller);
			AssetDatabase.AddObjectToAsset(jump, controller);
			AssetDatabase.AddObjectToAsset(slide, controller);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			return controller;
		}

		private static AnimationClip MakeRunClip()
		{
			var clip = new AnimationClip();
			clip.frameRate = 12f;
			// Animate subtle vertical bob
			var curve = AnimationCurve.EaseInOut(0, -0.02f, 0.5f, 0.02f);
			var curve2 = AnimationCurve.EaseInOut(0.5f, 0.02f, 1f, -0.02f);
			AnimationCurve yCurve = new AnimationCurve(curve.keys);
			foreach (var k in curve2.keys) yCurve.AddKey(k);
			clip.SetCurve("", typeof(Transform), "localPosition.y", yCurve);
			clip.wrapMode = WrapMode.Loop;
			return clip;
		}

		private static AnimationClip MakeJumpClip()
		{
			var clip = new AnimationClip();
			clip.frameRate = 12f;
			AnimationCurve yCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
			clip.SetCurve("", typeof(Transform), "localPosition.y", yCurve);
			return clip;
		}

		private static AnimationClip MakeSlideClip()
		{
			var clip = new AnimationClip();
			clip.frameRate = 12f;
			AnimationCurve yCurve = new AnimationCurve(new Keyframe(0, -0.1f), new Keyframe(1, -0.1f));
			clip.SetCurve("", typeof(Transform), "localPosition.y", yCurve);
			clip.wrapMode = WrapMode.Loop;
			return clip;
		}
#endif
	}
}