﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Postman : MonoBehaviour {
    [HideInInspector] public PostalArea postalArea;
    [HideInInspector] public Mail MailToPickUp, MailToDeliver;
    public List<Mail> MailInBag, route;
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
        MailInBag = new List<Mail> ();
        foreach (PostmanUppgrade uppgrade in Uppgrades) {
            Intellect += uppgrade.Intellect;
            Speed += uppgrade.Speed;
            Capacity += uppgrade.Capacity;
            Pollution += uppgrade.Pollution;
        }
        Movement.SetSpeed (Speed);
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
        Movement.SetDestination (AssignedTerminal.transform.position);
        yield return new WaitUntil (() => Movement.AtDestination ());
        yield return new WaitForSeconds (1f);

        foreach (var mail in MailInBag) {
            mail.Assign (AssignedTerminal);
        }
        MailInBag.Clear ();
        // yield return new WaitUntil (() => AssignedTerminal.MailInTerminal.Count > 0);
        StartCoroutine (UploadMail (AssignedTerminal.GetMailToDeliver (this)));
    }

    IEnumerator UploadMail (List<Mail> mailList) {
        foreach (var mail in mailList) {
            mail.Assign (this);
            yield return new WaitForSeconds (1f);
        }
        StartCoroutine (DeliverMailLoop ());
    }
    IEnumerator DeliverMailLoop () {
        List<Mail> delivered = new List<Mail> ();
        foreach (var mail in MailInBag) {
            route.Add (mail);
        }
        MailInBag.Clear ();
        foreach (var house in AssignedHouses) {
            foreach (var mail in route) {
                if (mail.Recipient == house) {
                    Movement.SetDestination (house.transform.position);
                    yield return new WaitUntil (() => Movement.AtDestination ());
                    yield return new WaitForSeconds (1f);
                    delivered.Add (mail);
                }
            }
            route.RemoveAll (mail => delivered.Contains (mail));
            foreach (var mail in delivered) {
                GameManager.instance.MailPool.ReturnToPool (mail);
            }
            delivered.Clear ();
            if (house.MailToSend.Count > 0) {
                Movement.SetDestination (house.transform.position);
                yield return new WaitUntil (() => Movement.AtDestination ());
                foreach (var mail in house.MailToSend) {
                    mail.Assign (this);
                }
                house.MailToSend.Clear ();
            }
        }

        route.Clear ();
        yield return new WaitForSeconds (1f);
        StartCoroutine (ReturnToTerminal ());
    }

    public void Assign (Terminal terminal) {
        if (AssignedTerminal != null) {
            // OnMailDelivered -= AssignedTerminal.HandleMailDelivery;
            // OnMailPickup -= AssignedTerminal.HandleMailPickup;
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