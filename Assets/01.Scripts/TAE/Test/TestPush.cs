using _01.Scripts.TAE.Players;
using UnityEngine;

namespace _01.Scripts.TAE.Test
{
    
    public enum PushDirType {None, Right, Left }

    public class TestPush : MonoBehaviour
    {
        [field: SerializeField] public PushDirType DirType { get; private set; }
        public float power;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlatformPlayer player))
            {
                player.MoveCompo.SetPushDir((DirType == PushDirType.Left ? Vector2.left : Vector2.right) * power, DirType);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlatformPlayer player))
            {
                player.MoveCompo.SetPushDir(Vector2.zero);
            }
        }
    }
}