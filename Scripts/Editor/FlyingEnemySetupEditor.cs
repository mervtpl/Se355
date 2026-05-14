using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class FlyingEnemySetupEditor
{
    private static readonly Vector2[] DefaultSpawnPositions =
    {
        new Vector2(-9f, 5f),
        new Vector2(0f, 6f),
        new Vector2(9f, 5f),
        new Vector2(-10f, -4f),
        new Vector2(0f, -6f),
        new Vector2(10f, -4f)
    };

    private const string CircleTexturePath = "Assets/Generated/FlyingEnemy/Circle.png";
    private const string OvalTexturePath = "Assets/Generated/FlyingEnemy/Oval.png";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    private const string BulletPrefabPath = "Assets/Prefabs/PlayerBullet.prefab";
    private const string FlyingEnemyPrefabPath = "Assets/Prefabs/FlyingEnemy.prefab";
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    public static void CreateOrRefreshSetup()
    {
        EnsureFolders();
        GenerateSpriteTextures();
        AssetDatabase.Refresh();
        ConfigureSpriteImporter(CircleTexturePath);
        ConfigureSpriteImporter(OvalTexturePath);
        AssetDatabase.Refresh();

        CreateOrRefreshPrefabs();
        CreateOrRefreshScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Flying Enemy/Create Or Refresh Setup")]
    private static void CreateOrRefreshSetupMenu()
    {
        CreateOrRefreshSetup();
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory("Assets/Generated/FlyingEnemy");
        Directory.CreateDirectory("Assets/Prefabs");
    }

    private static void GenerateSpriteTextures()
    {
        WritePngTexture(CircleTexturePath, CreateCircleTexture(128));
        WritePngTexture(OvalTexturePath, CreateOvalTexture(128, 72));
    }

    private static void WritePngTexture(string path, Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(texture);
    }

    private static Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance <= radius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateOvalTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
        float radiusX = width * 0.42f;
        float radiusY = height * 0.28f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedX = (x - center.x) / radiusX;
                float normalizedY = (y - center.y) / radiusY;
                float ellipseValue = normalizedX * normalizedX + normalizedY * normalizedY;
                float alpha = ellipseValue <= 1f ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    private static void ConfigureSpriteImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private static void CreateOrRefreshPrefabs()
    {
        Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleTexturePath);
        Sprite ovalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OvalTexturePath);

        GameObject bulletRoot = CreateBulletPrefab(circleSprite);
        PrefabUtility.SaveAsPrefabAsset(bulletRoot, BulletPrefabPath);
        Object.DestroyImmediate(bulletRoot);

        GameObject enemyRoot = CreateFlyingEnemyPrefab(circleSprite, ovalSprite);
        PrefabUtility.SaveAsPrefabAsset(enemyRoot, FlyingEnemyPrefabPath);
        Object.DestroyImmediate(enemyRoot);

        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);
        GameObject playerRoot = CreatePlayerPrefab(circleSprite, bulletPrefab);
        PrefabUtility.SaveAsPrefabAsset(playerRoot, PlayerPrefabPath);
        Object.DestroyImmediate(playerRoot);
    }

    private static GameObject CreatePlayerPrefab(Sprite circleSprite, GameObject bulletPrefab)
    {
        GameObject root = new GameObject("Player");
        root.tag = "Player";

        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = circleSprite;
        renderer.color = new Color(0.3f, 0.78f, 1f, 1f);
        renderer.sortingOrder = 20;

        Rigidbody2D rigidbody2D = root.AddComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.freezeRotation = true;
        rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D collider2D = root.AddComponent<CircleCollider2D>();
        collider2D.radius = 0.45f;

        PlayerMovement movement = root.AddComponent<PlayerMovement>();
        movement.moveSpeed = 5f;
        movement.arenaMin = new Vector2(-12f, -7f);
        movement.arenaMax = new Vector2(12f, 7f);

        PlayerHealth playerHealth = root.AddComponent<PlayerHealth>();
        playerHealth.maxHealth = 5;
        playerHealth.invulnerabilityDuration = 0.75f;

        GameObject areaPulseVisual = new GameObject("AreaPulseVisual");
        areaPulseVisual.transform.SetParent(root.transform, false);
        areaPulseVisual.transform.localPosition = Vector3.zero;
        areaPulseVisual.transform.localScale = Vector3.one * 4.4f;

        SpriteRenderer areaRenderer = areaPulseVisual.AddComponent<SpriteRenderer>();
        areaRenderer.sprite = circleSprite;
        areaRenderer.color = new Color(0.55f, 0.9f, 1f, 0.28f);
        areaRenderer.sortingOrder = 12;
        areaRenderer.enabled = false;

        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(root.transform, false);
        firePoint.transform.localPosition = new Vector3(0.55f, 0f, 0f);

        AutoShooter autoShooter = root.AddComponent<AutoShooter>();
        autoShooter.fireRange = 7f;
        autoShooter.fireRate = 0.35f;
        autoShooter.bulletPrefab = bulletPrefab;
        autoShooter.firePoint = firePoint.transform;
        autoShooter.bulletSpeed = 9f;
        autoShooter.bulletDamage = 1;
        autoShooter.bulletLifeTime = 2f;

        AreaPulseAttack areaPulseAttack = root.AddComponent<AreaPulseAttack>();
        areaPulseAttack.damageRadius = 2.2f;
        areaPulseAttack.damage = 2;
        areaPulseAttack.cooldown = 3f;
        areaPulseAttack.effectDuration = 0.2f;
        areaPulseAttack.areaVisualTransform = areaPulseVisual.transform;
        areaPulseAttack.areaVisualRenderer = areaRenderer;
        areaPulseAttack.areaColor = new Color(0.55f, 0.9f, 1f, 0.28f);

        return root;
    }

    private static GameObject CreateBulletPrefab(Sprite circleSprite)
    {
        GameObject root = new GameObject("PlayerBullet");
        root.tag = "PlayerBullet";
        root.transform.localScale = new Vector3(0.22f, 0.22f, 1f);

        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = circleSprite;
        renderer.color = new Color(0.92f, 0.98f, 1f, 1f);
        renderer.sortingOrder = 30;

        Rigidbody2D rigidbody2D = root.AddComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider2D = root.AddComponent<CircleCollider2D>();
        collider2D.radius = 0.5f;
        collider2D.isTrigger = true;

        Bullet bullet = root.AddComponent<Bullet>();
        bullet.speed = 9f;
        bullet.damage = 1;
        bullet.lifeTime = 2f;

        return root;
    }

    private static GameObject CreateFlyingEnemyPrefab(Sprite circleSprite, Sprite ovalSprite)
    {
        GameObject root = new GameObject("FlyingEnemy");
        root.tag = "Enemy";

        Rigidbody2D rigidbody2D = root.AddComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.freezeRotation = true;
        rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider2D = root.AddComponent<CircleCollider2D>();
        collider2D.radius = 0.48f;

        EnemyHealth enemyHealth = root.AddComponent<EnemyHealth>();
        enemyHealth.maxHealth = 3;

        FlyingEnemyMovement movement = root.AddComponent<FlyingEnemyMovement>();
        movement.moveSpeed = 2f;
        movement.detectionRange = 20f;
        movement.stopDistance = 1.2f;
        movement.idleHoverSpeed = 1.8f;
        movement.idleHoverAmount = 0.45f;
        movement.chaseHoverAmount = 0.55f;
        movement.chaseHoverSpeed = 3f;
        movement.followOffsetRadius = 1.2f;
        movement.separationDistance = 1.4f;
        movement.separationStrength = 1.8f;
        movement.contactDamage = 1;
        movement.damageCooldown = 1f;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        SpriteRenderer visualRenderer = visual.AddComponent<SpriteRenderer>();
        visualRenderer.sprite = circleSprite;
        visualRenderer.color = new Color(0.8f, 1f, 0.39f, 1f);
        visualRenderer.sortingOrder = 10;

        FlyingVisualEffect visualEffect = visual.AddComponent<FlyingVisualEffect>();
        visualEffect.visualHoverAmount = 0.18f;
        visualEffect.visualHoverSpeed = 3f;
        visualEffect.tiltAmount = 8f;
        visualEffect.tiltSpeed = 2.4f;
        visualEffect.pulseAmount = 0.05f;
        visualEffect.pulseSpeed = 2f;

        CreateShellSegment(visual.transform, ovalSprite, 0f);
        CreateShellSegment(visual.transform, ovalSprite, 45f);
        CreateShellSegment(visual.transform, ovalSprite, 90f);
        CreateShellSegment(visual.transform, ovalSprite, 135f);
        CreateShellSegment(visual.transform, ovalSprite, 180f);
        CreateShellSegment(visual.transform, ovalSprite, 225f);
        CreateShellSegment(visual.transform, ovalSprite, 270f);
        CreateShellSegment(visual.transform, ovalSprite, 315f);

        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(root.transform, false);
        shadow.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        shadow.transform.localScale = new Vector3(0.9f, 0.35f, 1f);

        SpriteRenderer shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = ovalSprite;
        shadowRenderer.color = new Color(0f, 0f, 0f, 0.28f);
        shadowRenderer.sortingOrder = 2;

        FlyingShadowEffect shadowEffect = shadow.AddComponent<FlyingShadowEffect>();
        shadowEffect.visualEffect = visualEffect;
        shadowEffect.shadowScaleAmount = 0.12f;
        shadowEffect.shadowAlpha = 0.28f;

        return root;
    }

    private static void CreateShellSegment(Transform parent, Sprite ovalSprite, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector3 localPosition = new Vector3(Mathf.Cos(radians) * 0.62f, Mathf.Sin(radians) * 0.62f, 0f);

        GameObject segment = new GameObject("ShellSegment");
        segment.transform.SetParent(parent, false);
        segment.transform.localPosition = localPosition;
        segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);
        segment.transform.localScale = new Vector3(0.18f, 0.34f, 1f);

        SpriteRenderer segmentRenderer = segment.AddComponent<SpriteRenderer>();
        segmentRenderer.sprite = ovalSprite;
        segmentRenderer.color = new Color(0.35f, 0.64f, 0.12f, 1f);
        segmentRenderer.sortingOrder = 9;
    }

    private static void CreateOrRefreshScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        RemoveGeneratedObjects();

        Camera mainCamera = EnsureCamera();
        Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleTexturePath);
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject background = CreateBackground(circleSprite);
        background.name = "Background";

        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        GameObject playerObject = PrefabUtility.InstantiatePrefab(playerPrefab, scene) as GameObject;
        if (playerObject != null)
        {
            playerObject.name = "Player";
            playerObject.transform.position = Vector3.zero;
        }

        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        }

        if (playerObject != null)
        {
            cameraFollow.target = playerObject.transform;
        }

        cameraFollow.worldMin = new Vector2(-12f, -7f);
        cameraFollow.worldMax = new Vector2(12f, 7f);
        cameraFollow.smoothSpeed = 6f;

        GameObject spawnPointsParent = new GameObject("SpawnPoints");
        Transform[] spawnPoints = CreateSpawnPoints(spawnPointsParent.transform);

        GameObject gameManagerObject = new GameObject("GameManager");
        GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

        GameObject levelManagerObject = new GameObject("LevelManager");
        LevelManager levelManager = levelManagerObject.AddComponent<LevelManager>();
        levelManager.flyingEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FlyingEnemyPrefabPath);
        levelManager.spawnPoints = spawnPoints;
        levelManager.timeBetweenLevels = 2f;
        levelManager.baseEnemyCount = 3;
        levelManager.enemyIncreasePerLevel = 2;
        levelManager.baseEnemyHealth = 3;
        levelManager.baseEnemyMoveSpeed = 2f;
        levelManager.enemyMoveSpeedIncreasePerLevel = 0.15f;
        levelManager.randomSpawnMin = new Vector2(-12f, -7f);
        levelManager.randomSpawnMax = new Vector2(12f, 7f);
        levelManager.minSpawnDistanceFromPlayer = 4f;

        GameObject canvasObject = CreateCanvas(defaultFont, out UIManager uiManager);

        gameManager.uiManager = uiManager;
        gameManager.levelManager = levelManager;
        if (playerObject != null)
        {
            gameManager.playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static Camera EnsureCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5.5f;
            mainCamera.backgroundColor = new Color(0.36f, 0.09f, 0.13f, 1f);
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        }

        return mainCamera;
    }

    private static GameObject CreateBackground(Sprite circleSprite)
    {
        GameObject background = new GameObject("Background");
        CreateBackgroundSprite(background.transform, "Field", circleSprite, new Vector3(0f, 0f, 5f), new Vector3(30f, 20f, 1f), new Color(0.52f, 0.08f, 0.12f, 1f), -10);
        CreateBackgroundSprite(background.transform, "GlowA", circleSprite, new Vector3(-5.5f, 3.2f, 4f), new Vector3(10f, 7f, 1f), new Color(0.76f, 0.25f, 0.33f, 0.26f), -9);
        CreateBackgroundSprite(background.transform, "GlowB", circleSprite, new Vector3(6f, -2.8f, 4f), new Vector3(12f, 8f, 1f), new Color(0.92f, 0.4f, 0.46f, 0.18f), -9);
        CreateBackgroundSprite(background.transform, "CellA", circleSprite, new Vector3(-1.5f, -1.5f, 3f), new Vector3(4.5f, 4.5f, 1f), new Color(1f, 0.65f, 0.72f, 0.08f), -8);
        CreateBackgroundSprite(background.transform, "CellB", circleSprite, new Vector3(4.5f, 1.2f, 3f), new Vector3(3.5f, 3.5f, 1f), new Color(1f, 0.72f, 0.78f, 0.08f), -8);
        return background;
    }

    private static void CreateBackgroundSprite(Transform parent, string name, Sprite sprite, Vector3 position, Vector3 scale, Color color, int sortingOrder)
    {
        GameObject spriteObject = new GameObject(name);
        spriteObject.transform.SetParent(parent, false);
        spriteObject.transform.localPosition = position;
        spriteObject.transform.localScale = scale;

        SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private static GameObject CreateCanvas(Font font, out UIManager uiManager)
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject uiManagerObject = new GameObject("UIManager");
        uiManagerObject.transform.SetParent(canvasObject.transform, false);
        uiManager = uiManagerObject.AddComponent<UIManager>();

        uiManager.healthText = CreateUIText(canvasObject.transform, "HealthText", font, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(130f, -40f), new Vector2(260f, 50f), "Health: 5/5", TextAnchor.MiddleLeft, 28);
        uiManager.levelText = CreateUIText(canvasObject.transform, "LevelText", font, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-130f, -40f), new Vector2(240f, 50f), "Level: 1", TextAnchor.MiddleRight, 28);
        uiManager.enemiesRemainingText = CreateUIText(canvasObject.transform, "EnemiesRemainingText", font, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(420f, 50f), "Enemies Remaining: 0", TextAnchor.MiddleCenter, 28);
        uiManager.levelCompleteText = CreateUIText(canvasObject.transform, "LevelCompleteText", font, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(500f, 80f), "Level Complete", TextAnchor.MiddleCenter, 42);
        uiManager.gameOverText = CreateUIText(canvasObject.transform, "GameOverText", font, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(500f, 80f), "Game Over", TextAnchor.MiddleCenter, 48);
        uiManager.restartText = CreateUIText(canvasObject.transform, "RestartText", font, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(500f, 60f), "Press R to Restart", TextAnchor.MiddleCenter, 28);

        return canvasObject;
    }

    private static Text CreateUIText(Transform parent, string objectName, Font font, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, string textValue, TextAnchor alignment, int fontSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = textValue;

        return text;
    }

    private static Transform[] CreateSpawnPoints(Transform parent)
    {
        Transform[] spawnPoints = new Transform[DefaultSpawnPositions.Length];

        for (int i = 0; i < DefaultSpawnPositions.Length; i++)
        {
            GameObject spawnPoint = new GameObject("SpawnPoint_" + (i + 1));
            spawnPoint.transform.SetParent(parent, false);
            spawnPoint.transform.position = DefaultSpawnPositions[i];
            spawnPoints[i] = spawnPoint.transform;
        }

        return spawnPoints;
    }

    private static void RemoveGeneratedObjects()
    {
        string[] objectNames =
        {
            "Background",
            "FlyingEnemyTestBackground",
            "PlayerDummy",
            "Player",
            "GameManager",
            "LevelManager",
            "Canvas",
            "SpawnPoints",
            "FlyingEnemySpawner"
        };

        for (int i = 0; i < objectNames.Length; i++)
        {
            GameObject existing = GameObject.Find(objectNames[i]);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        FlyingEnemyMovement[] existingEnemies = Object.FindObjectsByType<FlyingEnemyMovement>(FindObjectsSortMode.None);
        for (int i = 0; i < existingEnemies.Length; i++)
        {
            if (existingEnemies[i] != null)
            {
                Object.DestroyImmediate(existingEnemies[i].gameObject);
            }
        }
    }
}
