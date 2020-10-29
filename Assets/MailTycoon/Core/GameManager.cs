﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
    public GameObject mailPrefab, postmanPrefab, householdPrefab, postOfficePrefab, mapIndicatorPrefab;
    [HideInInspector] public static GameManager instance;
    [HideInInspector] public PostalArea selectedArea;
    [HideInInspector] public IMouseInteractable mouseInteractable;
    public List<Color> MediumColors, LightColors;
    public GameObject toMove;

    private void Awake () {
        instance = this;
    }

    void Update () {
        if (toMove) {
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay (mouse);
            RaycastHit hit;
            if (selectedArea) {
                selectedArea.CreatePostRoutes ();
            }
            if (Physics.Raycast (castPoint, out hit, Mathf.Infinity)) {
                toMove.transform.position = new Vector3 (hit.point.x, hit.point.y, -1f);
            }
        }

        if (!IsPointerOverUIElement ()) {
            if (Input.GetMouseButtonDown (0)) {
                if (mouseInteractable != null) {
                    mouseInteractable.OnClick ();
                }
            }
            if (Input.GetMouseButtonUp (0)) {
                toMove = null;

            }
            if (Input.GetMouseButton (0)) {
                if (mouseInteractable != null) {
                    mouseInteractable.OnClickHold ();
                }
            }
        }
    }

    public void HandleMouseOverEnd () {
        mouseInteractable = null;
    }
    public void HandleMouseOverStart (IMouseInteractable interactable) {
        mouseInteractable = interactable;
        Debug.Log (interactable);
    }

    public static bool IsPointerOverUIElement () {
        var eventData = new PointerEventData (EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new List<RaycastResult> ();
        EventSystem.current.RaycastAll (eventData, results);
        return results.Count > 0;
    }
}