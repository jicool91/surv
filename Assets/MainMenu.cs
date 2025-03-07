using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Меню")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject charactersPanel;
    public GameObject creditsPanel;
    
    [Header("Кнопки")]
    public Button playButton;
    public Button optionsButton;
    public Button charactersButton;
    public Button creditsButton;
    public Button quitButton;
    public Button backButton;
    
    [Header("Настройки")]
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle fullscreenToggle;
    
    [Header("Персонажи")]
    public CharacterSelection[] characterSelections;
    
    [Header("Звуки")]
    public AudioSource bgmSource;
    public AudioSource buttonClickSound;
    
    private void Start()
    {
        // Показываем главное меню
        ShowMainMenu();
        
        // Добавляем обработчики для кнопок
        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClick);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsButtonClick);
        if (charactersButton != null) charactersButton.onClick.AddListener(OnCharactersButtonClick);
        if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsButtonClick);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClick);
        if (backButton != null) backButton.onClick.AddListener(ShowMainMenu);
        
        // Загружаем настройки
        LoadSettings();
        
        // Добавляем обработчики для настроек
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
    }
    
    // Методы для переключения между панелями меню
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (charactersPanel != null) charactersPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (backButton != null) backButton.gameObject.SetActive(false);
    }
    
    public void ShowOptionsMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (charactersPanel != null) charactersPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (backButton != null) backButton.gameObject.SetActive(true);
    }
    
    public void ShowCharactersMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (charactersPanel != null) charactersPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (backButton != null) backButton.gameObject.SetActive(true);
    }
    
    public void ShowCreditsMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (charactersPanel != null) charactersPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (backButton != null) backButton.gameObject.SetActive(true);
    }
    
    // Обработчики кнопок
    private void OnPlayButtonClick()
    {
        PlayButtonClickSound();
        
        // Запускаем игровую сцену
        SceneManager.LoadScene("GameScene");
    }
    
    private void OnOptionsButtonClick()
    {
        PlayButtonClickSound();
        ShowOptionsMenu();
    }
    
    private void OnCharactersButtonClick()
    {
        PlayButtonClickSound();
        ShowCharactersMenu();
    }
    
    private void OnCreditsButtonClick()
    {
        PlayButtonClickSound();
        ShowCreditsMenu();
    }
    
    private void OnQuitButtonClick()
    {
        PlayButtonClickSound();
        
        // Выход из игры
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Обработчики настроек
    private void OnBGMVolumeChanged(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
        }
        
        // Сохраняем настройки
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }
    
    private void OnSFXVolumeChanged(float volume)
    {
        // Устанавливаем громкость звуковых эффектов
        AudioSource[] allAudioSources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources)
        {
            if (source != bgmSource) // Проверяем, что это не фоновая музыка
            {
                source.volume = volume;
            }
        }
        
        // Сохраняем настройки
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
    
    private void OnFullscreenToggled(bool isFullscreen)
    {
        // Устанавливаем полноэкранный режим
        Screen.fullScreen = isFullscreen;
        
        // Сохраняем настройки
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    // Метод для загрузки настроек
    private void LoadSettings()
    {
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        
        // Применяем настройки
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = bgmVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;
        
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        Screen.fullScreen = isFullscreen;
    }
    
    // Метод для воспроизведения звука нажатия кнопки
    private void PlayButtonClickSound()
    {
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }
    }
}

[System.Serializable]
public class CharacterSelection
{
    public string characterName;
    public Sprite characterImage;
    public Button selectButton;
    public GameObject lockIcon;
    public bool isUnlocked = true;
    public int requiredScore = 0; // Счет, необходимый для разблокировки
    
    // Метод для проверки разблокировки персонажа
    public bool CheckUnlocked()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        isUnlocked = highScore >= requiredScore;
        
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }
        
        if (selectButton != null)
        {
            selectButton.interactable = isUnlocked;
        }
        
        return isUnlocked;
    }
}