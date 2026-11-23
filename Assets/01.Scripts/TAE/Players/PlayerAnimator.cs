using System.Collections.Generic;
using _01.Scripts.TAE.Agents;
using _01.Scripts.TAE.Data;
using UnityEngine.U2D.Animation;

namespace _01.Scripts.TAE.Players
{
    public class PlayerAnimator : AgentAnimator
    {
        private SpriteLibrary _spriteLibrary;
        private PlatformPlayer _owner;
        
        private Dictionary<UIDataSO, SpriteLibraryAsset> _dataDict;

        public void Initialize(PlatformPlayer playerController)
        {
            _owner = playerController;
            _spriteLibrary = GetComponent<SpriteLibrary>();
        }


        public void ChangeAgent(UIDataSO data)
        {
            _spriteLibrary.spriteLibraryAsset = data.spriteLibrary;
        }
    }
}