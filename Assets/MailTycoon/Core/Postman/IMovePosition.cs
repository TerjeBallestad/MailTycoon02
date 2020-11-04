using UnityEngine;

public interface IMovePosition {
    void SetDestination (Vector3 destination);
    void SetSpeed (float speed);
    bool AtDestination ();
}