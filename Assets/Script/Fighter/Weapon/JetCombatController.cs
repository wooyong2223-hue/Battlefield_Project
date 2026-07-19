using UnityEngine;

public class JetCombatController : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private JetEquipment machineGun;
    [SerializeField] private JetEquipment missile;

    [Header("Utilities")]
    [SerializeField] private JetEquipment flare;
    [SerializeField] private JetEquipment repair;

    // 핵심: 현재 들고 있는 무기를 저장할 변수입니다. (다형성 활용)
    private JetEquipment currentWeapon;

    [Header("Zoom Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float normalFOV = 60f; // 기본 시야각
    [SerializeField] private float zoomFOV = 30f;   // 줌 당겼을 때의 시야각
    [SerializeField] private float zoomSpeed = 10f; // 줌인/아웃 속도

    private bool isZooming = false;

    void Start()
    {
        // 시작할 때 기본 무기를 기관총으로 설정해 줍니다.
        currentWeapon = machineGun;

        // 카메라를 깜빡하고 안 넣었을 경우 자동으로 메인 카메라를 찾아줍니다.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // 1. 무기 교체 (1번: 기관총, 2번: 미사일)
        if (Input.GetKeyDown(KeyCode.Alpha1) && machineGun != null)
        {
            currentWeapon = machineGun;
            Debug.Log("무기 장착: 기관총");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && missile != null)
        {
            currentWeapon = missile;
            Debug.Log("무기 장착: 미사일");
        }

        // 2. 무기 발사 (좌클릭 유지)
        // currentWeapon이 기관총이든 미사일이든 상관없습니다! 똑같이 TryUse()만 부르면 됩니다.
        if (Input.GetMouseButton(0) && currentWeapon != null)
        {
            currentWeapon.TryUse();
        }

        // 3. 확대/축소 상태 감지 (우클릭 유지)
        isZooming = Input.GetMouseButton(1);

        // 4. 유틸리티 (F: 플레어, R: 수리)
        if (Input.GetKeyDown(KeyCode.F) && flare != null) flare.TryUse();
        if (Input.GetKeyDown(KeyCode.R) && repair != null) repair.TryUse();
    }

    void LateUpdate()
    {
        // 줌 기능은 카메라 조작이므로 LateUpdate에서 처리하는 것이 더 부드럽습니다.
        HandleZoom();
    }

    private void HandleZoom()
    {
        if (mainCamera == null) return;

        // 우클릭 중이면 zoomFOV로, 아니면 normalFOV로 목표값을 정합니다.
        float targetFOV = isZooming ? zoomFOV : normalFOV;

        // Mathf.Lerp를 이용해 현재 시야각에서 목표 시야각으로 부드럽게 값을 변경합니다.
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }
}