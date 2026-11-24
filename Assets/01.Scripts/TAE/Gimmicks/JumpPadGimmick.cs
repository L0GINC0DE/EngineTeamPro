using _01.Scripts.TAE.Players;
using UnityEngine;

namespace _01.Scripts.TAE
{
    public class JumpPadGimmick : GimmickBase
    {
        [SerializeField, Min(0f)] private float launchSpeed = 27f;
        [SerializeField, Min(0f)] private float ascentDeceleration = 27f;


        public override void Activate(PlatformPlayer player)
        {
            player.MoveCompo.Bounce(launchSpeed, ascentDeceleration);
        }
    }
}
