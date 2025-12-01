using UnityEngine;

public class frozen_enemyState : GenericState
{
    private EnemyController enemyController;

    public override void Setup(GameObject parent)
    {
        enemyController = parent.GetComponentInChildren<EnemyController>();
    }

    public override void Enter()
    {
        enemyController.stopEnemyMovement();
        Animator animator = enemyController.getEnemyAnimator();
        animator.SetBool("isFrozen", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isReturning", false);
        animator.SetBool("isKilling", false);

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
