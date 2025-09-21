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
    public GameObject freezePrefab; // Use existing Freez prefab
    public float iceSpeed = 20f; // Faster speed
    public LayerMask freezeTargetLayer;
    public float freezeRange = 15f;
    public float freezeDuration = 2f;
    public float freezeCooldown = 0.3f; // Very short cooldown
    private float freezeTimer = 0f;
    
    [Header("Freeze Combat System")]
    public int freezeHitsToKill = 2; // Number of freeze hits to kill enemy

    void Awake()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        Debug.Log($"[PlayerAbilities] Start called - freezePrefab: {(freezePrefab != null ? "Found" : "NULL")}, firePoint: {(firePoint != null ? "Found" : "NULL")}");
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
        if (hittingTimer > 0f) hittingTimer -= Time.deltaTime;
        if (freezeTimer > 0f) freezeTimer -= Time.deltaTime;
        

        // إطلاق النار فقط عند الضغط
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("[PlayerAbilities] Mouse clicked - calling TryUseAbility");
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
        Debug.Log($"[PlayerAbilities] TryUseAbility called - Current ability: {currentAbility}");
        
        switch (currentAbility)
        {
            case AbilityType.Shooting:
                if (shootTimer <= 0f) 
                { 
                    Debug.Log("[PlayerAbilities] Shooting ability activated!");
                    DoShoot(); 
                    shootTimer = shootCooldown; 
                }
                else
                {
                    Debug.Log($"[PlayerAbilities] Shooting on cooldown: {shootTimer:F2}s");
                }
                break;
            case AbilityType.Sword:
                if (hittingTimer <= 0f) 
                { 
                    Debug.Log("[PlayerAbilities] Sword ability activated!");
                    StartCoroutine(DoHitting()); 
                    hittingTimer = hittingCooldown; 
                }
                else
                {
                    Debug.Log($"[PlayerAbilities] Sword on cooldown: {hittingTimer:F2}s");
                }
                break;
            case AbilityType.Freeze:
                if (freezeTimer <= 0f) 
                { 
                    Debug.Log("[PlayerAbilities] Freeze ability activated!");
                    DoFreeze(); 
                    freezeTimer = freezeCooldown; 
                }
                else
                {
                    Debug.Log($"[PlayerAbilities] Freeze on cooldown: {freezeTimer:F2}s");
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
        // Play shoot sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShootSound();
        }

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
        // Only play hitting animation in Sky scene
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!currentScene.Contains("Sky") && !currentScene.Contains("SKY"))
        {
            Debug.Log("[PlayerAbilities] Hitting animation skipped - not in Sky scene");
            yield break;
        }
        
        if (isHittingAnimating) yield break;
        
        isHittingAnimating = true;
        
        // شغل أنيميشن الـ hitting (حركة اليد للأعلى)
        if (playerAnimator != null && !string.IsNullOrEmpty(hittingParameterName))
        {
            playerAnimator.SetTrigger(hittingParameterName);
        }
        
        // تفعيل حالة الضرب فوراً (مزامنة مع PlayerController)
        isHittingAttacking = true;
        Debug.Log("[PlayerAbilities] Hitting attack activated!");
        
        // إبقاء حالة الضرب لمدة أطول (لضمان موت الانمي)
        yield return new WaitForSeconds(0.5f);
        isHittingAttacking = false;
        Debug.Log("[PlayerAbilities] Hitting attack deactivated!");
        
        isHittingAnimating = false;
    }

    // Check if player is currently hitting
    public bool IsHittingAttacking()
    {
        return isHittingAttacking;
    }

    void DoFreeze()
    {
        if (freezePrefab == null || firePoint == null) 
        {
            Debug.LogError("[PlayerAbilities] DoFreeze failed - freezePrefab or firePoint is null!");
            return;
        }

        // Play shooting animation for freeze (same as shooting)
        if (playerAnimator != null && useShootingAnimation)
        {
            playerAnimator.SetTrigger(shootTriggerName);
            Debug.Log("[PlayerAbilities] Freeze shooting animation triggered!");
        }

        Debug.Log("[PlayerAbilities] DoFreeze called - creating freeze particle");
        GameObject ice = Instantiate(freezePrefab, firePoint.position, Quaternion.identity);

        // Always shoot left (horizontal direction)
        Vector2 dir = Vector2.left; // Always go left

        Rigidbody2D rb = ice.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * iceSpeed;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

        Collider2D iceCollider = ice.GetComponent<Collider2D>();
        if (iceCollider != null)
            iceCollider.isTrigger = true;

        // Set rotation to horizontal (0 degrees for left direction)
        ice.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        // If the particle system has a main module, set the start rotation and make it burst
        ParticleSystem ps = ice.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startRotation = 0f; // 0 degrees = horizontal
            main.startLifetime = 1f; // Normal lifetime
            main.startSpeed = 15f; // Normal speed
            // Don't limit particles - let it work naturally
            // Stop continuous emission by setting emission rate to 0
            
            // Let particle system work naturally - no burst, no limits
            var emission = ps.emission;
            emission.enabled = true;
            // Don't modify emission - let it work as designed
            
            Debug.Log("[PlayerAbilities] Set particle system to burst mode - fast and short!");
        }

        // No need for IceParticleController - let particle system work naturally

        Destroy(ice, 2f); // Destroy after 2 seconds
    }
    

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Only handle hitting in Sky scene
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
        bool isSkyScene = currentScene.Contains("sky");
        
        if (!isSkyScene)
        {
            // Skip hitting logic in non-sky scenes
            return;
        }
        
        Debug.Log($"[PlayerAbilities] OnTriggerEnter2D: {collision.name}, isHittingAttacking: {isHittingAttacking}, Tag: {collision.tag}");
        
        // إذا كان اللاعب في حالة ضرب ولمس enemy (فقط في sky scene)
        if (isHittingAttacking && collision.CompareTag("Enemy"))
        {
            Debug.Log($"[PlayerAbilities] Hitting enemy in sky scene: {collision.name}");
            
            // جرب EnemyHealth أولاً
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Debug.Log($"[PlayerAbilities] Found EnemyHealth, calling TakeHit()");
                enemyHealth.TakeHit();
                return;
            }
            
            // إذا لم يجد EnemyHealth، جرب FlyingEnemy
            FlyingEnemy flyingEnemy = collision.GetComponent<FlyingEnemy>();
            if (flyingEnemy != null)
            {
                Debug.Log($"[PlayerAbilities] Found FlyingEnemy, calling TakeDamage(3)");
                flyingEnemy.TakeDamage(3); // ضربة قاتلة (يقتل بضربة واحدة)
                return;
            }
            
            // إذا لم يجد FlyingEnemy، جرب EnemyMov
            EnemyMov enemy = collision.GetComponent<EnemyMov>();
            if (enemy != null)
            {
                Debug.Log($"[PlayerAbilities] Found EnemyMov, calling TakeDamage(2)");
                enemy.TakeDamage(2); // ضربة قوية
            }
            else
            {
                Debug.Log($"[PlayerAbilities] No enemy component found on: {collision.name}");
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only handle hitting in Sky scene
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
        bool isSkyScene = currentScene.Contains("sky");
        
        if (!isSkyScene)
        {
            // Skip hitting logic in non-sky scenes
            return;
        }
        
        Debug.Log($"[PlayerAbilities] OnCollisionEnter2D: {collision.gameObject.name}, isHittingAttacking: {isHittingAttacking}, Tag: {collision.gameObject.tag}");
        
        // إذا كان اللاعب في حالة ضرب ولمس enemy (فقط في sky scene)
        if (isHittingAttacking && collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"[PlayerAbilities] Hitting enemy in sky scene: {collision.gameObject.name}");
            
            // جرب EnemyHealth أولاً
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Debug.Log($"[PlayerAbilities] Found EnemyHealth, calling TakeHit()");
                enemyHealth.TakeHit();
                return;
            }
            
            // إذا لم يجد EnemyHealth، جرب FlyingEnemy
            FlyingEnemy flyingEnemy = collision.gameObject.GetComponent<FlyingEnemy>();
            if (flyingEnemy != null)
            {
                Debug.Log($"[PlayerAbilities] Found FlyingEnemy, calling TakeDamage(3)");
                flyingEnemy.TakeDamage(3); // ضربة قاتلة (يقتل بضربة واحدة)
                return;
            }
            
            // إذا لم يجد FlyingEnemy، جرب EnemyMov
            EnemyMov enemy = collision.gameObject.GetComponent<EnemyMov>();
            if (enemy != null)
            {
                Debug.Log($"[PlayerAbilities] Found EnemyMov, calling TakeDamage(2)");
                enemy.TakeDamage(2); // ضربة قوية
            }
            else
            {
                Debug.Log($"[PlayerAbilities] No enemy component found on: {collision.gameObject.name}");
            }
        }
    }

    void OnDestroy()
    {
        // لا حاجة لتنظيف الـ Trigger - ينتهي تلقائياً
    }
}
