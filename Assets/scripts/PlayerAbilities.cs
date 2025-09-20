using System.Collections;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public enum AbilityType { None, Shooting, Sword, Freeze }
    public AbilityType currentAbility = AbilityType.None;

    [Header("Shooting")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float shootCooldown = 0.4f;
    private float shootTimer = 0f;

    [Header("Shooting Animation")]
    public bool useShootingAnimation = true;
    public Animator playerAnimator;
    public string shootTriggerName = "Shoot"; // استخدام Trigger بدلاً من Bool
    private bool canShoot = false; // التحكم في إمكانية الإطلاق

    [Header("Hitting Animation")]
    public string hittingParameterName = "isHitting"; // اسم الـ parameter في Animator
    public float hittingCooldown = 0.6f;
    private bool isHittingAnimating = false;
    private bool isHittingAttacking = false; // هل اللاعب في حالة ضرب؟
    private float hittingTimer = 0f;

    [Header("Freeze")]
    public GameObject icePrefab;
    public float iceSpeed = 12f;
    public LayerMask freezeTargetLayer;
    public float freezeRange = 15f;
    public float freezeDuration = 2f;
    public float freezeCooldown = 1f;
    private float freezeTimer = 0f;

    void Awake()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        DetectSceneAndSetAbility();
        // تأخير بسيط لضمان أن الـ Animator جاهز
        StartCoroutine(InitializeAnimator());
    }
    
    IEnumerator InitializeAnimator()
    {
        // انتظار عدة إطارات للتأكد من أن الـ Animator جاهز
        yield return new WaitForSeconds(0.2f);
        
        // التأكد من أن الـ Animator في حالة Idle
        if (playerAnimator != null)
        {
            // إعادة تشغيل الـ Animator للتأكد من الحالة الصحيحة
            playerAnimator.enabled = false;
            yield return new WaitForSeconds(0.01f);
            playerAnimator.enabled = true;
        }
        
        // تفعيل إمكانية الإطلاق بعد التأكد من أن كل شيء جاهز
        canShoot = true;
    }

    void Update()
    {
        if (shootTimer > 0f) shootTimer -= Time.deltaTime;
        if (freezeTimer > 0f) freezeTimer -= Time.deltaTime;
        if (hittingTimer > 0f) hittingTimer -= Time.deltaTime;

        // إطلاق النار فقط عند الضغط
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TryUseAbility();
        }
    }

    void DetectSceneAndSetAbility()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();

        if (sceneName.Contains("seweronfire"))
            currentAbility = AbilityType.Shooting;
        else if (sceneName.Contains("snow") || sceneName.Contains("ice"))
            currentAbility = AbilityType.Freeze;
        else if (sceneName.Contains("sky"))
            currentAbility = AbilityType.Sword;
        else
            currentAbility = AbilityType.Shooting;
    }

    void TryUseAbility()
    {
        switch (currentAbility)
        {
            case AbilityType.Shooting:
                if (shootTimer <= 0f) 
                { 
                    DoShoot(); 
                    shootTimer = shootCooldown; 
                }
                break;
            case AbilityType.Sword:
                if (hittingTimer <= 0f) 
                { 
                    StartCoroutine(DoHitting()); 
                    hittingTimer = hittingCooldown; 
                }
                break;
            case AbilityType.Freeze:
                if (freezeTimer <= 0f) 
                { 
                    DoFreeze(); 
                    freezeTimer = freezeCooldown; 
                }
                break;
        }
    }

    void DoShoot()
    {
        if (bulletPrefab == null || firePoint == null || !canShoot) return;

        if (useShootingAnimation)
        {
            StartCoroutine(ShootWithAnimation());
        }
        else
        {
            ShootBullet();
        }
    }

    void ShootBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 dir = (mouseWorld - firePoint.position).normalized;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * bulletSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(bullet, 3f);
    }

    IEnumerator ShootWithAnimation()
    {
        // شغل أنيميشن الإطلاق (Trigger - يشتغل مرة واحدة فقط)
        if (playerAnimator != null && !string.IsNullOrEmpty(shootTriggerName))
        {
            playerAnimator.SetTrigger(shootTriggerName);
        }

        ShootBullet();

        // انتظار قصير جداً - فقط لضمان أن الرصاصة تطلع
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator DoHitting()
    {
        if (isHittingAnimating) yield break;
        
        isHittingAnimating = true;
        
        // شغل أنيميشن الـ hitting (حركة اليد للأعلى)
        if (playerAnimator != null && !string.IsNullOrEmpty(hittingParameterName))
        {
            playerAnimator.SetTrigger(hittingParameterName);
        }
        
        // تفعيل حالة الضرب بعد فترة قصيرة (عندما تصل اليد للأعلى)
        yield return new WaitForSeconds(0.1f);
        isHittingAttacking = true;
        
        // إبقاء حالة الضرب لمدة قصيرة
        yield return new WaitForSeconds(0.2f);
        isHittingAttacking = false;
        
        isHittingAnimating = false;
    }

    void DoFreeze()
    {
        if (icePrefab == null || firePoint == null) return;

        GameObject ice = Instantiate(icePrefab, firePoint.position, Quaternion.identity);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 dir = (mouseWorld - firePoint.position).normalized;
        dir.y = 0; 
        dir = dir.normalized;

        Rigidbody2D rb = ice.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * iceSpeed;
            rb.gravityScale = 0f;
            rb.isKinematic = false;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

        Collider2D iceCollider = ice.GetComponent<Collider2D>();
        if (iceCollider != null)
            iceCollider.isTrigger = true;

        ice.transform.rotation = Quaternion.Euler(0, 0, 0);

        IceParticleController particleController = ice.GetComponent<IceParticleController>();
        if (particleController == null)
            ice.AddComponent<IceParticleController>();

        Destroy(ice, 4f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // إذا كان اللاعب في حالة ضرب ولمس enemy
        if (isHittingAttacking && collision.CompareTag("Enemy"))
        {
            // جرب EnemyHealth أولاً
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeHit();
                return;
            }
            
            // إذا لم يجد EnemyHealth، جرب EnemyMov
            EnemyMov enemy = collision.GetComponent<EnemyMov>();
            if (enemy != null)
            {
                enemy.TakeDamage(2); // ضربة قوية
            }
        }
    }

    void OnDestroy()
    {
        // لا حاجة لتنظيف الـ Trigger - ينتهي تلقائياً
    }
}
