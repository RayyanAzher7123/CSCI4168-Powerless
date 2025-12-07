using UnityEngine;

public class GetBatteryQueue : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip clip;
    
    bool wasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!wasTriggered)
        {
            source.PlayOneShot(clip);
            wasTriggered = true;
        }
    }
}
