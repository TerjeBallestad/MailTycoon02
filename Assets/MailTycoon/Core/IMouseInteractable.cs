using System;
using UnityEngine;

public interface IMouseInteractable {
    void OnHover ();
    void OnClick ();
    void OnClickHold ();
    void OnClickEnd ();
    void OnMouseHoverExit ();
    event Action<IMouseInteractable> OnMouseStartHover;
    event Action OnMouseEndHover;

}