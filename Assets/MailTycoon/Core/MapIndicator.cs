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
    public void OnClickStart () {
        GameManager.instance.MovingIdicator = this;
    }
    public void OnClickHold () {

    }

    public void OnClickEnd () {

    }
    public void OnMouseHoverExit () {

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
}