using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Linehaul : MonoBehaviour {
    public List<Terminal> Route;
    public List<Mail> MailOnBoard;
    public bool DoDeliveries = true;
    public float speed = 1f;
    int terminalIndex = 0;
    IMovePosition Movement;
    private void Awake () {
        Movement = GetComponent<IMovePosition> ();
    }
    private void Start () {
        Movement.SetSpeed (speed);

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
}