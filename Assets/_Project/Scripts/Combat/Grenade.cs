using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private GameObject explosionEffect;        // 비워둬도 동작 - 임시 이펙트 확인용
    [SerializeField] private SpriteRenderer landingIndicator;   // 착탄 후 폭발까지 표시할 폭발 범위

    private GrenadeData data;
    private Vector3 startPoint;
    private Vector3 landPoint;
    private float flightTimer;
    private float fuseTimer;
    private bool hasLanded;

    public void Launch(Vector3 start, Vector3 land, GrenadeData grenadeData)
    {
        data = grenadeData;
        startPoint = start;
        landPoint = land;
        transform.position = start;
    }

    private void Update()
    {
        if(!hasLanded)
        {
            flightTimer += Time.deltaTime;
            float t = Mathf.Clamp01(flightTimer / data.flightTime);

            // 물리 대신 포물선을 직접 계산 / 좀비,방어벽과 충돌하지 않고 착탄 원 위치에 정확히 떨어지게 하려고
            Vector3 pos = Vector3.Lerp(startPoint, landPoint, t);
            pos.y += data.arcHeight * 4f * t * (1f - t);
            transform.position = pos;

            if (t >= 1f)
            {
                hasLanded = true;
                ShowLandingIndicator();
            }
            return;
        }

        fuseTimer += Time.deltaTime;
        if(fuseTimer >= data.fuseTime)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, data.explosionRadius);

        // 좀비 하나에 콜라이더가 여러 개면 중복으로 잡히므로 한 마리당 한 번만 피해
        HashSet<EnemyHealth> damaged = new HashSet<EnemyHealth>();
        foreach(Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if(enemy != null && damaged.Add(enemy))
            {
                enemy.TakeDamage(data.damage);
            }
        }

        if(explosionEffect != null)
        {
            Destroy(Instantiate(explosionEffect, transform.position, Quaternion.identity), 2f);
        }
        Destroy(gameObject);
    }

    private void ShowLandingIndicator()
    {
        // 수류탄 본체가 작게 스케일돼 있어도 월드 크기가 반경x2가 되도록 부모 스케일로 나눔
        float size = data.explosionRadius * 2f / transform.lossyScale.x;
        landingIndicator.transform.localScale = new Vector3(size, size, 1f);
        landingIndicator.transform.position = landPoint + Vector3.up * 0.05f; // 바닥과 겹쳐 깜빡이지 않게 살짝 띄움
        landingIndicator.gameObject.SetActive(true);
    }
}
