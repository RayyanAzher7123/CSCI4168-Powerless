using UnityEngine;

public class GenericState : MonoBehaviour
{
    public virtual void Setup(GameObject parent) { }
    public virtual void Enter() { }
    public virtual void Do() { }
    public virtual void FixedDo() { }
    public virtual void Exit() { }
}
