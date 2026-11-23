using System.Collections;
using _01.Scripts.TAE.Managers;
using _01.Scripts.TAE.Players;
using _01.Scripts.TAE.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01.Scripts.TAE.Combat
{
    public class GimmickController : MonoBehaviour
    {
        private PlayerInputReader playerInput;

        public void Initialize(PlatformPlayer player)
        {
            playerInput = player.InputCompo;
        }
        public void StartSelect()
        {
            StartCoroutine(StartSelectCo());
        }
        private IEnumerator StartSelectCo()
        {
            var manager = GimmickManager.Instance;

            GameManager.Instance.TimeStop(true);
            manager.StartSelect();
            
            playerInput.OnPrevKeyPressed += manager.Prev;
            playerInput.OnNextKeyPressed += manager.Next;

            while (!Keyboard.current.enterKey.wasPressedThisFrame)
            {
                yield return null;
            }

            playerInput.OnPrevKeyPressed -= manager.Prev;
            playerInput.OnNextKeyPressed -= manager.Next;
            
            yield return StartCoroutine(SelectCoroutine());

            manager.ClearSelection();
            GameManager.Instance.TimeStop(false);
        }
        
        private IEnumerator SelectCoroutine()
        {
            var manager = UIManager.Instance;
            
            manager.ShowGimmickUI(GimmickManager.Instance.CurrentPointPos());

            while (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                yield return null;
            }

            manager.SelectFirstBtn();
            
            while (!Keyboard.current.enterKey.wasPressedThisFrame)
            {
                yield return null;
            }
            
            manager.HideGimmickUI();
        }
    }
}