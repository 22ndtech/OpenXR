using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ndtech.Input;

public class SetTextForSpeech : MonoBehaviour {
  [SerializeField] GameObject ttsGameObject;
  public void OnUpdateSpeech() {
    ClickHandler clickHandlerScript = ttsGameObject.GetComponent<ClickHandler>();
    Debug.Log("clickHandlerScript.upEvent.GetPersistentMethodName(0) = " + clickHandlerScript.upEvent.GetPersistentMethodName(0));
  }
}
