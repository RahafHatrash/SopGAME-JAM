using UnityEngine;

public class IceParticleController : MonoBehaviour
{
    [Header("Particle Settings")]
    public float particleSpeed = 15f;  // سرعة الـ particles
    public float particleLifetime = 2f;  // مدة حياة الـ particles
    public float particleSpread = 0.5f;  // انتشار الـ particles
    
    private ParticleSystem particleSystem;
    private Rigidbody2D rb;
    private Vector2 direction;
    
    void Start()
    {
        // الحصول على ParticleSystem
        particleSystem = GetComponent<ParticleSystem>();
        rb = GetComponent<Rigidbody2D>();
        
        // الحصول على اتجاه الحركة من الـ Rigidbody (أفقي فقط)
        if (rb != null && rb.linearVelocity.magnitude > 0)
        {
            direction = rb.linearVelocity.normalized;
            // إجبار الاتجاه ليكون أفقي
            direction.y = 0;
            direction = direction.normalized;
        }
        else
        {
            direction = Vector2.right; // افتراضي للأمام
        }
        
        // إعداد الـ ParticleSystem
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startSpeed = particleSpeed;
            main.startLifetime = particleLifetime;
            main.startSize = 0.2f; // حجم مناسب للـ particles
            main.maxParticles = 100; // عدد أكثر للـ particles
            main.startColor = Color.cyan; // لون أزرق فاتح مثل الثلج
            
            // إعداد الـ emission
            var emission = particleSystem.emission;
            emission.rateOverTime = 50f; // معدل إنتاج الـ particles
            
            var shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f; // زاوية صغيرة جداً للـ cone
            shape.radius = 0.1f; // نصف قطر صغير
            shape.length = 0.1f; // طول صغير
            
            // اتجاه الـ cone أفقي (90 درجة للاتجاه الأفقي)
            shape.rotation = new Vector3(0, 0, 90f);
            
            // إعداد الـ velocity over lifetime (أفقي فقط)
            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.x = direction.x * particleSpeed;
            velocityOverLifetime.y = 0; // لا حركة عمودية
            
            // إعداد الـ size over lifetime (لجعل الـ particles تكبر مع الوقت)
            var sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
            
            // إعداد الـ color over lifetime (لجعل الـ particles تختفي تدريجياً)
            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLifetime.color = gradient;
            
            // بدء الـ ParticleSystem
            particleSystem.Play();
        }
        
        // تدمير الكائن بعد مدة معينة
        Destroy(gameObject, 4f);
    }
    
    void Update()
    {
        // التأكد من أن الـ ParticleSystem يعمل
        if (particleSystem != null && !particleSystem.isPlaying)
        {
            particleSystem.Play();
        }
        
        // تحديث اتجاه الـ particles باستمرار (أفقي فقط)
        if (particleSystem != null && rb != null && rb.linearVelocity.magnitude > 0)
        {
            Vector2 currentDirection = rb.linearVelocity.normalized;
            // إجبار الاتجاه ليكون أفقي
            currentDirection.y = 0;
            currentDirection = currentDirection.normalized;
            
            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            velocityOverLifetime.x = currentDirection.x * particleSpeed;
            velocityOverLifetime.y = 0; // لا حركة عمودية
        }
    }
}
