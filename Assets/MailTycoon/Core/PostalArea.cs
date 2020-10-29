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
    Dictionary<Postman, List<Household>> routes;
    List<Postman> postmen;

    List<Household> households;
    new Collider collider;

    private void Start () {
        terminals = new List<Terminal> ();
        households = new List<Household> ();
        mailToBePickedUp = new List<Mail> ();
        postmen = new List<Postman> ();
        collider = GetComponent<Collider> ();
        routes = new Dictionary<Postman, List<Household>> ();
        GameManager.instance.selectedArea = this;

        SpawnHouseholds ();
        for (int i = 0; i < 5; i++) {

            SpawnPostman ();
        }
        SpawnPostalOffice ();
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

    public int SpawnPostman () {

        Household randomHouse = households.ElementAt (Random.Range (0, households.Count));
        Postman postman = Instantiate (GameManager.instance.postmanPrefab, randomHouse.transform.position, Quaternion.identity).GetComponent<Postman> ();
        MapIndicator PostmanMapDivisionObject = Instantiate (GameManager.instance.mapIndicatorPrefab, new Vector3 (randomHouse.transform.position.x, randomHouse.transform.position.y, -1f), Quaternion.identity).GetComponent<MapIndicator> ();
        PostmanMapDivisionObject.GetComponent<SpriteRenderer> ().color = GameManager.instance.LightColors[postmen.Count];
        postman.MapDivisionPosition = PostmanMapDivisionObject.transform;
        PostmanMapDivisionObject.gameObject.SetActive (false);
        postmen.Add (postman);
        postman.postalArea = this;
        postman.gameObject.name = this.name + " postman";
        postman.OnMailPickup += HandleMailPickup;
        postman.OnMailDelivered += HandleMailDelivery;
        postman.StartCoroutine ("WaitForAssignment");
        return postmen.Count;
    }

    public int DespawnPostman () {
        if (postmen.Count > 0) {
            Postman postmanToRemove = postmen[postmen.Count - 1];
            postmen.Remove (postmanToRemove);
            Destroy (postmanToRemove.gameObject);
        }
        return postmen.Count;
    }

    void SpawnPostalOffice () {
        foreach (var household in households) {
            if (household.Inhabitants == 0) {
                Terminal terminal = Instantiate (GameManager.instance.postOfficePrefab, household.transform.position, Quaternion.identity).GetComponent<Terminal> ();
                households.Remove (household);
                Destroy (household.gameObject);
                terminal.Postmen = new List<Postman> (postmen);
                foreach (var postman in postmen) {
                    postman.AssignedTerminal = terminal;
                    postman.OnMailPickup -= HandleMailPickup;
                    postman.OnMailPickup += terminal.HandleMailPickup;
                }
                terminal.Area = this;
                terminals.Add (terminal);
                break;
            }
        }
        AssignHouseholdsToTerminals ();
    }

    public void AssignHouseholdsToTerminals () {

        foreach (var household in households) {
            float shortestDistance = 1000f;

            foreach (var terminal in terminals) {
                float distance = ((household.transform.position - terminal.transform.position).sqrMagnitude);

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
            AssignDeliveryToPostman (mail, postman);
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
            AssignDeliveryToPostman (postman.MailInBag[0], postman);
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
        mail.Area = this;
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
        postman.Movement.SetMovePosition (mail.transform.position);
    }
    void AssignDeliveryToPostman (Mail mail, Postman postman) {
        postman.Movement.SetMovePosition (mail.Recipient.transform.position);
        postman.MailToDeliver = mail;
        postman.MailToPickUp = null;
        mail.PickedUp = true;
    }

    void AssignPostmanToTerminal (Terminal terminal, Postman postman) {
        postman.OnMailPickup -= HandleMailPickup;
        postman.OnMailPickup += terminal.HandleMailPickup;
        postman.OnMailDelivered -= HandleMailDelivery;
        postman.OnMailDelivered += terminal.HandleMailDelivery;
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
}