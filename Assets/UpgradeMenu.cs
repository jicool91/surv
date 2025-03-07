using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeMenu : MonoBehaviour
{
    [Header("Настройки меню")]
    public int upgradeOptionsCount = 3;
    
    [Header("Компоненты")]
    public UpgradeOption[] upgradeOptions;
    public Button closeButton;
    public AudioSource openSound;
    public AudioSource selectSound;
    
    private PlayerController player;
    
    private void Awake()
    {
        // Скрываем меню при старте
        gameObject.SetActive(false);
        
        // Добавляем обработчик для кнопки закрытия
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMenu);
        }
    }
    
    private void OnEnable()
    {
        // Воспроизводим звук открытия меню
        if (openSound != null)
        {
            openSound.Play();
        }
    }
    
    public void GenerateUpgradeOptions()
    {
        // Находим игрока
        player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player == null)
            return;
            
        // Создаем списки доступных улучшений
        List<Upgrade> availableUpgrades = new List<Upgrade>();
        
        // Добавляем улучшения для текущих оружий игрока
        foreach (Transform child in player.weaponParent)
        {
            Weapon weapon = child.GetComponent<Weapon>();
            if (weapon != null && weapon.level < weapon.maxLevel)
            {
                availableUpgrades.Add(new Upgrade 
                { 
                    upgradeType = UpgradeType.WeaponLevelUp, 
                    weaponToUpgrade = weapon,
                    icon = weapon.weaponIcon,
                    title = $"Улучшить {weapon.weaponName}",
                    description = weapon.GetUpgradeDescription()
                });
            }
        }
        
        // Добавляем улучшения для текущих пассивных предметов игрока
        foreach (Transform child in player.transform)
        {
            PassiveItem item = child.GetComponent<PassiveItem>();
            if (item != null && item.level < item.maxLevel)
            {
                availableUpgrades.Add(new Upgrade 
                { 
                    upgradeType = UpgradeType.PassiveItemLevelUp, 
                    passiveItemToUpgrade = item,
                    icon = item.itemIcon,
                    title = $"Улучшить {item.itemName}",
                    description = item.GetUpgradeDescription()
                });
            }
        }
        
        // Добавляем новые оружия
        foreach (Weapon weapon in GameManager.instance.availableWeapons)
        {
            // Проверяем, есть ли уже такое оружие у игрока
            bool hasWeapon = false;
            foreach (Transform child in player.weaponParent)
            {
                Weapon playerWeapon = child.GetComponent<Weapon>();
                if (playerWeapon != null && playerWeapon.GetType() == weapon.GetType())
                {
                    hasWeapon = true;
                    break;
                }
            }
            
            // Если у игрока еще нет такого оружия, добавляем его в список
            if (!hasWeapon)
            {
                availableUpgrades.Add(new Upgrade 
                { 
                    upgradeType = UpgradeType.NewWeapon, 
                    weaponPrefab = weapon,
                    icon = weapon.weaponIcon,
                    title = $"Новое оружие: {weapon.weaponName}",
                    description = weapon.description
                });
            }
        }
        
        // Добавляем новые пассивные предметы
        foreach (PassiveItem item in GameManager.instance.availablePassiveItems)
        {
            // Проверяем, есть ли уже такой предмет у игрока
            bool hasItem = false;
            foreach (Transform child in player.transform)
            {
                PassiveItem playerItem = child.GetComponent<PassiveItem>();
                if (playerItem != null && playerItem.GetType() == item.GetType())
                {
                    hasItem = true;
                    break;
                }
            }
            
            // Если у игрока еще нет такого предмета, добавляем его в список
            if (!hasItem)
            {
                availableUpgrades.Add(new Upgrade 
                { 
                    upgradeType = UpgradeType.NewPassiveItem, 
                    passiveItemPrefab = item,
                    icon = item.itemIcon,
                    title = $"Новый предмет: {item.itemName}",
                    description = item.description
                });
            }
        }
        
        // Перемешиваем список доступных улучшений
        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            int randomIndex = Random.Range(i, availableUpgrades.Count);
            Upgrade temp = availableUpgrades[i];
            availableUpgrades[i] = availableUpgrades[randomIndex];
            availableUpgrades[randomIndex] = temp;
        }
        
        // Определяем, сколько опций покажем игроку
        int optionsToShow = Mathf.Min(upgradeOptionsCount, availableUpgrades.Count);
        
        // Если нет доступных улучшений, закрываем меню
        if (optionsToShow == 0)
        {
            CloseMenu();
            return;
        }
        
        // Устанавливаем опции улучшений
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            if (i < optionsToShow)
            {
                // Показываем опцию с улучшением
                upgradeOptions[i].gameObject.SetActive(true);
                upgradeOptions[i].SetUpgradeOption(availableUpgrades[i]);
            }
            else
            {
                // Скрываем лишние опции
                upgradeOptions[i].gameObject.SetActive(false);
            }
        }
    }
    
    public void SelectUpgrade(Upgrade upgrade)
    {
        // Воспроизводим звук выбора
        if (selectSound != null)
        {
            selectSound.Play();
        }
        
        // Применяем выбранное улучшение
        switch (upgrade.upgradeType)
        {
            case UpgradeType.WeaponLevelUp:
                upgrade.weaponToUpgrade.LevelUp();
                break;
                
            case UpgradeType.PassiveItemLevelUp:
                upgrade.passiveItemToUpgrade.LevelUp();
                break;
                
            case UpgradeType.NewWeapon:
                player.AddWeapon(upgrade.weaponPrefab);
                break;
                
            case UpgradeType.NewPassiveItem:
                player.AddPassiveItem(upgrade.passiveItemPrefab);
                break;
        }
        
        // Закрываем меню
        CloseMenu();
    }
    
    private void CloseMenu()
    {
        // Закрываем меню через GameManager
        GameManager.instance.CloseUpgradeMenu();
    }
}

[System.Serializable]
public class UpgradeOption : MonoBehaviour
{
    public Image iconImage;
    public Text titleText;
    public Text descriptionText;
    public Button selectButton;
    
    private Upgrade currentUpgrade;
    
    private void Awake()
    {
        // Добавляем обработчик для кнопки выбора
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectButtonClick);
        }
    }
    
    public void SetUpgradeOption(Upgrade upgrade)
    {
        currentUpgrade = upgrade;
        
        // Обновляем UI
        if (iconImage != null && upgrade.icon != null)
        {
            iconImage.sprite = upgrade.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }
        
        if (titleText != null)
        {
            titleText.text = upgrade.title;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = upgrade.description;
        }
    }
    
    private void OnSelectButtonClick()
    {
        // Вызываем метод выбора улучшения
        UpgradeMenu upgradeMenu = GetComponentInParent<UpgradeMenu>();
        if (upgradeMenu != null && currentUpgrade != null)
        {
            upgradeMenu.SelectUpgrade(currentUpgrade);
        }
    }
}

public enum UpgradeType
{
    WeaponLevelUp,
    PassiveItemLevelUp,
    NewWeapon,
    NewPassiveItem
}

public class Upgrade
{
    public UpgradeType upgradeType;
    public Sprite icon;
    public string title;
    public string description;
    
    // Для улучшения существующего оружия
    public Weapon weaponToUpgrade;
    
    // Для улучшения существующего пассивного предмета
    public PassiveItem passiveItemToUpgrade;
    
    // Для получения нового оружия
    public Weapon weaponPrefab;
    
    // Для получения нового пассивного предмета
    public PassiveItem passiveItemPrefab;
}