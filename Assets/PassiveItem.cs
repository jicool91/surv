using UnityEngine;

public abstract class PassiveItem : MonoBehaviour
{
    [Header("Основные настройки")]
    public string itemName;
    public Sprite itemIcon;
    [TextArea] public string description;
    
    [Header("Характеристики")]
    public int level = 1;
    public int maxLevel = 5;
    public float speedBonus = 0f;
    public float healthBonus = 0f;
    
    [Header("Параметры улучшения")]
    public float speedBonusPerLevel = 0.1f; // +0.1 к скорости за уровень
    public float healthBonusPerLevel = 5f; // +5 к здоровью за уровень
    
    protected PlayerController playerController;
    protected float baseSpeedBonus;
    protected float baseHealthBonus;
    
    public virtual void Initialize(PlayerController player)
    {
        playerController = player;
        
        // Сохраняем базовые значения
        baseSpeedBonus = speedBonus;
        baseHealthBonus = healthBonus;
        
        // Применяем начальные бонусы
        ApplyItemEffects();
    }
    
    // Метод для применения эффектов предмета
    protected virtual void ApplyItemEffects()
    {
        // Базовая реализация применяет только бонусы к скорости и здоровью
        // Дочерние классы могут переопределить этот метод для применения дополнительных эффектов
    }
    
    // Метод для улучшения предмета
    public virtual void LevelUp()
    {
        if (level >= maxLevel)
            return;
            
        level++;
        
        // Увеличиваем характеристики согласно бонусам за уровень
        speedBonus = baseSpeedBonus + (level - 1) * speedBonusPerLevel;
        healthBonus = baseHealthBonus + (level - 1) * healthBonusPerLevel;
        
        // Применяем обновленные эффекты
        ApplyItemEffects();
    }
    
    // Метод для получения информации об улучшении
    public virtual string GetUpgradeDescription()
    {
        if (level >= maxLevel)
            return "Максимальный уровень достигнут";
            
        string description = $"Улучшить {itemName} до уровня {level + 1}:\n";
        
        if (speedBonusPerLevel > 0)
        {
            description += $"• Бонус к скорости: {speedBonus:F1} → {baseSpeedBonus + level * speedBonusPerLevel:F1}\n";
        }
        
        if (healthBonusPerLevel > 0)
        {
            description += $"• Бонус к здоровью: {healthBonus:F0} → {baseHealthBonus + level * healthBonusPerLevel:F0}\n";
        }
        
        return description;
    }
}

// Пример класса для амулета скорости
public class SpeedAmulet : PassiveItem
{
    public override void Initialize(PlayerController player)
    {
        // Устанавливаем базовые значения
        baseSpeedBonus = 0.5f;
        baseHealthBonus = 0f;
        speedBonusPerLevel = 0.3f;
        
        // Вызываем базовую инициализацию
        base.Initialize(player);
    }
}

// Пример класса для броши здоровья
public class HealthBrooch : PassiveItem
{
    public override void Initialize(PlayerController player)
    {
        // Устанавливаем базовые значения
        baseSpeedBonus = 0f;
        baseHealthBonus = 10f;
        healthBonusPerLevel = 7f;
        
        // Вызываем базовую инициализацию
        base.Initialize(player);
    }
}

// Пример класса для кольца магнита, притягивающего опыт
public class MagnetRing : PassiveItem
{
    public float pickupRadius = 2f;
    public float pickupRadiusPerLevel = 0.5f;
    private float basePickupRadius;
    
    public override void Initialize(PlayerController player)
    {
        // Устанавливаем базовые значения
        baseSpeedBonus = 0f;
        baseHealthBonus = 0f;
        basePickupRadius = pickupRadius;
        
        // Вызываем базовую инициализацию
        base.Initialize(player);
    }
    
    protected override void ApplyItemEffects()
    {
        base.ApplyItemEffects();
        
        // Обновляем радиус подбора
        pickupRadius = basePickupRadius + (level - 1) * pickupRadiusPerLevel;
    }
    
    public override string GetUpgradeDescription()
    {
        string baseDescription = base.GetUpgradeDescription();
        
        if (level >= maxLevel)
            return baseDescription;
            
        return baseDescription + $"• Радиус подбора: {pickupRadius:F1} → {basePickupRadius + level * pickupRadiusPerLevel:F1}";
    }
    
    private void Update()
    {
        if (playerController == null)
            return;
            
        // Находим все объекты опыта поблизости
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        
        foreach (Collider2D collider in colliders)
        {
            ExperienceOrb expOrb = collider.GetComponent<ExperienceOrb>();
            if (expOrb != null)
            {
                // Притягиваем опыт к игроку
                expOrb.MoveTowards(playerController.transform.position, 5f);
            }
        }
    }
}