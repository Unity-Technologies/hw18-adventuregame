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
        Action m_DialogueEnd;

        public void StartDialogue(SerializableDialogData dialogue, Action onDialogueEnd = null)
        {
            if (m_CurrentDialogue == null)
            {
                Debug.Log("Dialogue Start");
                m_CurrentDialogue = dialogue;
                int startIndex = 0;
                for (int i = 0; i < dialogue.m_dialogNodes.Count; ++i)
                {
                    if (dialogue.m_dialogNodes[i].m_title == "START")
                    {
                        startIndex = i;
                        break;
                    }
                }
                m_CurrentDialogueNode = dialogue.m_dialogNodes[startIndex];
                ContinueDialogue();
            }
        }

        void ContinueDialogue()
        {
            ContinueDialogue(0);
        }

        void ContinueDialogue(int selection)
        {
            Debug.Log("Dialogue Advance");
            int outputsCount = m_CurrentDialogueNode.m_outputs.Count;
            if (outputsCount > 0 && selection < outputsCount)
            {
                m_CurrentDialogueNode = m_CurrentDialogue.m_dialogNodes[m_CurrentDialogueNode.m_outputs[selection].m_targetNode];
                if (outputsCount > 1)
                {
                    var dialogueChoices = new string[outputsCount];
                    for (int i = 0; i < outputsCount; i++)
                    {
                        dialogueChoices[i] = m_CurrentDialogueNode.m_outputDialogs[i];
                    }
                    AdventureGameOverlayManager.Instance.DisplayDialogueOptions(dialogueChoices, m_CurrentDialogueNode.m_characterDialogue, ContinueDialogue);
                }
                else
                {
                    AdventureGameOverlayManager.Instance.DisplayCharacterDialogue(m_CurrentDialogueNode.m_characterDialogue, m_CurrentDialogueNode.m_speakingCharacterName, ContinueDialogue);
                }
            }
            else
            {
                EndDialogue();
            }
        }

        void EndDialogue()
        {
            Debug.Log("Dialogue End");
            m_CurrentDialogue = null;
            m_CurrentDialogueNode = null;
            if (m_DialogueEnd != null)
            {
                m_DialogueEnd();
            }
            m_DialogueEnd = null;
        }
    }
}
