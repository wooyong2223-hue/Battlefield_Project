using UnityEngine;

public class MachineGun : JetEquipment
{
    [Header("MachineGun")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    protected override void ExecuteAction()
    {
        if (firePoint != null && bulletPrefab != null)
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Debug.Log("BULLET BULLET BULLET BULLET");
    }
}