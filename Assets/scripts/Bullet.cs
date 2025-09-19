using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;       // كل رصاصة تنقص 1 دم
    public float lifeTime = 2f;  // عمر الرصاصة قبل تختفي

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // جرب EnemyHealth أولاً (النظام الجديد)
        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeHit(); // كل طلقة = زيادة عداد الضربات
            Destroy(gameObject); // تدمر الرصاصة بعد الضربة
            return;
        }
        
        // إذا لم يجد EnemyHealth، جرب EnemyMov (النظام القديم)
        EnemyMov enemy = collision.GetComponent<EnemyMov>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage); // استخدم TakeDamage بدلاً من TakeHit
            Destroy(gameObject); // تدمر الرصاصة بعد الضربة
        }
    }

}
