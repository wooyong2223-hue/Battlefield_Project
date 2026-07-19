using UnityEngine;

public class MissileLauncher : JetEquipment
{
    [Header("Missile")]
    [SerializeField] private Transform missilePoint;
    [SerializeField] private GameObject missilePrefab;
    
    protected override void ExecuteAction()
    {
        if (missilePoint != null && missilePrefab != null)
        {
            Instantiate(missilePrefab, missilePoint.position, missilePoint.rotation);
        }
        Debug.Log("MISSILE MISSILE MISSILE MISSILE");
    }
}