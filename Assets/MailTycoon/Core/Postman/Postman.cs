using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Postman : MonoBehaviour {
    [HideInInspector] public PostalArea postalArea;
    [HideInInspector] public Mail MailToPickUp, MailToDeliver;
    public List<Mail> MailInBag, MailToBePickedUp;
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
    Household deliveryDestination;

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

    // private void OnTriggerEnter2D (Collider2D other) {
    //     Mail mail = other.GetComponent<Mail> ();
    //     Household house = other.GetComponent<Household> ();
    //     if (mail) {
    //         OnMailPickup?.Invoke (mail, this);
    //         MailInBag.Add (mail);
    //         mail.Assign (this);
    //         Destroy (mail.GetComponent<Collider2D> ());
    //     }
    //     if (house == deliveryDestination) {

    //     }
    // }

    // private void OnTriggerStay2D (Collider2D other) {
    //     if (MailToPickUp) {
    //         if (other.gameObject == MailToPickUp.gameObject) {
    //             OnMailPickup?.Invoke (MailToPickUp, this);
    //         }
    //     }
    //     if (MailToDeliver) {
    //         if (other.gameObject == MailToDeliver.Recipient.gameObject) {
    //             OnMailDelivered?.Invoke (MailToDeliver, this);
    //         }
    //     }
    // }

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
        yield return new WaitForSeconds (1f);

        if (MailInBag.Count > 0) {
            Debug.Log ("putting in some mail");
            AssignedTerminal.MailInTerminal.AddRange (MailInBag);
            MailInBag.Clear ();
        }
        // yield return new WaitUntil (() => AssignedTerminal.MailInTerminal.Count > 0);
        StartCoroutine (UploadMail (AssignedTerminal.GetMailToDeliver (this)));
    }

    IEnumerator UploadMail (List<Mail> mailList) {
        foreach (var mail in mailList) {
            Debug.Log ("uploading mail");
            MailInBag.Add (mail);
            yield return new WaitForSeconds (0.1f);
        }
        StartCoroutine (DeliverMailLoop ());
    }
    IEnumerator DeliverMailLoop () {
        List<Mail> route = new List<Mail> (MailInBag);
        MailInBag.Clear ();
        Debug.Log ("starting a delivery");
        foreach (var house in AssignedHouses) {
            foreach (var mail in route) {
                if (mail.Recipient == house) {
                    Debug.Log ("Delivering");
                    Movement.SetDestination (house.transform.position);
                    yield return new WaitUntil (() => Movement.AtDestination ());
                    route.Remove (mail);
                    Destroy (mail);
                }
            }
            if (house.mailToSend.Count > 0) {
                Debug.Log ("Picking up");
                Movement.SetDestination (house.transform.position);
                yield return new WaitUntil (() => Movement.AtDestination ());
                foreach (var mail in house.mailToSend) {
                    mail.Assign (this);
                }
                house.mailToSend.Clear ();
            }
        }
        yield return new WaitForSeconds (1f);
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