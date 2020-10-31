using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
    public GameObject mailPrefab, postmanPrefab, householdPrefab, postOfficePrefab, mapIndicatorPrefab;
    [HideInInspector] public static GameManager instance;
    [HideInInspector] public PostalArea selectedArea;
    [HideInInspector] public IMouseInteractable mouseInteractable;
    [HideInInspector] public event Action SpawnPostman;

    public List<Color> MediumColors, LightColors;
    public GameObject toMove;

    private void Awake () {
        instance = this;
    }

    void Update () {
        if (toMove) {
            Terminal closesTerminal = null;
            float shortestDistance = 1000f;
            foreach (var terminal in selectedArea.terminals) {
                float distance = (terminal.transform.position - toMove.transform.position).sqrMagnitude;
                if (distance < shortestDistance) {
                    closesTerminal = terminal;
                    shortestDistance = distance;

                }
            }
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay (mouse);
            RaycastHit hit;
            if (closesTerminal) {
                closesTerminal.AssignHousesToPostmen ();
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
                if (mouseInteractable != null) {
                    mouseInteractable.OnClickEnd ();
                }
            }
            if (Input.GetMouseButton (0)) {
                if (mouseInteractable != null) {
                    mouseInteractable.OnClickHold ();
                }
            }
        }
    }
    public void HandleSpawnPostman () {
        SpawnPostman?.Invoke ();
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