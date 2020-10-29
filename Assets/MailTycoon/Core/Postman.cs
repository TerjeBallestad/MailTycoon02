using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Postman : MonoBehaviour {
    [HideInInspector] public PostalArea postalArea;
    [HideInInspector] public Mail MailToPickUp, MailToDeliver;
    [HideInInspector] public List<Mail> MailInBag, MailToBePickedUp;
    [HideInInspector] public IMovePosition Movement;
    [HideInInspector] public List<Household> AssignedHouses;
    [HideInInspector] public Terminal AssignedTerminal;
    [HideInInspector] public List<PostmanUppgrade> Uppgrades;
    [HideInInspector] public int Intellect = 16, Capacity = 40, Pollution;
    [HideInInspector] public float Speed = 1f;
    [HideInInspector] public Transform MapDivisionPosition;
    public event Action<Mail, Postman> OnMailPickup;
    public event Action<Mail, Postman> OnMailDelivered;

    private void Start () {
        Movement = GetComponent<IMovePosition> ();
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
        Movement.SetMovePosition (AssignedTerminal.transform.position);
        yield return new WaitUntil (() => AtTerminal ());
        Debug.Log ("at terminals");
        StartCoroutine (UploadMail (AssignedTerminal.GetMailToPickUp (this)));
    }

    IEnumerator UploadMail (List<Mail> mailList) {
        foreach (var mail in mailList) {
            MailInBag.Add (mail);
            yield return new WaitForSeconds (0.1f);
        }
    }

    bool AtTerminal () {
        if ((transform.position - AssignedTerminal.transform.position).sqrMagnitude < 2f) {
            return true;
        }
        return false;
    }

    public enum DeliveryMode {
        adhoc,
        route,
    }

}