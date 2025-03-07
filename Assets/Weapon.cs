using UnityEngine;
using System.Collections.Generic;

public abstract class Weapon : MonoBehaviour
{
    [Header("Основные настройки")]
    public string weaponName;
    public Sprite weaponIcon;
    [TextArea] public string description;
    
    [Header("Характеристики")]
    public int damage = 10;
    public float attackSpeed = 1f; // Атаки в секунду
    public float attackRange = 5f;
    
    [Header("Параметры улучшения")]
    public int level = 1;
    public int maxLevel = 5;
    public float damageMultiplierPerLevel = 0.2f; // +20% урона за уровень
    public float attackSpeedMultiplierPerLevel = 0.1f; // +10% скорости за уровень
    public float attackRangeMultiplierPerLevel = 0.1f; // +10% дальности за уровень
    
    [Header("Эффекты")]
    public GameObject attackEffect;
    public AudioSource attackSound;
    
    protected PlayerController playerController;
    protected float attackTimer;
    protected int baseDamage;
    protected float baseAttackSpeed;
    protected float baseAttackRange;
    
    public virtual void Initialize(PlayerController player)
    {
        playerController = player;
        attackTimer = 0f;
        
        // Сохраняем базовые значения
        baseDamage = damage;
        baseAttackSpeed = attackSpeed;
        baseAttackRange = attackRange;
    }
    
    protected virtual void Update()
    {
        attackTimer -= Time.deltaTime;
        
        if (attackTimer <= 0)
        {
            attackTimer = 1f / attackSpeed;
            Attack();
        }
    }
    
    // Абстрактный метод атаки, который должен быть реализован каждым типом оружия
    protected abstract void Attack();
    
    // Метод для визуализации атаки
    protected virtual void ShowAttackEffect(Vector3 position)
    {
        if (attackEffect != null)
        {
            Instantiate(attackEffect, position, Quaternion.identity);
        }
        
        if (attackSound != null)
        {
            attackSound.Play();
        }
    }
    
    // Метод для нанесения урона врагу
    protected virtual void DealDamage(Enemy enemy)
    {
        enemy.TakeDamage(damage);
    }
    
    // Метод для получения ближайшего врага
    protected virtual Enemy GetNearestEnemy()
    {
        List<Enemy> enemies = playerController.GetNearbyEnemies(attackRange);
        
        if (enemies.Count == 0)
            return null;
            
        Enemy nearestEnemy = null;
        float minDistance = float.MaxValue;
        
        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }
        
        return nearestEnemy;
    }
    
    // Метод для выбора случайных врагов
    protected virtual List<Enemy> GetRandomEnemies(int count)
    {
        List<Enemy> enemies = playerController.GetNearbyEnemies(attackRange);
        List<Enemy> selectedEnemies = new List<Enemy>();
        
        if (enemies.Count == 0)
            return selectedEnemies;
            
        // Перемешиваем список
        for (int i = 0; i < enemies.Count; i++)
        {
            int randomIndex = Random.Range(i, enemies.Count);
            Enemy temp = enemies[i];
            enemies[i] = enemies[randomIndex];
            enemies[randomIndex] = temp;
        }
        
        // Выбираем нужное количество или все доступные
        int enemyCount = Mathf.Min(count, enemies.Count);
        for (int i = 0; i < enemyCount; i++)
        {
            selectedEnemies.Add(enemies[i]);
        }
        
        return selectedEnemies;
    }
    
    // Метод для улучшения оружия
    public virtual void LevelUp()
    {
        if (level >= maxLevel)
            return;
            
        level++;
        
        // Увеличиваем характеристики согласно множителям
        damage = Mathf.RoundToInt(baseDamage * (1 + (level - 1) * damageMultiplierPerLevel));
        attackSpeed = baseAttackSpeed * (1 + (level - 1) * attackSpeedMultiplierPerLevel);
        attackRange = baseAttackRange * (1 + (level - 1) * attackRangeMultiplierPerLevel);
    }
    
    // Метод для получения информации об улучшении
    public virtual string GetUpgradeDescription()
    {
        if (level >= maxLevel)
            return "Максимальный уровень достигнут";
            
        string description = $"Улучшить {weaponName} до уровня {level + 1}:\n";
        description += $"• Урон: {damage} → {Mathf.RoundToInt(baseDamage * (1 + level * damageMultiplierPerLevel))}\n";
        description += $"• Скорость атаки: {attackSpeed:F1} → {baseAttackSpeed * (1 + level * attackSpeedMultiplierPerLevel):F1}\n";
        description += $"• Дальность атаки: {attackRange:F1} → {baseAttackRange * (1 + level * attackRangeMultiplierPerLevel):F1}";
        
        return description;
    }
}

// Пример класса для ближней атаки
public class MeleeWeapon : Weapon
{
    public float attackArc = 90f; // Угол атаки в градусах
    
    protected override void Attack()
    {
        List<Enemy> enemies = playerController.GetNearbyEnemies(attackRange);
        
        if (enemies.Count == 0)
            return;
            
        Vector3 attackPosition = transform.position + transform.forward * (attackRange / 2);
        ShowAttackEffect(attackPosition);
        
        foreach (Enemy enemy in enemies)
        {
            // Проверяем, находится ли враг в секторе атаки
            Vector3 directionToEnemy = enemy.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionToEnemy);
            
            if (angle <= attackArc / 2)
            {
                DealDamage(enemy);
            }
        }
    }
}

// Пример класса для дальней атаки
public class ProjectileWeapon : Weapon
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public int projectilesPerAttack = 1;
    public float spreadAngle = 15f;
    
    protected override void Attack()
    {
        Enemy targetEnemy = GetNearestEnemy();
        
        if (targetEnemy == null)
            return;
            
        ShowAttackEffect(transform.position);
        
        // Вычисляем направление к врагу
        Vector3 directionToEnemy = (targetEnemy.transform.position - transform.position).normalized;
        
        // Создаем нужное количество снарядов
        for (int i = 0; i < projectilesPerAttack; i++)
        {
            // Добавляем разброс для нескольких снарядов
            Quaternion spreadRotation = Quaternion.Euler(0, 0, Random.Range(-spreadAngle, spreadAngle));
            Vector3 spreadDirection = spreadRotation * directionToEnemy;
            
            // Создаем снаряд
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.LookRotation(Vector3.forward, spreadDirection));
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            
            if (projectileComponent != null)
            {
                projectileComponent.Initialize(damage, spreadDirection, projectileSpeed, attackRange / projectileSpeed);
            }
        }
    }
}

// Пример класса для AOE (Area of Effect) атаки
public class AOEWeapon : Weapon
{
    public float explosionRadius = 3f;
    
    protected override void Attack()
    {
        List<Enemy> enemies = playerController.GetNearbyEnemies(attackRange);
        
        if (enemies.Count == 0)
            return;
            
        // Выбираем случайного врага для центра взрыва
        Enemy targetEnemy = enemies[Random.Range(0, enemies.Count)];
        Vector3 explosionPosition = targetEnemy.transform.position;
        
        ShowAttackEffect(explosionPosition);
        
        // Находим всех врагов в радиусе взрыва
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius);
        
        foreach (Collider2D hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                DealDamage(enemy);
            }
        }
    }
}