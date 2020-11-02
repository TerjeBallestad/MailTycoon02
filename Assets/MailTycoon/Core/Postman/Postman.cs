using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Postman : MonoBehaviour {
    [HideInInspector] public PostalArea postalArea;
    [HideInInspector] public Mail MailToPickUp, MailToDeliver;
    [HideInInspector] public List<Mail> MailInBag, MailToBePickedUp;
    [HideInInspector] public IMovePosition Movement;
    public List<Household> AssignedHouses;
    [HideInInspector] public Terminal AssignedTerminal;
    [HideInInspector] public List<PostmanUppgrade> Uppgrades;
    [HideInInspector] public int Intellect = 16, Capacity = 40, Pollution;
    [HideInInspector] public float Speed = 1f;
    [HideInInspector] public MapIndicator MapIndicator;
    [HideInInspector] public bool PostalRoute = true;
    public event Action<Mail, Postman> OnMailPickup;
    public event Action<Mail, Postman> OnMailDelivered;

    private void Awake () {
        Movement = GetComponent<IMovePosition> ();
    }

    private void Start () {
        MailToBePickedUp = new List<Mail> ();
        MailInBag = new List<Mail> ();
        foreach (PostmanUppgrade uppgrade in Uppgrades) {
            Intellect += uppgrade.Intellect;
            Speed += uppgrade.Speed;
            Capacity += uppgrade.Capacity;
            Pollution += uppgrade.Pollution;
        }
        Movement.SetMoveSpeed (Speed);
    }

    private void OnTriggerEnter2D (Collider2D other) {
        Mail mail = other.GetComponent<Mail> ();
        if (mail) {
            OnMailPickup?.Invoke (mail, this);
        }
    }

    private void OnTriggerStay2D (Collider2D other) {

        if (MailToPickUp) {
            if (other.gameObject == MailToPickUp.gameObject) {
                OnMailPickup?.Invoke (MailToPickUp, this);
            }

        }
        if (MailToDeliver) {
            if (other.gameObject == MailToDeliver.Recipient.gameObject) {
                OnMailDelivered?.Invoke (MailToDeliver, this);
            }
        }
    }

    IEnumerator WaitForAssignment () {
        while (!MailToPickUp && !MailToDeliver) {
            yield return new WaitForSeconds (1);
            postalArea.GetNextPickupAssignment (this);
        }
    }
    IEnumerator ReturnToTerminal () {
        Debug.Log ("returningtoterminal" + AssignedTerminal);
        Movement.SetDestination (AssignedTerminal.transform.position);
        yield return new WaitUntil (() => Movement.AtDestination ());
        Debug.Log ("at terminals");
        if (MailInBag.Count > 0) {
            AssignedTerminal.MailInTerminal.AddRange (MailInBag);
            MailInBag.Clear ();
        }
        // yield return new WaitUntil (() => AssignedTerminal.MailInTerminal.Count > 0);
        StartCoroutine (UploadMail (AssignedTerminal.GetMailToPickUp (this)));
    }

    IEnumerator UploadMail (List<Mail> mailList) {
        foreach (var mail in mailList) {
            MailInBag.Add (mail);
            yield return new WaitForSeconds (0.1f);
        }
        StartCoroutine (DeliverMailLoop ());
    }
    IEnumerator DeliverMailLoop () {
        Queue<Household> route = new Queue<Household> (AssignedHouses);
        Debug.Log (route.Count);
        List<Mail> routeMail = new List<Mail> (MailInBag);
        MailInBag.Clear ();
        while (route.Count > 0) {
            Debug.Log ("starting a delivery");
            Household destination = null;
            Household house = route.Dequeue ();
            Debug.Log (house.mailToSend.Count);
            foreach (var mail in routeMail) {
                if (mail.Recipient == house) {
                    destination = house;
                    MailToDeliver = mail;
                    routeMail.Remove (mail);
                    break;
                }
            }
            if (house.mailToSend.Count > 0) {
                destination = house;
            }
            Debug.Log (destination);
            if (destination) {
                Movement.SetDestination (destination.transform.position);
            }
            Debug.Log ("Going to destination");
            yield return new WaitUntil (() => Movement.AtDestination ());
            Debug.Log ("At destination");
        }
        Debug.Log ("Returning to terminal");
        StartCoroutine (ReturnToTerminal ());
    }

    public void Assign (Terminal terminal) {
        if (AssignedTerminal != null) {
            OnMailDelivered -= AssignedTerminal.HandleMailDelivery;
            OnMailPickup -= AssignedTerminal.HandleMailPickup;
            // AssignedTerminal.MailToBePickedUp.Remove (this);
            AssignedTerminal.Postmen.Remove (this);
        }
        AssignedTerminal = terminal;
        // OnMailDelivered += AssignedTerminal.HandleMailDelivery;
        // OnMailPickup += AssignedTerminal.HandleMailPickup;
        // terminal.MailToBePickedUp.Add (this, new List<Mail> ());
        terminal.Postmen.Add (this);

    }
}