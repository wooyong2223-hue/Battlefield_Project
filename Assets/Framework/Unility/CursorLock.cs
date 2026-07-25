using UnityEngine;

namespace Battlefield.Unility
{
    public class CursorLock : MonoBehaviour
    {
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}