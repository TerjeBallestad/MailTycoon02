using UnityEngine;

public interface IMovePosition {
    void SetDestination (Vector3 destination);
    void SetMoveSpeed (float speed);
    bool AtDestination ();
}