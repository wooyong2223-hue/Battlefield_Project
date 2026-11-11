using UnityEngine;

namespace Battlefield.Framework.Utility
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