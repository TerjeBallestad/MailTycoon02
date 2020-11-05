using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
    public GameObject mailPrefab, postmanPrefab, householdPrefab, postOfficePrefab, mapIndicatorPrefab, linehaulPrefab;
    [HideInInspector] public static GameManager instance;
    [HideInInspector] public PostalArea selectedArea;
    [HideInInspector] public IMouseInteractable mouseInteractable;
    [HideInInspector] public event Action ShowAdjustmentLayer, DontShowAdjustmentLayer;
    [HideInInspector] public MapIndicator MovingIdicator;
    public List<Color> MediumColors, LightColors;
    public MailPool MailPool;

    private void Awake () {
        instance = this;
        MailPool = gameObject.AddComponent<MailPool> ();
        MailPool.prefab = mailPrefab.GetComponent<Mail> ();
    }
    private void Start () {

    }

    void Update () {
        if (MovingIdicator) {
            Terminal closesTerminal = MovingIdicator.GetClosestHousehold ().AssignedTerminal;
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay (mouse);
            RaycastHit hit;
            // if (closesTerminal) {
            //     closesTerminal.AssignHousesToPostmen ();
            // }
            if (Physics.Raycast (castPoint, out hit, Mathf.Infinity)) {
                MovingIdicator.transform.position = new Vector3 (hit.point.x, hit.point.y, -1f);
            }
            selectedArea.ShowPostalRoutes (MovingIdicator);
        }

        if (!IsPointerOverUIElement () && mouseInteractable != null) {
            if (Input.GetMouseButtonDown (0)) {
                mouseInteractable.OnClickStart ();
            }
            if (Input.GetMouseButtonUp (0)) {
                MovingIdicator = null;
                mouseInteractable.OnClickEnd ();
            }
            if (Input.GetMouseButton (0)) {
                mouseInteractable.OnClickHold ();
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

    public void ToggleAdjustmentLayer (bool show) {
        if (show) {
            Debug.Log ("enabled");
            ShowAdjustmentLayer?.Invoke ();
        } else {
            DontShowAdjustmentLayer?.Invoke ();
            Debug.Log ("disabled");
        }
    }
}