using UnityEngine;

public class FlareDispenser : JetEquipment
{
    [Header("Flare")]
    [SerializeField] private GameObject flareEffectPrefab;

    protected override void ExecuteAction()
    {
        if (flareEffectPrefab != null)
        {
            // 전투기 뒤쪽에 플레어 생성
            Instantiate(flareEffectPrefab, transform.position, Quaternion.identity);
        }
        Debug.Log("FLATE FLATE FLATE FLATE");
        // (여기에 자신을 향해 날아오는 미사일의 타겟팅을 해제하는 로직을 나중에 추가)
    }
}