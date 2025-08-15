#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SunsetStroll.Spawning
{
	public static class ObstaclePlaceholderFactory
	{
		public static GameObject CreateBoxObstacle(string name, Vector2 size, Color color)
		{
			GameObject go = new GameObject(name);
			var sr = go.AddComponent<SpriteRenderer>();
			sr.sprite = GenerateSprite();
			sr.color = color;
			sr.sortingOrder = 15;
			// scale sprite to desired world size (sprite is 0.02 units square at PPU=100)
			go.transform.localScale = new Vector3(size.x * 50f, size.y * 50f, 1f);

			var bc = go.AddComponent<BoxCollider2D>();
			bc.isTrigger = false;
			bc.size = new Vector2(0.02f, 0.02f);
			bc.offset = new Vector2(0f, 0.01f);

			var rb = go.AddComponent<Rigidbody2D>();
			rb.bodyType = RigidbodyType2D.Kinematic;

			go.AddComponent<SunsetStroll.World.Obstacle>();
			go.AddComponent<SunsetStroll.World.ScrollingMover>();
			return go;
		}

		private static Sprite GenerateSprite()
		{
			Texture2D tex = new Texture2D(2, 2);
			tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
			tex.Apply();
			Rect rect = new Rect(0, 0, 2, 2);
			Vector2 pivot = new Vector2(0.5f, 0f);
			Sprite sprite = Sprite.Create(tex, rect, pivot, 100f);
			sprite.name = "PlaceholderSprite";
			return sprite;
		}

#if UNITY_EDITOR
		[MenuItem("Sunset Stroll/Create Placeholder Obstacles")] 
		public static void CreatePrefabs()
		{
			string dir = "Assets/Placeholders/Obstacles";
			if (!AssetDatabase.IsValidFolder("Assets/Placeholders")) AssetDatabase.CreateFolder("Assets", "Placeholders");
			if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/Placeholders", "Obstacles");

			SavePrefab(CreateBoxObstacle("TrafficCone", new Vector2(0.6f, 0.8f), new Color(1f, 0.5f, 0.1f)), dir, "TrafficCone.prefab");
			SavePrefab(CreateBoxObstacle("Pothole", new Vector2(1.2f, 0.2f), new Color(0.2f, 0.2f, 0.2f)), dir, "Pothole.prefab");
			SavePrefab(CreateBoxObstacle("Barrier", new Vector2(1.4f, 0.9f), new Color(0.9f, 0.2f, 0.2f)), dir, "Barrier.prefab");
			SavePrefab(CreateBoxObstacle("GarbageBin", new Vector2(0.9f, 1.1f), new Color(0.2f, 0.6f, 0.2f)), dir, "GarbageBin.prefab");
			SavePrefab(CreateBoxObstacle("Dog", new Vector2(1.0f, 0.6f), new Color(0.6f, 0.4f, 0.2f)), dir, "Dog.prefab");

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private static void SavePrefab(GameObject go, string dir, string fileName)
		{
			string path = $"{dir}/{fileName}";
			PrefabUtility.SaveAsPrefabAsset(go, path);
			Object.DestroyImmediate(go);
		}
#endif
	}
}