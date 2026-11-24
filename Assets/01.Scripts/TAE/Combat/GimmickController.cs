using System.Collections;
using _01.Scripts.TAE.Managers;
using _01.Scripts.TAE.Players;
using _01.Scripts.TAE.Data;
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
            yield return null;

            while (true)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.leftArrowKey.wasPressedThisFrame)
                        manager.Prev();
                    else if (keyboard.rightArrowKey.wasPressedThisFrame)
                        manager.Next();

                    if (keyboard.escapeKey.wasPressedThisFrame || keyboard.cKey.wasPressedThisFrame)
                    {
                        playerInput.OnPrevKeyPressed -= manager.Prev;
                        playerInput.OnNextKeyPressed -= manager.Next;
                        manager.ClearSelection();
                        GameManager.Instance.TimeStop(false);
                        yield break;
                    }

                    if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                        break;
                }

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
            var placement = GimmickManager.Instance;
            UIDataSO pendingLaunch = null;
            bool launchRight = true;
            manager.ShowGimmickUI(GimmickManager.Instance.CurrentPointPos());

            void Previous() => Move(-1);
            void Next() => Move(1);

            void Move(int direction)
            {
                if (pendingLaunch == null)
                {
                    manager.MoveGimmickSelection(direction);
                    return;
                }

                launchRight = direction > 0;
                manager.ShowGimmickDirection(launchRight);
            }

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

                        if (keyboard.escapeKey.wasPressedThisFrame || keyboard.cKey.wasPressedThisFrame)
                            break;

                        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                        {
                            if (pendingLaunch != null)
                            {
                                if (placement.TryPlace(pendingLaunch, launchRight))
                                    break;
                            }
                            else
                            {
                                UIDataSO selected = manager.GetSelectedGimmick();
                                if (selected != null && selected.GimmickPrefab is LaunchGimmick)
                                {
                                    pendingLaunch = selected;
                                    manager.ShowGimmickDirection(launchRight);
                                }
                                else if (placement.TryPlace(selected))
                                {
                                    break;
                                }
                            }
                        }
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
