using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class SchoolStage : BaseStage {
    [Header("Stage Settings")]
    [SerializeField] private GameObject playerPrefab;       
    [SerializeField] private Camera mainCamera;               
    [SerializeField] private Color backgroundColor = Color.black;

    [Header("Water Attack Settings")]
    [SerializeField] private GameObject waterWarningPrefab;
    [SerializeField] private GameObject waterLaserPrefab;
    [SerializeField] private float waterWarningDuration = 1.5f;
    [SerializeField] private float waterAttackCooldown = 3f;
    
    [Header("Text Attack Settings")]
    [SerializeField] private GameObject[] textProjectilePrefab;
    [SerializeField] private float textMoveSpeed = 5f;
    [SerializeField] private float textAttackCooldown = 2f;

    [Header("Pattern Settings")]
    [SerializeField] private float difficultyIncreaseTime = 30f;
    [SerializeField] private float maxDifficulty = 3f;

    private List<GameObject> activeWaterWarnings = new List<GameObject>();
    private List<GameObject> activeWaterLasers = new List<GameObject>();
    private List<GameObject> activeTextProjectiles = new List<GameObject>();

    private float waterAttackTimer = 0f;
    private float textAttackTimer = 0f;
    private float stageTimer = 0f;
    private float currentDifficulty = 1f;

    private GameObject player;
    private Vector2 screenBounds;
    private float playerRadius = 0.5f; // 플레이어 콜라이더 반경

    public void Awake() {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                
                mainCamera.orthographic = true;            
                mainCamera.orthographicSize = 5f;           
                mainCamera.transform.position = new Vector3(0, 0, -10f); 
            }
        }
        
        mainCamera.backgroundColor = backgroundColor;
        
        screenBounds = CalculateScreenBounds();
    }

    public void Start() {
        Begin();
    }
    
    private Vector2 CalculateScreenBounds()
    {
        float height = 2f * mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;
        return new Vector2(width / 2, height / 2);
    }
    
    public override void Begin() {
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        mainCamera.transform.position = new Vector3(0, 0, -10);
        
        stageTimer = 0f;
        waterAttackTimer = waterAttackCooldown;
        textAttackTimer = textAttackCooldown;
        
        Manager.SoundManager.Instance.NextSoundAdd(0, new SoundClip(clip));
        Manager.SoundManager.Instance.NextSoundForce(0);
    }
        
    public override void End() {
        foreach(var warning in activeWaterWarnings)
            Destroy(warning);
        foreach(var laser in activeWaterLasers)
            Destroy(laser);
        foreach(var projectile in activeTextProjectiles)
            Destroy(projectile);
            
        activeWaterWarnings.Clear();
        activeWaterLasers.Clear();
        activeTextProjectiles.Clear();
        
        if(player != null)
            Destroy(player);
        
        
        base.End();
    }
    
    private void Update()
    {
        if (player != null)
        {
            Vector3 playerPos = player.transform.position;
            playerPos.x = Mathf.Clamp(playerPos.x, -screenBounds.x + playerRadius, screenBounds.x - playerRadius);
            playerPos.y = Mathf.Clamp(playerPos.y, -screenBounds.y + playerRadius, screenBounds.y - playerRadius);
            player.transform.position = playerPos;
        }

        stageTimer += Time.deltaTime;
        currentDifficulty = Mathf.Min(maxDifficulty, 1f + (stageTimer / difficultyIncreaseTime));

        waterAttackTimer -= Time.deltaTime;
        if (waterAttackTimer <= 0)
        {
            waterAttackTimer = waterAttackCooldown / currentDifficulty;
            int attackCount = Mathf.FloorToInt(currentDifficulty);
            for (int i = 0; i < attackCount; i++)
            {
                CreateWaterAttack();
            }
        }

        textAttackTimer -= Time.deltaTime;
        if (textAttackTimer <= 0)
        {
            textAttackTimer = textAttackCooldown / currentDifficulty;
            int attackCount = Mathf.FloorToInt(currentDifficulty);
            for (int i = 0; i < attackCount; i++)
            {
                CreateTextAttack();
            }
        }

        CleanupProjectiles();
    }

    public void CreateWaterAttack()
    {
        bool isVertical = Random.value > 0.5f;
        Vector3 position;
    
        if (isVertical) {
            float randomX = Random.Range(-screenBounds.x, screenBounds.x);
            position = new Vector3(randomX, 0, 0);
            StartCoroutine(SpawnWaterAttack(position, false));
        }
        else
        {
            float randomY = Random.Range(-screenBounds.y, screenBounds.y);
            position = new Vector3(0, randomY, 0);
            StartCoroutine(SpawnWaterAttack(position, false));
        }
    }

    private IEnumerator SpawnWaterAttack(Vector3 position, bool isVertical)
    {
        GameObject warning = Instantiate(waterWarningPrefab, position, Quaternion.identity);
        SpriteRenderer warningRenderer = warning.GetComponent<SpriteRenderer>();
        activeWaterWarnings.Add(warning);

        float elapsedTime = 0f;
        while (elapsedTime < waterWarningDuration)
        {
            float alpha = Mathf.PingPong(elapsedTime * 4f, 0.7f) + 0.3f;
            Color warningColor = warningRenderer.color;
            warningColor.a = alpha;
            warningRenderer.color = warningColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        activeWaterWarnings.Remove(warning);
        Destroy(warning);

        GameObject laser = Instantiate(waterLaserPrefab, position, isVertical ? Quaternion.Euler(0, 0, 90) : Quaternion.identity);
        activeWaterLasers.Add(laser);
        
        yield return new WaitForSeconds(0.5f);
        
        activeWaterLasers.Remove(laser);
        Destroy(laser);
    }

    public void CreateTextAttack()
    {
        bool isHorizontal = Random.value > 0.5f;
        Vector3 spawnPosition;
        Vector3 moveDirection;

        if (isHorizontal)
        {
            spawnPosition = new Vector3(-screenBounds.x - 1f, Random.Range(-screenBounds.y, screenBounds.y), 0);
            moveDirection = Vector3.right;
        }
        else
        {
            spawnPosition = new Vector3(Random.Range(-screenBounds.x, screenBounds.x), screenBounds.y + 1f, 0);
            moveDirection = Vector3.down;
        }

        GameObject textProjectile = Instantiate(getTextProjectile(), spawnPosition, Quaternion.identity);
        TextProjectile projectileScript = textProjectile.AddComponent<TextProjectile>();
        projectileScript.Initialize(moveDirection, textMoveSpeed * currentDifficulty);
        activeTextProjectiles.Add(textProjectile);
    }

    private void CleanupProjectiles()
    {
        activeTextProjectiles.RemoveAll(projectile => projectile == null);
    }

    public void SetDifficulty(float difficulty)
    {
        currentDifficulty = Mathf.Clamp(difficulty, 1f, maxDifficulty);
    }

    public GameObject getTextProjectile()
    {
        return textProjectilePrefab[Random.Range(0, textProjectilePrefab.Length)];
    }
}