using System;
using UnityEngine;
using Yarn.Unity;

namespace Game
{
    public class DialogueRunnerDispatcher : DialogueViewBase
    {
        public Action onDialoqueStarted;
        public Action<LocalizedLine> onRunLine;
        public Action<LocalizedLine> onInterruptLine;
        public Action onDismissLine;
        public Action<DialogueOption[]> onRunOptions;
        public Action onDialoqueComplete;

        public override void DialogueStarted()
        {
            onDialoqueStarted?.Invoke();
        }

        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            base.RunLine(dialogueLine, onDialogueLineFinished);
            onRunLine?.Invoke(dialogueLine);
        }

        public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            base.InterruptLine(dialogueLine, onDialogueLineFinished);
            onInterruptLine?.Invoke(dialogueLine);
        }

        public override void DismissLine(Action onDismissalComplete)
        {
            base.DismissLine(onDismissalComplete);
            onDismissLine?.Invoke();
        }

        public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
        {
            base.RunOptions(dialogueOptions, onOptionSelected);
            onRunOptions?.Invoke(dialogueOptions);
        }

        public override void DialogueComplete()
        {
            base.DialogueComplete();
            onDialoqueComplete?.Invoke();
        }

        public override void UserRequestedViewAdvancement()
        {
            base.UserRequestedViewAdvancement();
        }

        [YarnCommand("jumpTo")]
        public void JumpTo(string text)
        {
            Debug.Log("From A");
        }

    }
}