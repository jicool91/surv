using UnityEngine;

public class ExperienceOrb : MonoBehaviour
{
    [Header("Настройки")]
    public float experienceValue = 1f;
    public float moveSpeed = 5f;
    public float attractionRadius = 3f;
    public float collectRadius = 0.5f;
    public float lifetime = 30f; // Время жизни орба перед исчезновением
    
    [Header("Визуальные эффекты")]
    public SpriteRenderer spriteRenderer;
    public Color smallOrbColor = Color.cyan;
    public Color mediumOrbColor = Color.blue;
    public Color largeOrbColor = Color.magenta;
    public GameObject collectEffect;
    public AudioSource collectSound;
    
    private float timer = 0f;
    private Transform targetPlayer;
    private bool isMovingToTarget = false;
    
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        // Устанавливаем цвет в зависимости от количества опыта
        if (spriteRenderer != null)
        {
            if (experienceValue <= 2f)
            {
                spriteRenderer.color = smallOrbColor;
                transform.localScale = Vector3.one * 0.8f;
            }
            else if (experienceValue <= 5f)
            {
                spriteRenderer.color = mediumOrbColor;
                transform.localScale = Vector3.one * 1f;
            }
            else
            {
                spriteRenderer.color = largeOrbColor;
                transform.localScale = Vector3.one * 1.2f;
            }
        }
    }
    
    private void Start()
    {
        // Находим игрока
        targetPlayer = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Добавляем небольшой случайный импульс при создании орба
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * 2f, ForceMode2D.Impulse);
        }
    }
    
    private void Update()
    {
        // Увеличиваем таймер
        timer += Time.deltaTime;
        
        // Если орб существует слишком долго, начинаем мигать перед исчезновением
        if (timer >= lifetime * 0.8f && spriteRenderer != null)
        {
            float alpha = Mathf.PingPong(Time.time * 5f, 1f);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
            
            // Если время жизни истекло, уничтожаем орб
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
        
        // Если игрок не найден, выходим
        if (targetPlayer == null)
            return;
            
        // Проверяем, достаточно ли близко игрок для притяжения
        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
        
        // Если орб уже движется к игроку или игрок вошел в радиус притяжения
        if (isMovingToTarget || distanceToPlayer <= attractionRadius)
        {
            isMovingToTarget = true;
            MoveTowards(targetPlayer.position, moveSpeed);
            
            // Если игрок достаточно близко, собираем орб
            if (distanceToPlayer <= collectRadius)
            {
                Collect();
            }
        }
    }
    
    // Метод для движения орба к указанной позиции
    public void MoveTowards(Vector3 position, float speed)
    {
        isMovingToTarget = true;
        
        // Вычисляем направление к цели
        Vector3 direction = (position - transform.position).normalized;
        
        // Двигаемся к цели
        transform.position += direction * speed * Time.deltaTime;
    }
    
    // Метод для сбора орба
    private void Collect()
    {
        // Находим компонент игрока
        PlayerController player = targetPlayer.GetComponent<PlayerController>();
        if (player != null)
        {
            // Даем опыт игроку
            player.GainExperience(experienceValue);
            
            // Создаем эффект сбора
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }
            
            // Воспроизводим звук сбора
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound.clip, transform.position);
            }
            
            // Уничтожаем орб
            Destroy(gameObject);
        }
    }
    
    // Метод для установки значения опыта (используется при создании)
    public void SetExperienceValue(float value)
    {
        experienceValue = value;
        
        // Обновляем визуальные эффекты
        if (spriteRenderer != null)
        {
            if (value <= 2f)
            {
                spriteRenderer.color = smallOrbColor;
                transform.localScale = Vector3.one * 0.8f;
            }
            else if (value <= 5f)
            {
                spriteRenderer.color = mediumOrbColor;
                transform.localScale = Vector3.one * 1f;
            }
            else
            {
                spriteRenderer.color = largeOrbColor;
                transform.localScale = Vector3.one * 1.2f;
            }
        }
    }
}