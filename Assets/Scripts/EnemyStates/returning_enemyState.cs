using UnityEngine;

public class returning_enemyState : GenericState
{
    private EnemyController enemyController;
    
    public override void Setup(GameObject parent)
    {
        enemyController = parent.GetComponentInChildren<EnemyController>();
    }

    public override void Enter()
    {
        enemyController.chooseIdlePoint();
    }

    public override void Do()
    {
        
    }

    public override void FixedDo()
    {
        enemyController.moveToIdlePoint();
    }

    public override void Exit()
    {
        
    }
}
