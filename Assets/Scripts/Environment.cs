using UnityEngine;

public class Environment : MonoBehaviour
{
    void Start()
    {
        RenderSettings.ambientIntensity = 0.5f;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogDensity = 0.1f;
    }
}
