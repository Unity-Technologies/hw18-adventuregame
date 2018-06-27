using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.AdventureGame
{
    public enum TriggerEventType
    {
        SCENE_TRANSITION,
        TRAP,
        EVENT,
        SCRIPT,
        OTHER,
    }

    public class Triggerable : MonoBehaviour
    {

         public void OnTriggered(TriggerEventType type)
        {
            switch (type)
            {
                case TriggerEventType.SCENE_TRANSITION :
                    Debug.Log("Triggered a SCENE_TRANSITION");
                    SceneManager.Instance.TriggerDoorway();
                    break;
                case TriggerEventType.TRAP :
                    Debug.Log("Triggered a TRAP");
                    break;
                case TriggerEventType.EVENT :
                    Debug.Log("Triggered an EVENT");
                    break;
                case TriggerEventType.SCRIPT :
                    Debug.Log("Triggered a SCRIPT");
                    break;
                case TriggerEventType.OTHER :
                    Debug.Log("Triggered OTHER");
                    break;
                default : break;
            }           
        }
    }
}