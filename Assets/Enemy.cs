using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Основные настройки")]
    public string enemyName;
    public int maxHealth = 20;
    public int currentHealth;
    public int damage = 5;
    public float moveSpeed = 2f;
    public float attackCooldown = 1f;
    public float experienceValue = 10f;
    
    [Header("Анимация и эффекты")]
    public Animator animator;
    public GameObject hitEffect;
    public GameObject deathEffect;
    public AudioSource hitSound;
    public AudioSource deathSound;
    
    [Header("Дроп")]
    public GameObject[] possibleDrops;
    public float dropChance = 0.1f;

    protected Transform target;
    protected Rigidbody2D rb;
    protected float attackTimer;
    protected bool isAttacking = false;
    protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        // Находим игрока как цель
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        // Если нет цели или враг мертв, прекращаем обновление
        if (target == null || currentHealth <= 0)
            return;

        // Управление таймером атаки
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        
        // Обновляем анимацию
        if (animator != null)
        {
            animator.SetBool("IsAttacking", isAttacking);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (target == null || currentHealth <= 0 || isAttacking)
            return;

        // Поворачиваем спрайт в сторону игрока
        if (target.position.x < transform.position.x)
        {
            sr.flipX = true;
        }
        else
        {
            sr.flipX = false;
        }
        
        // Двигаемся к игроку
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (hitSound != null)
            hitSound.Play();
            
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            
        if (animator != null)
            animator.SetTrigger("Hit");

        // Эффект отбрасывания
        Vector2 knockbackDirection = (transform.position - target.position).normalized;
        rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
            
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (deathSound != null)
            deathSound.Play();
            
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
            
        // Даем опыт игроку
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.GainExperience(experienceValue);
        }
        
        // Шанс на дроп предмета
        DropItem();
        
        // Удаляем объект врага
        Destroy(gameObject);
    }

    protected virtual void DropItem()
    {
        if (possibleDrops.Length == 0 || Random.value > dropChance)
            return;
            
        int randomIndex = Random.Range(0, possibleDrops.Length);
        Instantiate(possibleDrops[randomIndex], transform.position, Quaternion.identity);
    }
    
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && attackTimer <= 0)
        {
            Attack(collision.gameObject);
        }
    }
    
    protected virtual void Attack(GameObject player)
    {
        // Сбрасываем таймер атаки
        attackTimer = attackCooldown;
        isAttacking = true;
        
        // Останавливаемся для атаки
        rb.linearVelocity = Vector2.zero;
        
        if (animator != null)
            animator.SetTrigger("Attack");
            
        // Наносим урон игроку
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
        }
        
        // Возвращаемся к передвижению через небольшую задержку
        Invoke("ResetAttack", 0.5f);
    }
    
    protected virtual void ResetAttack()
    {
        isAttacking = false;
    }
}