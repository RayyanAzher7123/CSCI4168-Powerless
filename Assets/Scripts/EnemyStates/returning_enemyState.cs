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
        Animator animator = enemyController.getEnemyAnimator();
        animator.SetBool("isReturning", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isFrozen", false);

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
