using UnityEngine;

public class IceParticleController : MonoBehaviour
{
    [Header("Ice Settings")]
    public float lifetime = 4f;
    public float speed = 12f;
    
    void Start()
    {
        Debug.Log("[IceParticleController] Ice particle created!");
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[IceParticleController] Ice particle hit: {other.name}, Tag: {other.tag}");
        
        // Check if it hit an enemy
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("[IceParticleController] Ice particle hit enemy - applying freeze effect!");
            
            // Apply freeze effect
            IceEffect iceEffect = GetComponent<IceEffect>();
            if (iceEffect == null)
            {
                iceEffect = gameObject.AddComponent<IceEffect>();
            }
            
            // Destroy the ice particle
            Destroy(gameObject);
        }
    }
}