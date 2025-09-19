using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
{
    EnemyHealth enemy = collision.collider.GetComponent<EnemyHealth>();
    if (enemy != null)
    {
        enemy.TakeDamage(1);
    }

    Destroy(gameObject); // الرصاصة تختفي بعد الاصطدام
}

}
