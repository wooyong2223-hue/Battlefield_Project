using UnityEngine;

namespace Battlefield.Fighter.Controller
{
    public class KeyboardJetInput : MonoBehaviour, IJetInput
    {
        public float Throttle { get; private set; }
        public float Pitch { get; private set; }
        public float Roll { get; private set; }
        public float Yaw { get; private set; }

        public bool ChangeCamera { get; private set; }
        public bool RearView { get; private set; }

        private void Update()
        {
            Throttle = Input.GetAxis("Vertical");
            Pitch = -Input.GetAxis("Mouse Y");
            Roll = -Input.GetAxis("Mouse X");

            Yaw = 0f;

            if (Input.GetKey(KeyCode.A))
            {
                Yaw = -1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Yaw = 1f;
            }

            ChangeCamera = Input.GetKeyDown(KeyCode.C);
            RearView = Input.GetKey(KeyCode.V);
        }
    }
}