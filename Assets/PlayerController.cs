using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Основные настройки")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public int maxHealth = 100;
    public int currentHealth;
    public float experienceToNextLevel = 100f;
    public float currentExperience = 0f;
    public int playerLevel = 1;

    [Header("Компоненты")]
    public Transform weaponParent;
    public Animator animator;
    public GameObject levelUpEffect;
    public AudioSource levelUpSound;
    public AudioSource hurtSound;
    public AudioSource deathSound;

    [Header("UI")]
    public HealthBar healthBar;
    public ExperienceBar experienceBar;
    public LevelText levelText;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private List<Weapon> weapons = new List<Weapon>();
    private List<PassiveItem> passiveItems = new List<PassiveItem>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (healthBar != null) healthBar.SetMaxHealth(maxHealth);
        if (experienceBar != null) experienceBar.SetMaxExperience(experienceToNextLevel);
        if (levelText != null) levelText.SetLevel(playerLevel);
        
        // Добавляем начальное оружие
        StartingWeapon();
    }

    private void Update()
    {
        // Получаем направление движения
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        // Анимация движения
        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude);
        }

        // Поворот персонажа в направлении движения
        if (moveDirection != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        // Перемещение
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (hurtSound != null)
            hurtSound.Play();
            
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
            
        if (animator != null)
            animator.SetTrigger("Hurt");
            
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void GainExperience(float amount)
    {
        currentExperience += amount;
        
        if (experienceBar != null)
            experienceBar.SetExperience(currentExperience);
            
        // Проверка на повышение уровня
        if (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        playerLevel++;
        currentExperience -= experienceToNextLevel;
        experienceToNextLevel *= 1.2f; // Увеличиваем опыт для следующего уровня
        
        // Восстанавливаем немного здоровья при повышении уровня
        currentHealth = Mathf.Min(currentHealth + (maxHealth / 4), maxHealth);
        
        if (levelUpEffect != null)
            Instantiate(levelUpEffect, transform.position, Quaternion.identity);
            
        if (levelUpSound != null)
            levelUpSound.Play();
            
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
            
        if (experienceBar != null)
            experienceBar.SetMaxExperience(experienceToNextLevel);
            
        if (levelText != null)
            levelText.SetLevel(playerLevel);
            
        // Открываем меню выбора улучшений
        GameManager.instance.OpenUpgradeMenu();
    }

    public void AddWeapon(Weapon weaponPrefab)
    {
        // Инстанцируем оружие и привязываем к родителю
        Weapon newWeapon = Instantiate(weaponPrefab, weaponParent);
        newWeapon.Initialize(this);
        weapons.Add(newWeapon);
    }

    public void AddPassiveItem(PassiveItem itemPrefab)
    {
        // Инстанцируем пассивный предмет и добавляем его эффекты
        PassiveItem newItem = Instantiate(itemPrefab, transform);
        newItem.Initialize(this);
        passiveItems.Add(newItem);
        
        // Применяем эффекты предмета
        ApplyPassiveItemEffects();
    }

    private void ApplyPassiveItemEffects()
    {
        // Сбрасываем характеристики персонажа до базовых
        moveSpeed = 5f;
        
        // Применяем эффекты всех пассивных предметов
        foreach (PassiveItem item in passiveItems)
        {
            moveSpeed += item.speedBonus;
            maxHealth += (int)item.healthBonus;  // Явное приведение float к int
        }
        
        // Обновляем UI
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }
    }

    private void StartingWeapon()
    {
        // Получаем начальное оружие из GameManager
        Weapon startingWeapon = GameManager.instance.startingWeapon;
        if (startingWeapon != null)
        {
            AddWeapon(startingWeapon);
        }
    }

    private void Die()
    {
        if (deathSound != null)
            deathSound.Play();
            
        if (animator != null)
            animator.SetTrigger("Death");
            
        // Отключаем управление
        enabled = false;
        rb.linearVelocity = Vector2.zero;
        
        // Вызываем экран смерти через GameManager
        GameManager.instance.GameOver();
    }

    // Метод для проверки ближайших врагов (используется оружием)
    public List<Enemy> GetNearbyEnemies(float radius)
    {
        List<Enemy> nearbyEnemies = new List<Enemy>();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        
        foreach (Collider2D collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                nearbyEnemies.Add(enemy);
            }
        }
        
        return nearbyEnemies;
    }
}