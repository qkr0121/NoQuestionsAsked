using UnityEngine;

public class MuzzleFlashEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem flash;          // Flash 자식(루트 파티클 시스템) - 자식 파티클은 함께 재생됨
    [SerializeField] private Light flashLight;              // 비워도 동작
    [SerializeField] private float lightDuration = 0.06f;   // 파티클 수명(0.05초)보다 약간 길게

    private float lightTimer;

    private void Awake()
    {
        if (flashLight != null) flashLight.enabled = false;
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);   // 총구 위치에서, 캐릭터 회전이 늦게 따라와도 화염이 탄 방향을 향하게 함

        // 연사로 아직 재생 중이어도 처음부터 다시 터지게 함
        flash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        flash.Play(true);

        if (flashLight != null)
        {
            flashLight.enabled = true;
            lightTimer = lightDuration;
        }
    }

    private void Update()
    {
        if (lightTimer <= 0f) return;

        lightTimer -= Time.deltaTime;
        if (lightTimer <= 0f) flashLight.enabled = false;   // 발사 순간에만 총구 주변 바닥이 번쩍이게 함
    }
}
