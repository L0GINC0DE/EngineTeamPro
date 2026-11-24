using System;
using _01.Scripts.TAE.Players;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE
{
    public class ShieldGimmick : GimmickBase
    {
        [SerializeField, Range(0f, 1f)] private float moveMultiplier = 0.5f;

        private PlatformPlayer equippedPlayer;


        public override void Activate(PlatformPlayer player)
        {
            equippedPlayer = player;
            player.EquipGimmickShield(this, moveMultiplier);
        }

        private void OnDisable() => Release();

        private void Update()
        {
            if (Keyboard.current.zKey.wasPressedThisFrame)
                equippedPlayer.ReleaseGimmickShield(this);
        }

        private void Release()
        {
            if (equippedPlayer != null)
                equippedPlayer.ReleaseGimmickShield(this);

            equippedPlayer = null;
        }
    }
}
