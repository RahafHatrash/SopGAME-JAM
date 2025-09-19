using System.Collections;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public enum AbilityType { None, Shooting, Sword, Freeze }
    public AbilityType currentAbility = AbilityType.None;
    
    [Header("Auto-Detection")]
    public bool autoDetectScene = true; // تفعيل الكشف التلقائي للـ scene

    [Header("Shooting")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float shootCooldown = 0.4f;
    private float shootTimer = 0f;

    [Header("Sword")]
    public GameObject swordHitbox;
    public float swordActiveTime = 0.12f;
    public float swordCooldown = 0.6f;
    private bool swordReady = true;

    [Header("Freeze")]
    public GameObject icePrefab;
    public float iceSpeed = 12f;  // أسرع وأكثر استقراراً
    public LayerMask freezeTargetLayer;
    public float freezeRange = 15f;  // مدى أطول
    public float freezeDuration = 2f;
    public float freezeCooldown = 1f;
    private float freezeTimer = 0f;

    void Start()
    {
        if (autoDetectScene)
        {
            DetectSceneAndSetAbility();
        }
    }

    void Update()
    {
        // إزالة اختيار القدرات اليدوي إذا كان الكشف التلقائي مفعل
        if (!autoDetectScene)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) currentAbility = AbilityType.Shooting;
            if (Input.GetKeyDown(KeyCode.Alpha2)) currentAbility = AbilityType.Sword;
            if (Input.GetKeyDown(KeyCode.Alpha3)) currentAbility = AbilityType.Freeze;
        }
        else
        {
            // إمكانية تغيير القدرة يدوياً حتى مع الكشف التلقائي (اختياري)
            if (Input.GetKeyDown(KeyCode.Alpha1)) 
            {
                currentAbility = AbilityType.Shooting;
                Debug.Log("[Manual Override] Set to Shooting");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) 
            {
                currentAbility = AbilityType.Sword;
                Debug.Log("[Manual Override] Set to Sword");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) 
            {
                currentAbility = AbilityType.Freeze;
                Debug.Log("[Manual Override] Set to Freeze");
            }
        }

        if (shootTimer > 0f) shootTimer -= Time.deltaTime;
        if (freezeTimer > 0f) freezeTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            TryUseAbility();
    }
    
    void DetectSceneAndSetAbility()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
        
        Debug.Log($"[Auto-Detection] Current scene: {sceneName}");
        
        // كشف نوع الـ scene وتحديد القدرة المناسبة
        if (sceneName.Contains("cloud") || sceneName.Contains("sky"))
        {
            currentAbility = AbilityType.Shooting;
            Debug.Log("[Auto-Detection] Cloud/Sky scene detected - Set to Shooting");
        }
        else if (sceneName.Contains("snow") || sceneName.Contains("ice"))
        {
            currentAbility = AbilityType.Freeze;
            Debug.Log("[Auto-Detection] Snow/Ice scene detected - Set to Freeze");
        }
        else if (sceneName.Contains("slime") || sceneName.Contains("swamp"))
        {
            currentAbility = AbilityType.Sword;
            Debug.Log("[Auto-Detection] Slime/Swamp scene detected - Set to Sword");
        }
        else
        {
            // افتراضي: الطلق
            currentAbility = AbilityType.Shooting;
            Debug.Log("[Auto-Detection] Unknown scene - Default to Shooting");
        }
    }
    
    // دالة لإعادة كشف الـ scene (يمكن استدعاؤها من مكان آخر)
    public void ReDetectScene()
    {
        if (autoDetectScene)
        {
            DetectSceneAndSetAbility();
        }
    }

    void TryUseAbility()
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

    void DoShoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

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

    IEnumerator DoSword()
    {
        if (swordHitbox == null) yield break;
        swordReady = false;
        swordHitbox.SetActive(true);
        yield return new WaitForSeconds(swordActiveTime);
        swordHitbox.SetActive(false);
        yield return new WaitForSeconds(Mathf.Max(0f, swordCooldown - swordActiveTime));
        swordReady = true;
    }

    void DoFreeze()
    {
        if (icePrefab == null || firePoint == null) return;

        GameObject ice = Instantiate(icePrefab, firePoint.position, Quaternion.identity);
        
        // اتجاه باتجاه الماوس (أفقي فقط)
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 dir = (mouseWorld - firePoint.position).normalized;
        
        // إجبار الاتجاه ليكون أفقي (y = 0)
        dir.y = 0;
        dir = dir.normalized;

        Rigidbody2D rb = ice.GetComponent<Rigidbody2D>();
        if (rb != null) 
        {
            rb.linearVelocity = dir * iceSpeed;
            // منع الـ Ice من التأثر بالجاذبية
            rb.gravityScale = 0f;
            // منع الـ Ice من التأثير على الأجسام الأخرى
            rb.isKinematic = false;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }
        
        // إعداد Collider للـ Ice
        Collider2D iceCollider = ice.GetComponent<Collider2D>();
        if (iceCollider != null)
        {
            iceCollider.isTrigger = true; // لا يدفع الأجسام
        }

        // دوران أفقي (0 درجة للاتجاه الأفقي)
        ice.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        // إضافة IceParticleController للتحكم في الـ Particle System
        IceParticleController particleController = ice.GetComponent<IceParticleController>();
        if (particleController == null)
        {
            particleController = ice.AddComponent<IceParticleController>();
        }

        // مدة أطول للـ Ice ليصل لمسافة أبعد
        Destroy(ice, 4f);

        // لا نستخدم Raycast هنا - التجميد يحدث عند الاصطدام في IceEffect.cs
        Debug.Log("[Freeze] Ice particle launched horizontally - will freeze on contact with Ping enemies");
    }
}
