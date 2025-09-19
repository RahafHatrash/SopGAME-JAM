using System.Collections;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public enum AbilityType { None, Shooting, Sword, Freeze }

    [Header("Current Ability (اختاري يدوي أو بزر 1/2/3)")]
    public AbilityType currentAbility = AbilityType.None;

    [Header("Shooting (Cloud)")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float shootCooldown = 0.4f;
    private float shootTimer = 0f;

    [Header("Sword (Slime)")]
    public GameObject swordHitbox;
    public float swordActiveTime = 0.12f;
    public float swordCooldown = 0.6f;
    private bool swordReady = true;

    [Header("Freeze (Snow)")]
    public GameObject icePrefab;       // Prefab Particle System للفريز
    public float iceSpeed = 0.5f;
    public LayerMask freezeTargetLayer;
    public float freezeRange = 3f;
    public float freezeDuration = 2f;
    public float freezeCooldown = 1f;
    private float freezeTimer = 0f;

    void Update()
    {
        // تبديل القدرات
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentAbility = AbilityType.Shooting;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentAbility = AbilityType.Sword;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentAbility = AbilityType.Freeze;

        // مؤقتات الكولداون
        if (shootTimer > 0f) shootTimer -= Time.deltaTime;
        if (freezeTimer > 0f) freezeTimer -= Time.deltaTime;

        // زر التنفيذ
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TryUseAbility();
        }
    }

    private void TryUseAbility()
    {
        switch (currentAbility)
        {
            case AbilityType.Shooting:
                if (shootTimer <= 0f) { DoShoot(); shootTimer = shootCooldown; }
                break;

            case AbilityType.Sword:
                if (swordReady) StartCoroutine(DoSword());
                break;

            case AbilityType.Freeze:
                if (freezeTimer <= 0f) { DoFreeze(); freezeTimer = freezeCooldown; }
                break;
        }
    }

    // --------- SHOOT ----------
    private void DoShoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector3 dir3 = (mouseWorld - firePoint.position).normalized;
        Vector2 dir = new Vector2(dir3.x, dir3.y);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * bulletSpeed; // استخدم velocity وليس linearVelocity

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(bullet, 3f);
    }

    // --------- SWORD ----------
    private IEnumerator DoSword()
    {
        if (swordHitbox == null) yield break;

        swordReady = false;
        swordHitbox.SetActive(true);

        yield return new WaitForSeconds(swordActiveTime);

        swordHitbox.SetActive(false);

        yield return new WaitForSeconds(Mathf.Max(0f, swordCooldown - swordActiveTime));
        swordReady = true;
    }

    // --------- FREEZE ----------
    private void DoFreeze()
    {
        if (icePrefab == null || firePoint == null) return;

        // إنشاء Particle System عند FirePoint
        GameObject ice = Instantiate(icePrefab, firePoint.position, Quaternion.identity);

        // تحديد اتجاه نحو الماوس
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector3 direction = (mouseWorld - firePoint.position).normalized;

        // تدوير البارتكل نحو الماوس
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        ice.transform.rotation = Quaternion.Euler(0, 0, angle);

        // حركة البارتكل إذا فيه Rigidbody2D
        Rigidbody2D rb = ice.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction * iceSpeed;

        Destroy(ice, 0.5f);

        // Raycast للتحقق من الاصطدام بالأعداء (Freeze effect)
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, freezeRange, freezeTargetLayer);
        if (hit.collider != null)
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
                StartCoroutine(FreezeEnemy(enemy));
        }
    }

    private IEnumerator FreezeEnemy(EnemyHealth enemy)
    {
        if (enemy == null) yield break;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();

        Color originalColor = Color.white;
        if (sr != null) originalColor = sr.color;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        enemy.enabled = false;
        if (sr != null) sr.color = Color.cyan;

        yield return new WaitForSeconds(freezeDuration);

        enemy.enabled = true;
        if (sr != null) sr.color = originalColor;
    }
}
