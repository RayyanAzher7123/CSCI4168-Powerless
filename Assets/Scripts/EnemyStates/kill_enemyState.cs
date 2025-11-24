using UnityEngine;
using UnityEngine.SceneManagement;

public class kill_enemyState : GenericState
{
    private EnemyController enemyController;
    
    public override void Setup(GameObject parent)
    {
        enemyController = parent.GetComponentInChildren<EnemyController>();
    }

    public override void Enter()
    {
        enemyController.killPlayer();
        enemyController.PlayFinalAttackAndDie();
    }

    public override void Do()
    {
        
    }

    public override void FixedDo()
    {
        
    }

    public override void Exit()
    {
        
    }
}
