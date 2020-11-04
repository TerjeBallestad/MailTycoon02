using UnityEngine;

public class MovePositionDirect : MonoBehaviour, IMovePosition {
    private Vector3 destination;
    private IMoveVelocity Movement;

    private void Awake () {
        Movement = GetComponent<IMoveVelocity> ();
    }
    public void SetDestination (Vector3 destination) {
        this.destination = destination;
    }
    public void SetSpeed (float speed) {
        Movement.SetSpeed (speed);
    }

    public bool AtDestination () {
        return (destination - transform.position).sqrMagnitude < 0.005f;
    }

    private void Update () {
        Vector3 direction = Vector3.zero;
        if (destination != Vector3.zero && !AtDestination ()) {
            direction = (destination - transform.position).normalized;
        }
        Movement.SetDirection (direction);
    }
}