using System;
using _01.Scripts.TAE.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public sealed class DialogueActionMenu : MonoBehaviour
{
    [Serializable]
    private struct Choice
    {
        public string label;
        [TextArea] public string response;
    }

    [SerializeField] private TextBarUI dialogue;
    [SerializeField] private GameObject returnFocus;
    [SerializeField, TextArea] private string prompt;
    [SerializeField] private Choice[] choices = Array.Empty<Choice>();

    private string previousText;
    private int selectedChoice = -1;
    private int openedFrame;

    public bool IsOpen => selectedChoice >= 0;

    public void Open()
    {
        if (IsOpen || !isActiveAndEnabled || dialogue == null || dialogue.IsDestroyed
            || choices.Length == 0) return;

        selectedChoice = 0;
        openedFrame = Time.frameCount;
        previousText = dialogue.Text;
        ShowChoices();
        EventSystem.current?.SetSelectedGameObject(gameObject);
    }

    public void Close(bool restoreFocus = true)
    {
        if (!IsOpen) return;

        selectedChoice = -1;
        if (dialogue != null)
        {
            dialogue.Text = previousText;
            dialogue.ResumeMessages();
        }
        if (restoreFocus && returnFocus != null && returnFocus.activeInHierarchy)
            EventSystem.current?.SetSelectedGameObject(returnFocus);
    }

    private void OnDisable() => Close(restoreFocus: false);

    private void LateUpdate()
    {
        if (!IsOpen || Time.frameCount == openedFrame) return;

        var events = EventSystem.current;
        if (events == null || events.currentInputModule is not InputSystemUIInputModule input) return;

        if (input.cancel?.action?.WasPerformedThisFrame() == true)
        {
            Close();
            return;
        }

        if (input.submit?.action?.WasPerformedThisFrame() == true)
        {
            Choose();
            return;
        }

        var move = input.move?.action;
        if (move != null && move.WasPerformedThisFrame()) Move(move.ReadValue<Vector2>());
    }

    private void Move(Vector2 direction)
    {
        int next = selectedChoice;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            next = selectedChoice / 2 * 2 + (direction.x > 0f ? 1 : 0);
        else if (direction.y != 0f)
            next += direction.y > 0f ? -2 : 2;

        if (next == selectedChoice || next < 0 || next >= choices.Length) return;

        selectedChoice = next;
        ShowChoices();
    }

    private void Choose()
    {
        string response = choices[selectedChoice].response;
        Close(restoreFocus: false);
        if (!string.IsNullOrWhiteSpace(response)) dialogue.Text = response;
        TurnManager.Instance.EndTurn();
    }

    private void ShowChoices()
    {
        string text = prompt ?? string.Empty;
        for (int i = 0; i < choices.Length; i++)
        {
            text += i % 2 == 0 ? "\n" : "<pos=50%>";
            text += i == selectedChoice ? "> " : "   ";
            text += choices[i].label;
        }
        dialogue.Text = text;
    }
}
