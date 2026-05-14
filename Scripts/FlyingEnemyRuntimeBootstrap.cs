using UnityEngine;

public class FlyingEnemyRuntimeBootstrap : MonoBehaviour
{
    private static FlyingEnemyRuntimeBootstrap instance;
    private static readonly Vector2[] DefaultSpawnPositions =
    {
        new Vector2(-6f, 3f),
        new Vector2(-5f, -3f),
        new Vector2(5f, 3.2f),
        new Vector2(6f, -2.8f)
    };

    private Sprite circleSprite;
    private Sprite shadowSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateBootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject("FlyingEnemyRuntimeBootstrap");
        instance = bootstrapObject.AddComponent<FlyingEnemyRuntimeBootstrap>();
    }

    private void Start()
    {
        if (FindFirstObjectByType<LevelManager>() != null ||
            FindFirstObjectByType<GameManager>() != null ||
            FindFirstObjectByType<PlayerMovement>() != null)
        {
            return;
        }

        BuildFallbackScene();
    }

    private void BuildFallbackScene()
    {
        CacheSprites();
        EnsureCamera();
        CreateBackground();

        Transform playerTarget = FindOrCreatePlayerTarget();
        EnsureSpawner(playerTarget);
        AssignPlayerToExistingEnemies(playerTarget);
    }

    private void EnsureCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        if (mainCamera == null)
        {
            return;
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5.5f;
        mainCamera.backgroundColor = new Color(0.36f, 0.09f, 0.13f, 1f);
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private Transform FindOrCreatePlayerTarget()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            return playerObject.transform;
        }

        GameObject playerDummy = new GameObject("PlayerDummy");
        playerDummy.tag = "Player";
        playerDummy.transform.position = new Vector3(2.5f, 0f, 0f);
        playerDummy.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        SpriteRenderer playerRenderer = playerDummy.AddComponent<SpriteRenderer>();
        playerRenderer.sprite = circleSprite;
        playerRenderer.color = new Color(0.84f, 0.95f, 1f, 1f);
        playerRenderer.sortingOrder = 8;

        Rigidbody2D rigidbody2D = playerDummy.AddComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.freezeRotation = true;
        rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D collider2D = playerDummy.AddComponent<CircleCollider2D>();
        collider2D.radius = 0.45f;
        playerDummy.AddComponent<PlayerDummyMovement>().moveSpeed = 5f;

        return playerDummy.transform;
    }

    private GameObject CreateFlyingEnemy(Transform playerTarget)
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
        enemyHealth.maxHealth = 4;

        FlyingEnemyMovement movement = root.AddComponent<FlyingEnemyMovement>();
        movement.playerTarget = playerTarget;
        movement.moveSpeed = 3f;
        movement.detectionRange = 20f;
        movement.stopDistance = 1.75f;
        movement.idleHoverSpeed = 1.8f;
        movement.idleHoverAmount = 0.45f;
        movement.chaseHoverAmount = 0.55f;
        movement.chaseHoverSpeed = 3f;
        movement.followOffsetRadius = 1.2f;
        movement.separationDistance = 1.4f;
        movement.separationStrength = 1.8f;
        movement.contactDamage = 1;
        movement.damageCooldown = 0.75f;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
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

        CreateShellSegment(visual.transform, 0f);
        CreateShellSegment(visual.transform, 45f);
        CreateShellSegment(visual.transform, 90f);
        CreateShellSegment(visual.transform, 135f);
        CreateShellSegment(visual.transform, 180f);
        CreateShellSegment(visual.transform, 225f);
        CreateShellSegment(visual.transform, 270f);
        CreateShellSegment(visual.transform, 315f);

        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(root.transform, false);
        shadow.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        shadow.transform.localScale = new Vector3(0.9f, 0.35f, 1f);

        SpriteRenderer shadowRenderer = shadow.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = shadowSprite;
        shadowRenderer.color = new Color(0f, 0f, 0f, 0.28f);
        shadowRenderer.sortingOrder = 2;

        FlyingShadowEffect shadowEffect = shadow.AddComponent<FlyingShadowEffect>();
        shadowEffect.visualEffect = visualEffect;
        shadowEffect.shadowScaleAmount = 0.12f;
        shadowEffect.shadowAlpha = 0.28f;

        return root;
    }

    private void EnsureSpawner(Transform playerTarget)
    {
        FlyingEnemySpawner existingSpawner = FindFirstObjectByType<FlyingEnemySpawner>();
        if (existingSpawner != null)
        {
            if (existingSpawner.playerTarget == null)
            {
                existingSpawner.playerTarget = playerTarget;
            }

            if (existingSpawner.flyingEnemyPrefab == null)
            {
                existingSpawner.flyingEnemyPrefab = CreateRuntimeEnemyTemplate(playerTarget);
            }

            existingSpawner.StartSpawning();
            return;
        }

        GameObject spawnerObject = new GameObject("FlyingEnemySpawner");
        FlyingEnemySpawner spawner = spawnerObject.AddComponent<FlyingEnemySpawner>();
        spawner.playerTarget = playerTarget;
        spawner.spawnInterval = 1f;
        spawner.maxAliveEnemies = 0;
        spawner.spawnPoints = CreateSpawnPoints(spawnerObject.transform);
        spawner.flyingEnemyPrefab = CreateRuntimeEnemyTemplate(playerTarget);
        spawner.StartSpawning();
    }

    private GameObject CreateRuntimeEnemyTemplate(Transform playerTarget)
    {
        GameObject template = CreateFlyingEnemy(playerTarget);
        template.name = "FlyingEnemyRuntimeTemplate";
        template.SetActive(false);
        template.transform.SetParent(transform, false);
        return template;
    }

    private Transform[] CreateSpawnPoints(Transform parent)
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

    private void AssignPlayerToExistingEnemies(Transform playerTarget)
    {
        FlyingEnemyMovement[] existingEnemies = FindObjectsByType<FlyingEnemyMovement>(FindObjectsSortMode.None);
        for (int i = 0; i < existingEnemies.Length; i++)
        {
            if (existingEnemies[i] != null && existingEnemies[i].playerTarget == null)
            {
                existingEnemies[i].playerTarget = playerTarget;
            }
        }
    }

    private void CreateShellSegment(Transform parent, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector3 localPosition = new Vector3(Mathf.Cos(radians) * 0.62f, Mathf.Sin(radians) * 0.62f, 0f);

        GameObject segment = new GameObject("ShellSegment");
        segment.transform.SetParent(parent, false);
        segment.transform.localPosition = localPosition;
        segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);
        segment.transform.localScale = new Vector3(0.18f, 0.34f, 1f);

        SpriteRenderer segmentRenderer = segment.AddComponent<SpriteRenderer>();
        segmentRenderer.sprite = shadowSprite;
        segmentRenderer.color = new Color(0.35f, 0.64f, 0.12f, 1f);
        segmentRenderer.sortingOrder = 9;
    }

    private void CreateBackground()
    {
        if (GameObject.Find("FlyingEnemyTestBackground") != null)
        {
            return;
        }

        GameObject background = new GameObject("FlyingEnemyTestBackground");
        CreateBackgroundSprite(background.transform, "Field", new Vector3(0f, 0f, 5f), new Vector3(30f, 20f, 1f), new Color(0.52f, 0.08f, 0.12f, 1f), -10);
        CreateBackgroundSprite(background.transform, "GlowA", new Vector3(-4f, 2f, 4f), new Vector3(8f, 5f, 1f), new Color(0.76f, 0.25f, 0.33f, 0.28f), -9);
        CreateBackgroundSprite(background.transform, "GlowB", new Vector3(5f, -2f, 4f), new Vector3(10f, 6f, 1f), new Color(0.92f, 0.4f, 0.46f, 0.2f), -9);
    }

    private void CreateBackgroundSprite(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, Color color, int sortingOrder)
    {
        GameObject spriteObject = new GameObject(objectName);
        spriteObject.transform.SetParent(parent, false);
        spriteObject.transform.localPosition = localPosition;
        spriteObject.transform.localScale = localScale;

        SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = circleSprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private void CacheSprites()
    {
        if (circleSprite == null)
        {
            circleSprite = CreateCircleSprite(128);
        }

        if (shadowSprite == null)
        {
            shadowSprite = CreateOvalSprite(128, 72);
        }
    }

    private Sprite CreateCircleSprite(int size)
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
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private Sprite CreateOvalSprite(int width, int height)
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
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
    }
}
