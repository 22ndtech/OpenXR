using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ClickHandler : MonoBehaviour
{
  public UnityEvent upEvent;
  public UnityEvent downEvent;
  public string messageText;

  void OnMouseDown() {
    Debug.Log("Down : messageText = " + messageText);
    downEvent?.Invoke();
  }

  void OnMouseUp() {
    Debug.Log("Up : messageText = " + messageText);
    upEvent?.Invoke();
  }
}
