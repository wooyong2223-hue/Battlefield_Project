using UnityEngine;

// 모든 장비의 추상 부모 클래스
public abstract class JetEquipment : MonoBehaviour
{
    [Header("Equipment Settings")]
    [SerializeField] protected float cooldown = 1f; // 재사용 대기시간
    protected float lastUseTime = -100f; // 마지막 사용시간

    // 장비 사용 시도 함수
    public virtual bool TryUse()
    {
        if(Time.time >= lastUseTime + cooldown)
        {
            lastUseTime = Time.time;
            ExecuteAction(); // 실제 발동 로직(자식 클래스 구현)
            return true;
        }
        return false;
    }

    protected abstract void ExecuteAction();
}