using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Terminal : MonoBehaviour, IMouseInteractable {
    [HideInInspector] public Household HouseholdAtLot;
    [HideInInspector] public List<Postman> Postmen;
    [HideInInspector] public List<Household> Households;
    public List<Mail> MailInTerminal;
    [HideInInspector] public Dictionary<Postman, List<Household>> Routes;
    [HideInInspector] public Dictionary<Postman, List<Mail>> MailToBePickedUp;
    [HideInInspector] public PostalArea Area;
    [HideInInspector] public event Action<IMouseInteractable> OnMouseStartHover;
    [HideInInspector] public event Action OnMouseEndHover;
    bool updateRoutes = false;
    bool showRoutes = true;

    private void Start () {
        OnMouseStartHover += GameManager.instance.HandleMouseOverStart;
        OnMouseEndHover += GameManager.instance.HandleMouseOverEnd;
        MailToBePickedUp = new Dictionary<Postman, List<Mail>> ();
    }

    private void Update () {
        if (updateRoutes)
            UpdatePostalRoutesVisual ();
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

        if (showRoutes) {
            ShowPostalRouteVisual ();
            showRoutes = !showRoutes;
        } else {
            DontShowPostalRouteVisual ();
            showRoutes = !showRoutes;
        }

    }
    public void OnClickHold () {

    }
    public void OnClickEnd () {

    }
    public void OnMouseHoverExit () {

    }

    public void ShowPostalRouteVisual () {

        foreach (var postman in Postmen) {
            postman.MapIndicator.gameObject.SetActive (true);
            foreach (var household in postman.AssignedHouses) {
                household.GetComponent<SpriteRenderer> ().color = GameManager.instance.LightColors[Postmen.IndexOf (postman) % GameManager.instance.LightColors.Count];
            }
        }
        updateRoutes = true;
    }

    void UpdatePostalRoutesVisual () {
        foreach (var postman in Postmen) {
            foreach (var household in postman.AssignedHouses) {
                household.GetComponent<SpriteRenderer> ().color = GameManager.instance.LightColors[Postmen.IndexOf (postman) % GameManager.instance.LightColors.Count];
            }
        }

    }

    public void DontShowPostalRouteVisual () {
        updateRoutes = false;
        foreach (var postman in Postmen) {
            postman.MapIndicator.gameObject.SetActive (false);
            foreach (var household in postman.AssignedHouses) {
                household.ResetToDefaultColor ();
            }
        }

    }
    public void UpdatePostalRoutes () {
        foreach (var household in Households) {
            float shortestDistance = 1000f;

            foreach (var postman in Postmen) {
                float distance = (household.transform.position - postman.MapIndicator.transform.position).sqrMagnitude;

                if (distance < shortestDistance) {
                    household.Assign (postman);
                    shortestDistance = distance;
                }
            }
        }
    }

    public void HandleMailPickup (Mail mail, Postman postman) {

        if (mail == postman.MailToPickUp) {
            if (postman.MailToBePickedUp.Count > 0) {
                postman.MailToPickUp = postman.MailToBePickedUp[0];
                postman.Movement.SetDestination (postman.MailToBePickedUp[0].transform.position);
                postman.MailToBePickedUp.RemoveAt (0);
            } else {
                postman.StartCoroutine ("ReturnToTerminal");
            }
        }

        if (mail == postman.MailToPickUp) {
            AssignDeliveryToPostman (mail, postman);
        } else {
            postman.MailInBag.Add (mail);
            mail.AssignedPostman = postman;
            mail.PickedUp = true;
            RemovePickupAssignment (mail, postman);
        }
        Destroy (mail.GetComponent<Collider2D> ());
    }

    public void HandleMailDelivery (Mail mail, Postman postman) {
        postman.MailToDeliver = null;

        if (postman.MailInBag.Count > 0) {
            AssignDeliveryToPostman (postman.MailInBag[0], postman);
            postman.MailInBag.RemoveAt (0);

        } else {

            postman.MailToBePickedUp = postman.AssignedTerminal.GetMailToPickUp (postman);

        }

        Destroy (mail.gameObject);
    }

    public void HandleMailNotInTime (Mail mail, Postman postman) {
        if (postman) {
            postman.MailToDeliver = null;
            postman.MailToPickUp = null;
            postman.StartCoroutine ("WaitForAssignment");
        }
        RemovePickupAssignment (mail, postman);

        Debug.Log ("GAME OVER");
        Destroy (mail.gameObject);
    }

    public void HandleMailSpawn (Household house) {
        Mail mail = Instantiate (GameManager.instance.mailPrefab).GetComponent<Mail> ();
        house.mailToSend.Add (mail);
        // MailToBePickedUp[house.AssignedPostman].Add (mail);
        mail.SenderArea = Area;
        Household recipientHouse = Households.ElementAt (UnityEngine.Random.Range (0, Households.Count));
        while (recipientHouse.Inhabitants == 0) {
            recipientHouse = Households.ElementAt (UnityEngine.Random.Range (0, Households.Count));
        }
        mail.Recipient = recipientHouse;
        mail.OnMailNotDeliveredInTime += HandleMailNotInTime;
        mail.transform.position = house.transform.position + new Vector3 (UnityEngine.Random.Range (-0.02f, 0.02f), UnityEngine.Random.Range (-0.02f, 0.02f), 0);
    }

    public List<Mail> GetMailToDeliver (Postman postman) {
        List<Mail> mailList = new List<Mail> ();

        foreach (var mail in MailInTerminal) {
            if (mail.Recipient.AssignedPostman == postman) {
                mailList.Add (mail);
            }
        }
        return mailList;
    }
    public List<Mail> GetMailToPickUp (Postman postman) {
        List<Mail> mail = new List<Mail> ();

        foreach (var house in postman.AssignedHouses) {

            mail.AddRange (house.mailToSend);
            house.mailToSend.Clear ();
        }

        return mail;
    }
    void AssignPickupToPostman (Mail mail, Postman postman) {
        if (MailToBePickedUp[postman].Contains (mail)) {
            MailToBePickedUp[postman].Remove (mail);
        }
        postman.MailToPickUp = mail;
        mail.AssignedPostman = postman;
        postman.Movement.SetDestination (mail.transform.position);
    }
    void AssignDeliveryToPostman (Mail mail, Postman postman) {
        postman.Movement.SetDestination (mail.Recipient.transform.position);
        postman.MailToDeliver = mail;
        postman.MailToPickUp = null;
        mail.PickedUp = true;
    }
    public void RemovePickupAssignment (Mail mail, Postman postman) {
        if (MailToBePickedUp[postman].Contains (mail)) {
            MailToBePickedUp[postman].Remove (mail);
        }
    }

}