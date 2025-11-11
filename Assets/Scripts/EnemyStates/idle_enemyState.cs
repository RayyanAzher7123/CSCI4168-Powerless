using UnityEngine;

public class idle_enemyState : GenericState
{
    private EnemyController enemyController;
    
    public override void Setup(GameObject parent)
    {
        enemyController = parent.GetComponentInChildren<EnemyController>();
    }

    public override void Enter()
    {
        enemyController.activateIdleModel();
        enemyController.stopEnemyMovement();
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
