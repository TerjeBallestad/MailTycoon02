using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapIndicator : MonoBehaviour, IMouseInteractable {
    public event Action<IMouseInteractable> OnMouseStartHover;
    public event Action OnMouseEndHover;
    public Sprite PostmanSprite, TerminalSprite, Box;
    Mode mode;

    private void Start () {
        mode = Mode.None;
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
    public void OnClickStart () {
        GameManager.instance.MovingIdicator = this;
    }
    public void OnClickHold () {
        switch (mode) {

            case Mode.None:
                break;

            case Mode.LinehaulRoute:

                break;

            case Mode.PostmanRoute:
                break;

            default:
                break;
        }
    }

    public void OnClickEnd () {

    }
    public void OnMouseHoverExit () {

    }

    public void ActivateTerminalRouteMarker (Terminal terminal) {
        SpriteRenderer sr = GetComponent<SpriteRenderer> ();
        sr.sprite = Box;
        sr.color = new Color (0.3719629f, 0.9433962f, 0.3070488f, 0.4627451f);
        transform.position = terminal.transform.position;
        mode = Mode.LinehaulRoute;
    }

    public void Deactivate () {

    }

    public Household GetClosestHousehold () {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll (transform.position, 1f);
        float shortestDistance = 1000f;
        Household nearestHouse = null;
        foreach (var collider in hitColliders) {
            Household household = collider.GetComponent<Household> ();
            if (household) {
                float distance = (household.transform.position - transform.position).sqrMagnitude;
                if (distance < shortestDistance) {
                    nearestHouse = household;
                    shortestDistance = distance;
                }
            }
        }
        return nearestHouse;
    }
    enum Mode {
        None,
        LinehaulRoute,
        PostmanRoute,
    }
}