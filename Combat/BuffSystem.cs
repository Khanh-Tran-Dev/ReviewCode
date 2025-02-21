using UnityEngine;
using System;

namespace SurZom.Combat
{
    public class BuffSystem : MonoBehaviour
    {
        public event Action OnStunned;
        public event Action OnControlReleased;
        private CharacterStat stats;

        private void Start()
        {
            stats = GetComponent<CharacterStat>();
        }

        public bool ApplyStun()
        {
            if (stats.CcResist) return false;
            OnStunned?.Invoke();
            return true;
        }

        public void ReleaseControl()
        {
            OnControlReleased?.Invoke();
        }
    }
}

