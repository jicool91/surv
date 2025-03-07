using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("Игровые настройки")]
    public float gameTime = 0f;
    public float gameTimeLimit = 1800f; // 30 минут
    public int score = 0;
    public bool gameOver = false;
    public bool gamePaused = false;
    
    [Header("Волны врагов")]
    public EnemyWave[] enemyWaves;
    public float timeBetweenWaves = 60f;
    public float bossSpawnTime = 300f; // Босс каждые 5 минут
    private float nextWaveTime = 0f;
    private float nextBossTime = 0f;
    private int currentWaveIndex = 0;
    
    [Header("Персонаж")]
    public PlayerController playerPrefab;
    public Transform playerSpawnPoint;
    public Weapon startingWeapon;
    
    [Header("Улучшения")]
    public List<Weapon> availableWeapons = new List<Weapon>();
    public List<PassiveItem> availablePassiveItems = new List<PassiveItem>();
    public UpgradeMenu upgradeMenu;
    
    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject victoryPanel;
    public Text timerText;
    public Text scoreText;
    
    private PlayerController player;
    
    private void Awake()
    {
        // Синглтон-паттерн
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Инициализация игры
        gameOver = false;
        gamePaused = false;
        gameTime = 0f;
        score = 0;
        
        // Создаем игрока
        SpawnPlayer();
        
        // Скрываем панели
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        
        // Инициализируем время первой волны
        nextWaveTime = timeBetweenWaves;
        nextBossTime = bossSpawnTime;
        
        // Обновляем UI
        UpdateUI();
    }
    
    private void Update()
    {
        if (gameOver || gamePaused)
            return;
            
        // Обновляем время игры
        gameTime += Time.deltaTime;
        
        // Проверяем условие победы по времени
        if (gameTime >= gameTimeLimit)
        {
            Victory();
            return;
        }
        
        // Обработка волн врагов
        if (gameTime >= nextWaveTime)
        {
            SpawnEnemyWave();
            nextWaveTime = gameTime + timeBetweenWaves;
        }
        
        // Обработка появления босса
        if (gameTime >= nextBossTime)
        {
            SpawnBoss();
            nextBossTime = gameTime + bossSpawnTime;
        }
        
        // Обработка паузы
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        // Обновляем UI
        UpdateUI();
    }
    
    private void SpawnPlayer()
    {
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            player = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        }
    }
    
    private void SpawnEnemyWave()
    {
        if (enemyWaves.Length == 0)
            return;
            
        // Выбираем волну врагов
        EnemyWave wave = enemyWaves[currentWaveIndex];
        
        // Спавним врагов из волны
        wave.SpawnWave();
        
        // Увеличиваем индекс волны (с циклическим возвратом)
        currentWaveIndex = (currentWaveIndex + 1) % enemyWaves.Length;
    }
    
    private void SpawnBoss()
    {
        // Находим все боссовые волны
        List<EnemyWave> bossWaves = new List<EnemyWave>();
        
        foreach (EnemyWave wave in enemyWaves)
        {
            if (wave.isBossWave)
            {
                bossWaves.Add(wave);
            }
        }
        
        // Если нет боссовых волн, выходим
        if (bossWaves.Count == 0)
            return;
            
        // Выбираем случайную боссовую волну
        EnemyWave selectedBossWave = bossWaves[Random.Range(0, bossWaves.Count)];
        
        // Спавним босса
        selectedBossWave.SpawnWave();
    }
    
    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
    
    public void OpenUpgradeMenu()
    {
        if (upgradeMenu == null)
            return;
            
        // Приостанавливаем игру
        Time.timeScale = 0f;
        gamePaused = true;
        
        // Генерируем варианты улучшений
        upgradeMenu.GenerateUpgradeOptions();
        
        // Показываем меню
        upgradeMenu.gameObject.SetActive(true);
    }
    
    public void CloseUpgradeMenu()
    {
        if (upgradeMenu == null)
            return;
            
        // Скрываем меню
        upgradeMenu.gameObject.SetActive(false);
        
        // Возобновляем игру
        Time.timeScale = 1f;
        gamePaused = false;
    }
    
    public void GameOver()
    {
        gameOver = true;
        
        // Показываем панель завершения игры
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    public void Victory()
    {
        gameOver = true;
        
        // Показываем панель победы
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }
    
    public void TogglePause()
    {
        gamePaused = !gamePaused;
        
        if (gamePaused)
        {
            // Приостанавливаем игру
            Time.timeScale = 0f;
            
            // Показываем панель паузы
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }
        else
        {
            // Возобновляем игру
            Time.timeScale = 1f;
            
            // Скрываем панель паузы
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }
    }
    
    public void RestartGame()
    {
        // Возобновляем нормальную скорость игры
        Time.timeScale = 1f;
        
        // Перезагружаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitToMainMenu()
    {
        // Возобновляем нормальную скорость игры
        Time.timeScale = 1f;
        
        // Загружаем сцену главного меню
        SceneManager.LoadScene("MainMenu");
    }
}

[System.Serializable]
public class EnemyWave
{
    public string waveName;
    public bool isBossWave = false;
    public EnemySpawnInfo[] enemies;
    public float spawnDelay = 0.5f;
    
    public void SpawnWave()
    {
        // Запускаем спавн волны через корутину
        GameManager.instance.StartCoroutine(SpawnWaveCoroutine());
    }
    
    private System.Collections.IEnumerator SpawnWaveCoroutine()
    {
        foreach (EnemySpawnInfo spawnInfo in enemies)
        {
            // Спавним указанное количество врагов
            for (int i = 0; i < spawnInfo.count; i++)
            {
                // Определяем позицию спавна (случайно вокруг игрока)
                Vector3 spawnPosition = GetRandomSpawnPosition();
                
                // Создаем врага
                UnityEngine.Object.Instantiate(spawnInfo.enemyPrefab, spawnPosition, Quaternion.identity);
                
                // Ждем указанное время перед спавном следующего врага
                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        // Находим игрока
        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player == null)
            return Vector3.zero;
            
        // Определяем радиусы спавна
        float minDistance = 10f; // Минимальное расстояние от игрока
        float maxDistance = 15f; // Максимальное расстояние от игрока
        
        // Генерируем случайное направление
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        
        // Генерируем случайное расстояние в указанном диапазоне
        float randomDistance = Random.Range(minDistance, maxDistance);
        
        // Вычисляем позицию спавна
        Vector3 spawnPosition = player.transform.position + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;
        
        return spawnPosition;
    }
}

[System.Serializable]
public class EnemySpawnInfo
{
    public Enemy enemyPrefab;
    public int count = 1;
}