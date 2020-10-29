using UnityEngine;

public class MovePositionDirect : MonoBehaviour, IMovePosition {
    private Vector3 movePosition;
    private IMoveVelocity Movement;

    private void Awake () {
        Movement = GetComponent<IMoveVelocity> ();
    }
    public void SetMovePosition (Vector3 movePosition) {
        this.movePosition = movePosition;
    }
    public void SetMoveSpeed (float speed) {
        Movement.SetSpeed (speed);
    }

    private void Update () {
        Vector3 direction = Vector3.zero;
        if (movePosition != Vector3.zero && (movePosition - transform.position).sqrMagnitude > 0.001f) {
            direction = (movePosition - transform.position).normalized;
        }
        Movement.SetDirection (direction);
    }
}