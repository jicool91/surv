using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject hitEffect;
    public AudioSource hitSound;
    
    private int damage;
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private float timer;
    
    public void Initialize(int damage, Vector3 direction, float speed, float lifetime)
    {
        this.damage = damage;
        this.direction = direction;
        this.speed = speed;
        this.lifetime = lifetime;
        timer = 0f;
        
        // Поворачиваем снаряд в направлении полета
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    private void Update()
    {
        // Передвигаем снаряд
        transform.position += direction * speed * Time.deltaTime;
        
        // Увеличиваем таймер
        timer += Time.deltaTime;
        
        // Если время жизни истекло, уничтожаем снаряд
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Если снаряд столкнулся с врагом
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Наносим урон
            enemy.TakeDamage(damage);
            
            // Воспроизводим эффект попадания
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            // Воспроизводим звук попадания
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound.clip, transform.position);
            }
            
            // Уничтожаем снаряд
            Destroy(gameObject);
        }
    }
}