using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextSpeech;
using UnityEngine.UI;
using UnityEngine.Android;

public class VoiceController : MonoBehaviour {
  [SerializeField] Text uiText;

  const string LANG_CODE = "en-US";

  private void Start() {
    Setup(LANG_CODE);

#if UNITY_ANDROID
    SpeechToText.instance.onPartialResultsCallback = OnPartialSpeechResult;
#endif
    SpeechToText.instance.onResultCallback = OnFinalSpeechResult;

    TextToSpeech.instance.onStartCallBack = OnSpeakStart;
    TextToSpeech.instance.onDoneCallback = OnSpeakStop;

#if UNITY_ANDROID
    CheckPermission();
#endif
  }

  void CheckPermission() {
    if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)) {
      Permission.RequestUserPermission(Permission.Microphone);
    }
  }

 #region Text to Speech
  public void StartSpeaking(string message) {
    TextToSpeech.instance.StartSpeak(message);
  }

  public void StartSpeaking(GameObject gameObject) {
    Text text = gameObject.GetComponent(typeof(Text)) as Text;

    uiText.text = "Starting to speak";

    if(text != null) {
      StartSpeaking(text.text);
    }
  }

  public void StopSpeaking() {
    TextToSpeech.instance.StopSpeak();
    Debug.Log("VoiceController:StopSpeaking");
  }

  void OnSpeakStart() {
    Debug.Log("VoiceControl:OnSpeakStart");
  }
  void OnSpeakStop() {
    Debug.Log("VoiceControl:OnSpeakStop");
  }
#endregion

 #region Speech to Text
  public void StartListening() {
    Debug.Log("StartListening");
    SpeechToText.instance.StartRecording();
    uiText.text = "Listening...";
  }
  public void StopListening() {
    Debug.Log("VoiceControl:StopListening");
    uiText.text = "Ready";
    SpeechToText.instance.StopRecording();
  }
  void OnFinalSpeechResult(string result) {
    Debug.Log("VoiceControl:OnFinalSpeechResult");
    uiText.text = "Got final result:\n\n" + result;
  }
  void OnPartialSpeechResult(string partialResult) {
    Debug.Log("VoiceControl:OnPartialSpeechResult");
    uiText.text = "Got partial result:\n\n" + partialResult;
  }
#endregion




  void Setup(string languageCode) {
    TextToSpeech.instance.Setting(languageCode, 1, 1);
    SpeechToText.instance.Setting(languageCode);
  }
}
