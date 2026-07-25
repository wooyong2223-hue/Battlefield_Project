using UnityEngine;

namespace Battlefield.Core
{
    public class stat : MonoBehaviour
    {
        [SerializeField] private float _attack = 20f;
        [SerializeField] private float _defense = 0f;
        [SerializeField] private float _moveSpeed = 1f;
    }
}
