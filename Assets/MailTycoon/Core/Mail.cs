using System;
using UnityEngine;

public class Mail : MonoBehaviour {

    [HideInInspector] public Household Recipient;
    [HideInInspector] public PostalArea RecipientArea;
    [HideInInspector] public PostalArea Area;
    [HideInInspector] public Postman AssignedPostman;
    [HideInInspector] public event Action<Mail, Postman> OnMailNotDeliveredInTime;
    [HideInInspector] public bool PickedUp = false;
    SpriteRenderer spriteRenderer;

    void Start () {
        spriteRenderer = GetComponent<SpriteRenderer> ();
    }

    void FixedUpdate () {
        if (AssignedPostman && PickedUp) {
            spriteRenderer.sprite = null;

        } else {
            Color color = spriteRenderer.color;
            color.b -= 0.001f;
            color.g -= 0.0005f;
            spriteRenderer.color = color;
            if (spriteRenderer.color.g < 0.10f) {
                OnMailNotDeliveredInTime?.Invoke (this, AssignedPostman);
            }
        }
    }
}