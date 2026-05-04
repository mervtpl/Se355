using UnityEngine;
using UnityEngine.Rendering;

public static class FlyingEnemyDemoBootstrap
{
    private const string DemoRootName = "__FlyingEnemyDemo";
    private const string BackgroundImageFileName = "Background.jpg";

    public static void EnsureSceneSetup()
    {
        var root = GameObject.Find(DemoRootName);
        if (root == null)
        {
            root = new GameObject(DemoRootName);
        }

        var camera = Camera.main;
        if (camera != null)
        {
            ConfigureCamera(camera);
        }

        EnsureDirectionalLight(root.transform);
        EnsureBackground(root.transform);
        EnsureGround(root.transform);

        var player = EnsurePlayer(root.transform);
        var virusTemplate = EnsureVirusTemplate(root.transform);
        EnsureSpawner(root.transform, player, virusTemplate);
    }

    private static void ConfigureCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = 9f;
        camera.transform.position = new Vector3(0f, 18f, 0f);
        camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.04f, 0.02f, 0.02f);
    }

    private static void EnsureDirectionalLight(Transform parent)
    {
        var lightObject = FindOrCreateChild(parent, "Demo Directional Light");
        lightObject.transform.rotation = Quaternion.Euler(55f, -30f, 0f);

        var light = GetOrAddComponent<Light>(lightObject);
        light.type = LightType.Directional;
        light.intensity = 1.4f;
        light.color = new Color(1f, 0.96f, 0.9f);
    }

    private static void EnsureGround(Transform parent)
    {
        var ground = FindOrCreatePrimitive(parent, "Ground Plane", PrimitiveType.Plane);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(2.4f, 1f, 2.4f);

        var groundRenderer = ground.GetComponent<Renderer>();
        groundRenderer.shadowCastingMode = ShadowCastingMode.Off;
        groundRenderer.receiveShadows = false;
        groundRenderer.material = CreateMaterial(new Color(0.18f, 0.07f, 0.07f), false, 0.2f);
    }

    private static void EnsureBackground(Transform parent)
    {
        var background = FindOrCreatePrimitive(parent, "Background Plane", PrimitiveType.Plane);
        background.transform.position = new Vector3(0f, -0.05f, 0f);
        background.transform.localScale = new Vector3(3.2f, 1f, 3.2f);

        var renderer = background.GetComponent<Renderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = CreateBackgroundMaterial();
    }

    private static Transform EnsurePlayer(Transform parent)
    {
        var player = FindOrCreatePrimitive(parent, "Player", PrimitiveType.Cube);
        player.transform.position = new Vector3(0f, 0.75f, 0f);
        player.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);

        var playerRenderer = player.GetComponent<Renderer>();
        playerRenderer.material = CreateMaterial(new Color(0.4f, 0.92f, 1f));

        var healthFeedback = GetOrAddComponent<PlayerHealthFeedback>(player);
        healthFeedback.hitFlashDuration = 0.6f;

        var controller = GetOrAddComponent<PlayerCubeController>(player);
        controller.moveSpeed = 4f;
        controller.sprintMultiplier = 1.8f;

        return player.transform;
    }

    private static GameObject EnsureVirusTemplate(Transform parent)
    {
        var virusTemplate = FindOrCreateChild(parent, "Virus Template");
        virusTemplate.transform.position = new Vector3(-6f, 1.8f, -6f);
        virusTemplate.transform.rotation = Quaternion.identity;
        virusTemplate.SetActive(false);

        var enemy = GetOrAddComponent<FlyingEnemy>(virusTemplate);
        enemy.playerTarget = parent.Find("Player");
        enemy.flightHeight = 1.8f;
        enemy.separationRadius = 1.35f;
        enemy.separationStrength = 1.6f;

        EnsureVirusVisual(virusTemplate.transform, enemy.EnemyColor);
        return virusTemplate;
    }

    private static void EnsureSpawner(Transform parent, Transform player, GameObject virusTemplate)
    {
        var spawnerObject = FindOrCreateChild(parent, "Flying Enemy Spawner");

        var spawner = GetOrAddComponent<FlyingEnemySpawner>(spawnerObject);
        spawner.playerTarget = player;
        spawner.enemyTemplate = virusTemplate;
        spawner.spawnInterval = 1.75f;
        spawner.maxEnemyCount = 8;
        spawner.spawnRadius = new Vector2(9f, 12f);
        spawner.enemyFlightHeight = 1.8f;
        spawner.minSpawnSeparation = 2f;
    }

    internal static Material CreateMaterial(Color color)
    {
        return CreateMaterial(color, true, 0f);
    }

    internal static Material CreateMaterial(Color color, bool lit, float alpha)
    {
        var shader = lit
            ? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")
            : Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        var material = new Material(shader);
        material.color = color;
        if (alpha > 0f)
        {
            material.color = new Color(color.r, color.g, color.b, 1f - alpha);
        }
        return material;
    }

    private static GameObject FindOrCreateChild(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child != null)
        {
            return child.gameObject;
        }

        var gameObject = new GameObject(childName);
        gameObject.transform.SetParent(parent);
        return gameObject;
    }

    private static GameObject FindOrCreatePrimitive(Transform parent, string childName, PrimitiveType primitiveType)
    {
        var child = parent.Find(childName);
        if (child != null)
        {
            return child.gameObject;
        }

        var gameObject = GameObject.CreatePrimitive(primitiveType);
        gameObject.name = childName;
        gameObject.transform.SetParent(parent);
        return gameObject;
    }

    internal static void EnsureVirusVisual(Transform root, Color enemyColor)
    {
        var body = FindOrCreatePrimitive(root, "Body", PrimitiveType.Sphere);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.72f, 0.34f, 0.72f);
        body.GetComponent<Renderer>().material = CreateMaterial(enemyColor);

        for (var i = 0; i < 8; i++)
        {
            var spike = FindOrCreatePrimitive(root, $"Spike {i + 1}", PrimitiveType.Capsule);
            var angle = i * Mathf.PI * 0.25f;
            var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            spike.transform.localPosition = direction * 0.48f;
            spike.transform.localRotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
            spike.transform.localScale = new Vector3(0.16f, 0.28f, 0.16f);
            spike.GetComponent<Renderer>().material = CreateMaterial(enemyColor);
        }

        var core = FindOrCreatePrimitive(root, "Core", PrimitiveType.Sphere);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        core.GetComponent<Renderer>().material = CreateMaterial(new Color(1f, 0.9f, 0.55f));
    }

    private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        if (gameObject.TryGetComponent<T>(out var component))
        {
            return component;
        }

        return gameObject.AddComponent<T>();
    }

    private static Material CreateBackgroundMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
        var material = new Material(shader);
        material.color = Color.white;
        var backgroundImagePath = System.IO.Path.Combine(Application.dataPath, BackgroundImageFileName);

        if (System.IO.File.Exists(backgroundImagePath))
        {
            var imageBytes = System.IO.File.ReadAllBytes(backgroundImagePath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (texture.LoadImage(imageBytes))
            {
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
                material.mainTexture = texture;
                material.mainTextureScale = Vector2.one;
                material.mainTextureOffset = Vector2.zero;
            }
        }
        else
        {
            material.color = new Color(0.22f, 0.04f, 0.04f);
        }

        return material;
    }
}
