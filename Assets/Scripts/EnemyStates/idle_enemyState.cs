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
        Animator animator = enemyController.getEnemyAnimator();
        animator.SetBool("isFrozen", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isReturning", false);

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
