using UnityEngine;
public class attacking_enemyState : GenericState
{
    private EnemyController enemyController;

    private bool running_animation = true;

    public override void Setup(GameObject parent)
    {
        enemyController = parent.GetComponentInChildren<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("EnemyController is null in attacking_enemyState.Setup()");
            return;
        }
    }

    public override void Enter()
    {

        Animator animator = enemyController.getEnemyAnimator();

        if (animator == null)
        {
            Debug.LogError("Animator is null in attacking_enemyState.Enter()");
            return;
        }


        animator.SetBool("isAttacking", true);
        animator.SetBool("isFrozen", false);
        Debug.Log("Enemy is attacking the player.");

        if (running_animation)
        {
            Debug.Log("Enemy attacking state: Running animation");
            animator.SetBool("isRunning", true);
            animator.SetBool("isWalking", false);
        }
        else
        {
            Debug.Log("Enemy attacking state: Walking animation");
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", true);
        }

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
