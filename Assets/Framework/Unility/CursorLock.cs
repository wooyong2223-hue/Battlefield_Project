using UnityEngine;

namespace Battlefield.Framework.Unility
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