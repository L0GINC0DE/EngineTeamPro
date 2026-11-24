using _01.Scripts.TAE.Players;
using UnityEngine;

namespace _01.Scripts.TAE
{
    public class LaunchGimmick : GimmickBase
    {
        [SerializeField] private bool launchRight = true;
        [SerializeField] private float speed = 12f;
        [SerializeField, Min(0f)] private float duration = 0.65f;

        public override void SetDirection(bool right) => launchRight = right;


        public override void Activate(PlatformPlayer player)
        {
            player.MoveCompo.Launch(launchRight ? speed : -speed, duration);
        }
    }
}
