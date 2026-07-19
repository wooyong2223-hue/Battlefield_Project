using UnityEngine;

public class RepairSystem : JetEquipment
{
    [Header("Repair")]
    [SerializeField] private float healAmount = 30f;

    protected override void ExecuteAction()
    {
        // (나중에 만들 JetHealth 스크립트를 가져와서 체력을 올려주는 로직을 넣습니다)
        // JetHealth health = GetComponent<JetHealth>();
        // if(health != null) health.Heal(healAmount);

        Debug.Log($"Repair (+{healAmount} HP)");
    }
}