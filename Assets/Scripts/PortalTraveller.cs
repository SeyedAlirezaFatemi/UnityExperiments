using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anything that wants to go through portals should inherit from this.
public class PortalTraveller : MonoBehaviour {
    public Vector3 previousOffsetFromPortal { get; set; }

    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot) {
        transform.position = pos;
        transform.rotation = rot;
    }

    // Called when first touches the portal
    public virtual void EnterPortalThreshold() { }

    // Called once no longer touching the portal (Excluding when teleporting)
    public virtual void ExitPortalThreshold() { }
}