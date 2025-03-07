using UnityEngine;
using UnityEngine.UI;

// Компонент для отображения здоровья игрока
public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    public Text healthText;
    public GameObject damageFlashEffect;
    public float flashDuration = 0.2f;
    
    private int lastHealth;
    
    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
            
        if (fill == null && slider != null)
            fill = slider.fillRect.GetComponent<Image>();
    }
    
    public void SetMaxHealth(int health)
    {
        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health;
        }
        
        if (fill != null && gradient != null)
        {
            fill.color = gradient.Evaluate(1f);
        }
        
        if (healthText != null)
        {
            healthText.text = $"{health}/{health}";
        }
        
        lastHealth = health;
    }
    
    public void SetHealth(int health)
    {
        // Если здоровье уменьшилось, показываем эффект урона
        if (health < lastHealth && damageFlashEffect != null)
        {
            ShowDamageEffect();
        }
        
        if (slider != null)
        {
            slider.value = health;
        }
        
        if (fill != null && gradient != null)
        {
            fill.color = gradient.Evaluate(slider.normalizedValue);
        }
        
        if (healthText != null)
        {
            healthText.text = $"{health}/{slider.maxValue}";
        }
        
        lastHealth = health;
    }
    
    private void ShowDamageEffect()
    {
        // Активируем эффект урона
        damageFlashEffect.SetActive(true);
        
        // Отключаем эффект через указанное время
        Invoke("HideDamageEffect", flashDuration);
    }
    
    private void HideDamageEffect()
    {
        if (damageFlashEffect != null)
        {
            damageFlashEffect.SetActive(false);
        }
    }
}

// Компонент для отображения опыта игрока
public class ExperienceBar : MonoBehaviour
{
    public Slider slider;
    public Text experienceText;
    public GameObject levelUpEffect;
    
    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
    }
    
    public void SetMaxExperience(float maxExperience)
    {
        if (slider != null)
        {
            slider.maxValue = maxExperience;
        }
        
        UpdateExperienceText();
    }
    
    public void SetExperience(float experience)
    {
        if (slider != null)
        {
            // Если достигли максимума, показываем эффект повышения уровня
            if (experience >= slider.maxValue && levelUpEffect != null)
            {
                levelUpEffect.SetActive(true);
                Invoke("HideLevelUpEffect", 1f);
            }
            
            // Устанавливаем значение опыта
            slider.value = experience;
        }
        
        UpdateExperienceText();
    }
    
    private void UpdateExperienceText()
    {
        if (experienceText != null && slider != null)
        {
            experienceText.text = $"{slider.value:F0}/{slider.maxValue:F0}";
        }
    }
    
    private void HideLevelUpEffect()
    {
        if (levelUpEffect != null)
        {
            levelUpEffect.SetActive(false);
        }
    }
}

// Компонент для отображения уровня игрока
public class LevelText : MonoBehaviour
{
    public Text levelText;
    public Animator animator;
    
    private void Awake()
    {
        if (levelText == null)
            levelText = GetComponent<Text>();
            
        if (animator == null)
            animator = GetComponent<Animator>();
    }
    
    public void SetLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Уровень {level}";
        }
        
        // Проигрываем анимацию изменения уровня
        if (animator != null)
        {
            animator.SetTrigger("LevelUp");
        }
    }
}

// Компонент для отображения таймера игры
public class GameTimer : MonoBehaviour
{
    public Text timerText;
    
    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponent<Text>();
    }
    
    private void Update()
    {
        if (timerText != null && GameManager.instance != null)
        {
            float gameTime = GameManager.instance.gameTime;
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}

// Компонент для отображения количества убитых врагов или счета
public class ScoreCounter : MonoBehaviour
{
    public Text scoreText;
    public string prefix = "Счёт: ";
    
    private void Awake()
    {
        if (scoreText == null)
            scoreText = GetComponent<Text>();
    }
    
    private void Update()
    {
        if (scoreText != null && GameManager.instance != null)
        {
            scoreText.text = prefix + GameManager.instance.score.ToString();
        }
    }
}

// Компонент для кнопки меню паузы
public class PauseButton : MonoBehaviour
{
    public Button button;
    
    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
            
        if (button != null)
        {
            button.onClick.AddListener(OnPauseButtonClick);
        }
    }
    
    private void OnPauseButtonClick()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.TogglePause();
        }
    }
}

// Компонент для кнопки рестарта игры
public class RestartButton : MonoBehaviour
{
    public Button button;
    
    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
            
        if (button != null)
        {
            button.onClick.AddListener(OnRestartButtonClick);
        }
    }
    
    private void OnRestartButtonClick()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.RestartGame();
        }
    }
}

// Компонент для кнопки выхода в главное меню
public class MainMenuButton : MonoBehaviour
{
    public Button button;
    
    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
            
        if (button != null)
        {
            button.onClick.AddListener(OnMainMenuButtonClick);
        }
    }
    
    private void OnMainMenuButtonClick()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.QuitToMainMenu();
        }
    }
}