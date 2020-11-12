using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Linehaul : MonoBehaviour, IMouseInteractable {
    public List<Terminal> Route;
    public List<Mail> MailOnBoard;
    public bool DoDeliveries = true;
    public float speed = 1f;
    int terminalIndex = 0;
    IMovePosition Movement;
    public event Action<IMouseInteractable> OnMouseStartHover;
    public event Action OnMouseEndHover;
    private void Awake () {
        Movement = GetComponent<IMovePosition> ();
    }
    private void Start () {
        OnMouseStartHover += GameManager.instance.HandleMouseOverStart;
        OnMouseEndHover += GameManager.instance.HandleMouseOverEnd;
        Movement.SetSpeed (speed);
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

        UpdateLinesBetweenTerminals ();

        foreach (var terminal in GameManager.instance.AllTerminals) {
            MapIndicator MI = GameManager.instance.MapIndicatorPool.Get ();
            MI.ActivateTerminalRouteMarker (terminal);
        }
    }
    public void OnClickHold () {

    }
    public void OnClickEnd () {

    }
    public void OnMouseHoverExit () {

    }

    IEnumerator GoToNextTerminal () {
        if (terminalIndex + 1 >= Route.Count) {
            terminalIndex = 0;
        } else {
            terminalIndex++;
        }
        Terminal nexttTerminal = Route[terminalIndex];
        Movement.SetDestination (nexttTerminal.transform.position);
        yield return new WaitUntil (() => Movement.AtDestination ());
        yield return new WaitForSeconds (1);
        StartCoroutine (Unload (nexttTerminal));
    }

    IEnumerator Unload (Terminal terminal) {
        List<Mail> mailOnBoard = new List<Mail> (MailOnBoard);
        foreach (var mail in mailOnBoard) {
            if (mail.Recipient.AssignedTerminal == terminal) {
                //Unload
                yield return new WaitForSeconds (0.4f);
                MailOnBoard.Remove (mail);
                terminal.MailInTerminal.Add (mail);
                mail.transform.SetParent (terminal.transform);
            }
        }
        StartCoroutine (Upload (terminal));
    }

    IEnumerator Upload (Terminal terminal) {
        List<Mail> mailInTerminal = new List<Mail> (terminal.MailInTerminal);
        foreach (var mail in mailInTerminal) {
            if (mail.Recipient.AssignedTerminal != terminal) {
                yield return new WaitForSeconds (0.5f);
                MailOnBoard.Add (mail);
                terminal.MailInTerminal.Remove (mail);
                mail.transform.SetParent (transform);
            }
        }
        StartCoroutine (GoToNextTerminal ());
    }

    void UpdateLinesBetweenTerminals () {
        Vector3[] positions = new Vector3[Route.Count];
        for (int i = 0; i < Route.Count; i++) {
            positions[i] = Route[i].transform.position;
        }
        GameObject newGO = new GameObject ("Line collider");
        LineRenderer lr = GetComponent<LineRenderer> ();

        MeshCollider meshCollider = newGO.AddComponent<MeshCollider> ();

        lr.SetPositions (positions);
        Mesh mesh = new Mesh ();
        lr.BakeMesh (mesh, true);
        meshCollider.sharedMesh = mesh;

    }
}