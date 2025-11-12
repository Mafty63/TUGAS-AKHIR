using System.Collections;
using CleverCrow.Fluid.Dialogues.Graphs;
using TMPro;
using UnityEngine;
using CleverCrow.Fluid.Dialogues;
using CleverCrow.Fluid.Databases;
using System;

namespace NoName.CoreMechanic
{
    public class DialoguePlayer : Singleton<DialoguePlayer>
    {
        private DialogueController _ctrl;
        [SerializeField] private float delayAfterDub = 1;
        [SerializeField] private float delayIfNoDub = 5;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI lines1;
        [SerializeField] private TextMeshProUGUI lines2;

        private bool dialogueIsPlaying;
        protected override void Awake()
        {
            base.Awake();
            var database = new DatabaseInstance();
            _ctrl = new DialogueController(database);
        }

        public void PlayDialogue(DialogueGraph dialogue, Action callback = null)
        {
            dialoguePanel.SetActive(true);

            _ctrl.Events.SpeakWithAudio.AddListener((actor, text, audioClip) =>
            {
                lines1.text = text;

                dialogueIsPlaying = true;
                if (audioClip != null)
                {
                    // AudioManager.Instance.PlayDub(audioClip);

                    StartCoroutine(NextDialogue());
                }
                else
                {
                    StartCoroutine(DelayNextDialogue());
                }

            });

            _ctrl.Events.End.AddListener(() =>
            {
                StartCoroutine(EndDialogue(callback));
            });

            _ctrl.Play(dialogue);
        }

        private IEnumerator NextDialogue()
        {
            // yield return new WaitUntil(() => !AudioManager.Instance.DubIsPlaying());
            yield return new WaitForSeconds(delayAfterDub);

            Debug.Log("next line");
            _ctrl.Next();
        }

        private IEnumerator DelayNextDialogue()
        {
            yield return new WaitForSeconds(delayIfNoDub);
            _ctrl.Next();
        }

        private IEnumerator EndDialogue(Action action)
        {
            // yield return new WaitUntil(() => !AudioManager.Instance.DubIsPlaying());
            yield return new WaitForSeconds(delayAfterDub);

            dialoguePanel.SetActive(false);
            Debug.Log("end dialogue");
            _ctrl.Events.SpeakWithAudio.RemoveAllListeners();
            _ctrl.Events.End.RemoveAllListeners();
            dialogueIsPlaying = false;
            action?.Invoke();
        }

        public void ForceStop()
        {
            if (!dialogueIsPlaying) return;
            _ctrl.Stop();

            dialoguePanel.SetActive(false);
        }

        private void Update()
        {
            _ctrl.Tick();
        }

    }
}