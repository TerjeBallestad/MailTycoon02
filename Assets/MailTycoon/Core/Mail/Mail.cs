using System;
using UnityEngine;

public class Mail : MonoBehaviour {

    [HideInInspector] public Household Sender;
    [HideInInspector] public Household Recipient;
    [HideInInspector] public PostalArea RecipientArea;
    [HideInInspector] public PostalArea SenderArea;
    public Postman AssignedPostman;
    [HideInInspector] public event Action<Mail, Postman> OnMailNotDeliveredInTime;
    [HideInInspector] public bool PickedUp = false;
    SpriteRenderer spriteRenderer;

    private void OnEnable () {
        spriteRenderer.color = Color.white;
    }

    void Awake () {
        spriteRenderer = GetComponent<SpriteRenderer> ();
    }

    void FixedUpdate () {
        if (AssignedPostman && PickedUp) {
            // spriteRenderer.sprite = null;

        } else {
            Color color = spriteRenderer.color;
            color.b -= 0.001f;
            color.g -= 0.0005f;
            spriteRenderer.color = color;
            if (spriteRenderer.color.g < 0.10f) {
                // OnMailNotDeliveredInTime?.Invoke (this, AssignedPostman);
            }
        }
    }

    public void Assign (Postman postman) {
        // postman.MailToDeliver = this;
        // postman.MailToPickUp = null;

        postman.MailInBag.Add (this);

        PickedUp = true;
        AssignedPostman = postman;
        transform.SetParent (postman.transform);
    }

    public void Assign (Terminal terminal) {
        terminal.MailInTerminal.Add (this);
        AssignedPostman = null;
        transform.SetParent (terminal.transform);

    }
}