using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    private static AmbienceManager instance;

    void Awake()
    {

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
