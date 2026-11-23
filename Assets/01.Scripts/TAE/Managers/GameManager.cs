using _01.Scripts.TAE.CoreLib;
using _01.Scripts.TAE.Players;
using UnityEngine;

namespace _01.Scripts.TAE.Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        public PlatformPlayer player;
        
        public void TimeStop(bool isTimeStop)
        {
            Time.timeScale = isTimeStop ? 0f : 1f;
            player.MoveCompo.StopInput(isTimeStop);
        }
    }
}