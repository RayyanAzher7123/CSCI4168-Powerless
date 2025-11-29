using UnityEngine;

public class attacking_enemyState : GenericState
{
    private EnemyController enemyController;
    
    public override void Setup(GameObject parent)
    {
        enemyController = parent.GetComponentInChildren<EnemyController>();
    }

    public override void Enter()
    {
        enemyController.activateActiveModel();
        enemyController.PlayAttackSound();
        
    }

    public override void Do()
    {
        
    }

    public override void FixedDo()
    {
        enemyController.huntPlayer();
    }

    public override void Exit()
    {
        
    }
}
