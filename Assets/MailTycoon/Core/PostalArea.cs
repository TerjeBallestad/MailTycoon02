using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PostalArea : MonoBehaviour {

    [SerializeField] int gridWidth = 20;
    [SerializeField] int gridHeight = 20;
    [SerializeField] Sprite household1, household2, household3;
    // [SerializeField] int targetInhabitants;
    List<Mail> mailToBePickedUp;
    public List<Terminal> terminals;
    List<Postman> postmen;

    List<Household> households;
    new Collider collider;

    private void Start () {
        terminals = new List<Terminal> ();
        households = new List<Household> ();
        mailToBePickedUp = new List<Mail> ();
        postmen = new List<Postman> ();
        collider = GetComponent<Collider> ();
        GameManager.instance.selectedArea = this;

        SpawnHouseholds ();
        for (int i = 0; i < 5; i++) {

            SpawnPostman ();
        }

        for (int i = 0; i < 2; i++) {
            Household randomHouse = households.ElementAt (Random.Range (0, households.Count));
            SpawnTerminal (randomHouse);
        }

    }

    void SpawnHouseholds () {
        float xStep = (collider.bounds.max.x - collider.bounds.min.x) / (float) gridWidth;
        float yStep = (collider.bounds.max.y - collider.bounds.min.y) / (float) gridHeight;
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                float coordX = collider.bounds.min.x + x * xStep;
                float coordY = collider.bounds.min.y + y * yStep;

                Vector3 position = new Vector3 (coordX, coordY, collider.bounds.min.z - 0.01f);
                RaycastHit hit;

                if (Physics.Raycast (position, transform.TransformDirection (Vector3.forward), out hit, 5f)) {
                    if (hit.collider.gameObject == gameObject) {
                        float sample = Mathf.PerlinNoise (coordX, coordY);
                        Household house = Instantiate (GameManager.instance.householdPrefab).GetComponent<Household> ();
                        house.gameObject.name = "house";
                        house.OnMailSpawn += HandleMailSpawn;
                        SpriteRenderer rend = house.GetComponent<SpriteRenderer> ();

                        house.transform.SetParent (transform);
                        house.transform.position = position + new Vector3 (Random.Range (-0.02f, 0.02f), Random.Range (-0.02f, 0.02f), 0);
                        house.HouseColor = GameManager.instance.MediumColors[households.Count % GameManager.instance.MediumColors.Count];
                        house.ResetToDefaultColor ();

                        if (sample > 0.8f) {
                            rend.sprite = household3;
                        } else if (sample > 0.4f) {
                            rend.sprite = household2;
                        } else if (sample > 0.2f) {
                            rend.sprite = household1;
                        } else {
                            sample = 0;
                        }
                        house.PropertyValue = (int) (sample * 100);
                        house.Inhabitants = (int) (sample * 100);

                        households.Add (house);
                    }
                }
            }
        }
    }

    public void SpawnPostman () {
        Household randomHouse = households.ElementAt (Random.Range (0, households.Count));
        Postman postman = Instantiate (GameManager.instance.postmanPrefab, randomHouse.transform.position, Quaternion.identity).GetComponent<Postman> ();
        MapIndicator mapIndicator = Instantiate (GameManager.instance.mapIndicatorPrefab, new Vector3 (randomHouse.transform.position.x, randomHouse.transform.position.y, -1f), Quaternion.identity).GetComponent<MapIndicator> ();
        mapIndicator.GetComponent<SpriteRenderer> ().color = GameManager.instance.LightColors[postmen.Count];
        postman.MapIndicator = mapIndicator;
        mapIndicator.gameObject.SetActive (false);
        postmen.Add (postman);
        postman.postalArea = this;
        postman.gameObject.name = this.name + " postman";
        postman.OnMailPickup += HandleMailPickup;
        postman.OnMailDelivered += HandleMailDelivery;
        // postman.StartCoroutine ("WaitForAssignment");
    }

    public int DespawnPostman () {
        if (postmen.Count > 0) {
            Postman postmanToRemove = postmen[postmen.Count - 1];
            postmen.Remove (postmanToRemove);
            Destroy (postmanToRemove.gameObject);
        }
        return postmen.Count;
    }

    void SpawnTerminal (Household household) {
        Terminal terminal = Instantiate (GameManager.instance.postOfficePrefab, household.transform.position, Quaternion.identity).GetComponent<Terminal> ();
        terminal.HouseholdAtLot = household;
        household.gameObject.SetActive (false);
        terminal.Postmen = new List<Postman> (postmen);
        foreach (var postman in postmen) {
            postman.OnMailPickup -= HandleMailPickup;
            postman.OnMailDelivered -= HandleMailDelivery;
        }
        terminal.Area = this;
        terminals.Add (terminal);
        AssignHouseholdsToTerminals ();
        AssignPostmenToTerminals ();
    }

    public void AssignPostmenToTerminals () {
        foreach (var postman in postmen) {
            float shortestDistance = 1000f;
            foreach (var terminal in terminals) {
                float distance = (terminal.transform.position - postman.transform.position).sqrMagnitude;
                if (distance < shortestDistance) {
                    postman.Assign (terminal);
                    shortestDistance = distance;
                }
            }
        }
        foreach (var terminal in terminals) {
            terminal.UpdatePostalRoutes ();
        }
        foreach (var postman in postmen) {
            postman.StartCoroutine ("ReturnToTerminal");
        }
    }

    public void AssignHouseholdsToTerminals () {
        foreach (var household in households) {
            household.OnMailSpawn -= HandleMailSpawn;
            float shortestDistance = 1000f;
            foreach (var terminal in terminals) {
                float distance = (household.transform.position - terminal.transform.position).sqrMagnitude;
                if (distance < shortestDistance) {
                    household.Assign (terminal);
                    shortestDistance = distance;
                }
            }
        }
    }

    void HandleMailPickup (Mail mail, Postman postman) {

        if (mail.AssignedPostman != null && mail.AssignedPostman != postman) {
            mail.AssignedPostman.MailToDeliver = null;
            mail.AssignedPostman.MailToPickUp = null;
            mail.AssignedPostman.StartCoroutine ("WaitForAssignment");
        }

        if (mail == postman.MailToPickUp) {
            postman.Movement.SetDestination (mail.transform.position);
            mail.Assign (postman);
        } else {
            postman.MailInBag.Add (mail);
            mail.AssignedPostman = postman;
            mail.PickedUp = true;
            RemovePickupAssignment (mail);
        }
        Destroy (mail.GetComponent<Collider2D> ());
    }

    void HandleMailDelivery (Mail mail, Postman postman) {
        postman.MailToDeliver = null;

        if (postman.MailInBag.Count > 0) {
            postman.Movement.SetDestination (mail.transform.position);
            postman.MailInBag[0].Assign (postman);
            postman.MailInBag.RemoveAt (0);

        } else {
            postman.StartCoroutine ("WaitForAssignment");
        }

        Destroy (mail.gameObject);
    }

    void HandleMailNotInTime (Mail mail, Postman postman) {
        if (postman) {
            postman.MailToDeliver = null;
            postman.MailToPickUp = null;
            postman.StartCoroutine ("WaitForAssignment");
        }
        RemovePickupAssignment (mail);

        Debug.Log ("GAME OVER");
        Destroy (mail.gameObject);
    }

    void HandleMailSpawn (Household house) {

        Mail mail = Instantiate (GameManager.instance.mailPrefab).GetComponent<Mail> ();
        mailToBePickedUp.Add (mail);
        mail.SenderArea = this;
        house.mailToSend.Add (mail);
        Household recipientHouse = households.ElementAt (Random.Range (0, households.Count));
        while (recipientHouse.Inhabitants == 0) {
            recipientHouse = households.ElementAt (Random.Range (0, households.Count));
        }
        mail.Recipient = recipientHouse;
        mail.OnMailNotDeliveredInTime += HandleMailNotInTime;
        mail.transform.position = house.transform.position + new Vector3 (Random.Range (-0.02f, 0.02f), Random.Range (-0.02f, 0.02f), 0);
    }

    public void GetNextPickupAssignment (Postman postman) {
        if (mailToBePickedUp.Count > 0) {
            AssignPickupToPostman (mailToBePickedUp[0], postman);
        } else postman.MailToPickUp = null;
    }
    void AssignPickupToPostman (Mail mail, Postman postman) {
        if (mailToBePickedUp.Contains (mail)) {
            mailToBePickedUp.Remove (mail);
        }
        postman.MailToPickUp = mail;
        mail.AssignedPostman = postman;
        postman.Movement.SetDestination (mail.transform.position);
    }

    public Mail GetPickupAssignmentToArea (PostalArea area) {
        foreach (Mail mail in mailToBePickedUp) {
            if (mail.RecipientArea == area) {
                return mail;
            }
        }
        return null;
    }

    public void RemovePickupAssignment (Mail mail) {
        if (mailToBePickedUp.Contains (mail)) {
            mailToBePickedUp.Remove (mail);
        }
    }

    public void ShowPostalRoutes (MapIndicator mapIndicator) {
        Terminal closestTerminal = mapIndicator.GetClosestHousehold ().AssignedTerminal;

        foreach (var terminal in terminals) {
            if (terminal == closestTerminal) {
                terminal.ShowPostalRouteVisual ();
            } else {
                terminal.DontShowPostalRouteVisual ();
            }
        }
    }
    public void DontShowPostalRoutes () {
        foreach (var terminal in terminals) {
            terminal.DontShowPostalRouteVisual ();
        }
    }

}