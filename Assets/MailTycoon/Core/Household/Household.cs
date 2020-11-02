using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Household : MonoBehaviour {

    [HideInInspector] public bool ShouldSpawnMail = true;
    [HideInInspector] public event Action<Household> OnMailSpawn;
    [HideInInspector] public Color HouseColor;
    [HideInInspector] public List<Mail> mailToSend;
    public Postman AssignedPostman;
    public Terminal AssignedTerminal;
    public Mailbox AssignedMailbox;
    public int Inhabitants, PropertyValue;

    private void OnEnable () {
        StartCoroutine (SpawnMailLoop ());
    }

    void SpawnMail () {
        for (int i = 0; i < Inhabitants; i++) {
            if (UnityEngine.Random.Range (0, 1000) > 998) {
                OnMailSpawn?.Invoke (this);
            }
        }
    }

    public void ResetToDefaultColor () {
        GetComponent<SpriteRenderer> ().color = HouseColor;
    }

    IEnumerator SpawnMailLoop () {
        while (ShouldSpawnMail) {
            yield return new WaitForSeconds (UnityEngine.Random.Range (4, 13));
            SpawnMail ();
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
            OnMailSpawn -= AssignedTerminal.HandleMailSpawn;
        }
        AssignedTerminal = terminal;
        terminal.Households.Add (this);
        OnMailSpawn += terminal.HandleMailSpawn;
    }
    public void Assign (Mailbox mailbox) {
        if (AssignedMailbox != null) {
            AssignedMailbox.Households.Remove (this);
        }
        AssignedMailbox = mailbox;

    }
}