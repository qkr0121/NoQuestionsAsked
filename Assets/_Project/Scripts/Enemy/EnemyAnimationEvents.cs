using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAttack enemyAttack;

    private void Awake()
    {
        // 이벤트는 Animator 가 있는 모델 오브젝트로 오므로 부모의 EnemyAttack 으로 전달
        enemyAttack = GetComponentInParent<EnemyAttack>();
    }

    // 공격 클립의 내려찍는 프레임에 Animation Event 로 지정
    public void OnAttackHit()
    {
        enemyAttack.OnBarricadeHit();
    }
}
