using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.AdventureGame
{
    public class DialogueManager : MonoBehaviour
    {
        static DialogueManager s_Instance;

        public static DialogueManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<DialogueManager>();
                    if (s_Instance == null)
                    {
                        GameObject manager = new GameObject();
                        s_Instance = manager.AddComponent<DialogueManager>();
                    }
                }
                return s_Instance;
            }
        }

        SerializableDialogData m_CurrentDialogue;
        SerializableDialogData.SerializableDialogNode m_CurrentDialogueNode;

        public void StartDialogue(SerializableDialogData dialogue)
        {
            if (m_CurrentDialogue != null)
            {
                m_CurrentDialogue = dialogue;
                m_CurrentDialogueNode = dialogue.m_dialogNodes[0];
                ContinueDialogue(0);
            }
        }

        void ContinueDialogue(int selection)
        {
            int outputsCount = m_CurrentDialogueNode.m_outputs.Count;
            if (outputsCount > 0 && selection < outputsCount)
            {
                m_CurrentDialogueNode = m_CurrentDialogue.m_dialogNodes[m_CurrentDialogueNode.m_outputs[selection].m_targetNode];
                string[] dialogueChoices = null;
                if (outputsCount > 1)
                {
                    dialogueChoices = new string[outputsCount];
                    for (int i = 0; i < outputsCount; i++)
                    {
                        dialogueChoices[i] = m_CurrentDialogueNode.m_outputs[i].m_outputDialog;
                    }
                }
                AdventureGameOverlayManager.Instance.DisplayDialogueOptions(dialogueChoices, m_CurrentDialogueNode.m_characterDialogue, ContinueDialogue);
            }
            else
            {
                EndDialogue();
            }
        }

        void EndDialogue()
        {
            m_CurrentDialogue = null;
            m_CurrentDialogueNode = null;
        }
    }
}
