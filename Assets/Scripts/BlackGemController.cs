using UnityEngine;
using UnityEngine.Events;

public class BlackGemController : MonoBehaviour
{
    public UnityEvent OnPlayerCollision;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerCollision.Invoke();
            Destroy(this.gameObject);
        }
    }

    
}

