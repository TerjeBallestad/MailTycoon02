using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapIndicator : MonoBehaviour, IMouseInteractable {
    public event Action<IMouseInteractable> OnMouseStartHover;
    public event Action OnMouseEndHover;

    private void Start () {
        OnMouseStartHover += GameManager.instance.HandleMouseOverStart;
        OnMouseEndHover += GameManager.instance.HandleMouseOverEnd;
    }

    private void OnMouseEnter () {
        OnMouseStartHover?.Invoke (this);
    }
    private void OnMouseExit () {
        if ((object) GameManager.instance.mouseInteractable == this) {
            OnMouseEndHover?.Invoke ();
        }
    }
    public void OnHover () {

    }
    public void OnClick () {
        GameManager.instance.toMove = gameObject;
    }
    public void OnClickHold () {

    }

    public void OnClickEnd () {

    }
    public void OnMouseHoverExit () {

    }

}