using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private AnimationClip attackClip;   // 공격 클립 길이를 방어벽 공격 간격에 맞춰 배속하기 위해 필요
    [SerializeField] private float walkAnimationSpeed = 4f;   // 슬로우가 없을 때 걷기 클립 배속 - 이동 속도에 비해 느린 클립이라 올림

    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");
    private static readonly int WalkSpeedHash = Animator.StringToHash("WalkSpeed");

    private Animator animator;
    private EnemyMovement enemyMovement;
    private EnemyAttack enemyAttack;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();   // 모델이 자식 FBX라 GetComponentInChildren 필요
        enemyMovement = GetComponent<EnemyMovement>();
        enemyAttack = GetComponent<EnemyAttack>();
    }

    private void Update()
    {
        // 방어벽에 막혀 공격 중일 때만 공격 모션 - 이동·피해 판정은 기존 EnemyAttack/EnemyMovement 그대로 두기 위해 상태만 읽음
        animator.SetBool(IsAttackingHash, enemyMovement.IsBlocked);

        // 클립 한 바퀴가 방어벽 공격 간격과 같아지도록 배속 - 모션 주기와 피해 주기가 어긋나지 않게 함
        // 풀에서 재사용될 때 Animator 가 파라미터를 초기화할 수 있어 매 프레임 지정
        animator.SetFloat(AttackSpeedHash, attackClip.length / enemyAttack.BarricadeAttackInterval);

        // 맞아서 느려지면 걷는 모션도 같이 느려져 피격 슬로우가 눈에 보이게 함
        animator.SetFloat(WalkSpeedHash, walkAnimationSpeed * enemyMovement.SpeedRatio);
    }
}
