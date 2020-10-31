using System;
using UnityEngine;

public interface IMouseInteractable {
    void OnHover ();
    void OnClickStart ();
    void OnClickHold ();
    void OnClickEnd ();
    void OnMouseHoverExit ();
    event Action<IMouseInteractable> OnMouseStartHover;
    event Action OnMouseEndHover;

}