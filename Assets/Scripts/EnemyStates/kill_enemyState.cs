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
        Animator animator = enemyController.getEnemyAnimator();
        animator.SetBool("isKilling", true);
        //TODO: Play death cutscene
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
