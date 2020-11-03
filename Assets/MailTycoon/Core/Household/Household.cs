using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Household : MonoBehaviour {

    [HideInInspector] public bool ShouldSpawnMail = true;
    [HideInInspector] public event Action<Household> OnMailSpawn;
    [HideInInspector] public Color HouseColor;
    public List<Mail> MailToSend;
    public Postman AssignedPostman;
    public PostalArea AssignedPostalArea;
    public Terminal AssignedTerminal;
    public Mailbox AssignedMailbox;
    public int Inhabitants, PropertyValue;

    private void OnEnable () {
        StartCoroutine (SpawnMailLoop ());
        MailToSend = new List<Mail> ();
    }

    void SpawnMail () {
        Mail mail = Instantiate (GameManager.instance.mailPrefab).GetComponent<Mail> ();
        // MailToBePickedUp[house.AssignedPostman].Add (mail);
        mail.SenderArea = AssignedPostalArea;
        Household recipientHouse = AssignedPostalArea.Households.ElementAt (UnityEngine.Random.Range (0, AssignedPostalArea.Households.Count));
        while (recipientHouse.Inhabitants == 0) {
            recipientHouse = AssignedPostalArea.Households.ElementAt (UnityEngine.Random.Range (0, AssignedPostalArea.Households.Count));
        }
        mail.Sender = this;
        MailToSend.Add (mail);
        mail.Recipient = recipientHouse;
        // mail.OnMailNotDeliveredInTime += AssignedPostalArea.HandleMailNotInTime;
        mail.transform.position = transform.position + new Vector3 (UnityEngine.Random.Range (-0.02f, 0.02f), UnityEngine.Random.Range (-0.02f, 0.02f), 0);
    }

    public void ResetToDefaultColor () {
        GetComponent<SpriteRenderer> ().color = HouseColor;
    }

    IEnumerator SpawnMailLoop () {
        while (ShouldSpawnMail) {
            yield return new WaitForSeconds (UnityEngine.Random.Range (4, 13));
            for (int i = 0; i < Inhabitants; i++) {
                if (UnityEngine.Random.Range (0, 1000) > 998) {
                    // OnMailSpawn?.Invoke (this);
                    SpawnMail ();
                }
            }
        }
    }
    public void Assign (Postman postman) {

        if (AssignedPostman != null) {
            AssignedPostman.AssignedHouses.Remove (this);
        }
        AssignedPostman = postman;
        postman.AssignedHouses.Add (this);
    }

    public void Assign (Terminal terminal) {
        if (AssignedTerminal != null) {
            AssignedTerminal.Households.Remove (this);
            // OnMailSpawn -= AssignedTerminal.HandleMailSpawn;
        }
        AssignedTerminal = terminal;
        terminal.Households.Add (this);
        // OnMailSpawn += terminal.HandleMailSpawn;
    }

}