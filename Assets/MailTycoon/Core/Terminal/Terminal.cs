using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Terminal : MonoBehaviour, IMouseInteractable {
    [HideInInspector] public Household HouseholdAtLot;
    public List<Postman> Postmen;
    public List<Household> Households;
    public List<Mail> MailInTerminal;
    [HideInInspector] public Dictionary<Postman, List<Household>> Routes;
    [HideInInspector] public Dictionary<Postman, List<Mail>> MailToBePickedUp;
    [HideInInspector] public PostalArea Area;
    [HideInInspector] public event Action<IMouseInteractable> OnMouseStartHover;
    [HideInInspector] public event Action OnMouseEndHover;
    bool updateRoutes = false;
    public bool ShouldUpdatePostalRoutes = true;

    private void Start () {
        OnMouseStartHover += GameManager.instance.HandleMouseOverStart;
        OnMouseEndHover += GameManager.instance.HandleMouseOverEnd;
        MailToBePickedUp = new Dictionary<Postman, List<Mail>> ();
    }

    private void Update () {
        if (updateRoutes) {
            UpdatePostalRoutes ();
            UpdatePostalRoutesVisual ();
        }
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

        if (ShouldUpdatePostalRoutes) {
            ShowPostalRouteVisual ();
            ShouldUpdatePostalRoutes = !ShouldUpdatePostalRoutes;
        } else {
            DontShowPostalRouteVisual ();
            ShouldUpdatePostalRoutes = !ShouldUpdatePostalRoutes;
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

    public List<Mail> GetMailToDeliver (Postman postman) {
        List<Mail> mailList = new List<Mail> ();

        foreach (var mail in MailInTerminal) {
            if (mail.Recipient.AssignedPostman == postman) {
                mailList.Add (mail);
            }
        }
        MailInTerminal.RemoveAll (mail => mailList.Contains (mail));
        return mailList;
    }

}