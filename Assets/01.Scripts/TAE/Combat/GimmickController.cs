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

            manager.StartSelect();
            if (!manager.CurrentPoint) yield break;
            
            GameManager.Instance.TimeStop(true);
            
            
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

            void Previous() => manager.MoveGimmickSelection(-1);
            void Next() => manager.MoveGimmickSelection(1);

            playerInput.OnPrevKeyPressed += Previous;
            playerInput.OnNextKeyPressed += Next;

            try
            {
                // The Enter that picked the point must not confirm a gimmick.
                yield return null;
                while (true)
                {
                    var keyboard = Keyboard.current;
                    if (keyboard != null)
                    {
                        if (keyboard.leftArrowKey.wasPressedThisFrame)
                            Previous();
                        else if (keyboard.rightArrowKey.wasPressedThisFrame)
                            Next();

                        if (keyboard.enterKey.wasPressedThisFrame && manager.TryConfirmGimmick())
                            break;
                    }

                    yield return null;
                }
            }
            finally
            {
                playerInput.OnPrevKeyPressed -= Previous;
                playerInput.OnNextKeyPressed -= Next;
                manager.HideGimmickUI();
            }
        }
    }
}
