using UnityEngine;

public class EnemyTesterAnimator : MonoBehaviour
{
    void Start()
    {
        var anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("MannequinTest: No Animator on this GameObject");
            return;
        }

        Debug.Log("MannequinTest: Playing Run on " + gameObject.name);
        anim.Play("Run", 0, 0f);        // exact clip name from the controller
    }
}
