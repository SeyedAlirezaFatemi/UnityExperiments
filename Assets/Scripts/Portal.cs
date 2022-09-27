using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.BoxCollider))]
public class Portal : MonoBehaviour {
    [SerializeField] private Portal _otherPortal;
    [SerializeField] private MeshRenderer _screen;

    // Components
    public Renderer Renderer { get; private set; }

    public List<PortalTraveller> _trackedTravellers;
    private Camera _playerCamera;
    private bool first = true;

    private void Awake() {
        Renderer = _screen;
        _trackedTravellers = new List<PortalTraveller>();
        _playerCamera = Camera.main;
    }

    private void LateUpdate() {
        for (var i = 0; i < _trackedTravellers.Count; i++) {
            var traveller = _trackedTravellers[i];
            var travellerTransform = traveller.transform;
    
            var offsetFromPortal = travellerTransform.position - transform.position;
            int currentPortalSide = Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int prevPortalSide = Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));
            // Debug.Log(offsetFromPortal.z);
            // if (first && offsetFromPortal.z < 1e-4) {
            //     first = false;
            //     Debug.Break();
            // }
            traveller.previousOffsetFromPortal = offsetFromPortal;
            if (currentPortalSide != prevPortalSide) {
                // Debug.Log("Crossed");
                // Traveller crossed the portal
                var m = _otherPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix *
                        travellerTransform.localToWorldMatrix;
                traveller.Teleport(transform, _otherPortal.transform, m.GetPosition(), m.rotation);

                _otherPortal.OnTravellerEnterPortal(traveller);
                _trackedTravellers.RemoveAt(i);
                i--;
            }

        }
    }

    private void OnTravellerEnterPortal(PortalTraveller traveller) {
        if (!_trackedTravellers.Contains(traveller)) {
            traveller.EnterPortalThreshold();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            _trackedTravellers.Add(traveller);
        }
    }

    private void OnTriggerEnter(Collider other) {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller) {
            OnTravellerEnterPortal(traveller);
        }
    }

    private void OnTriggerExit(Collider other) {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && _trackedTravellers.Contains(traveller)) {
            traveller.ExitPortalThreshold();
            _trackedTravellers.Remove(traveller);
        }
    }

    // Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
    public float ProtectScreenFromClipping(Vector3 viewPoint) {
        float halfHeight = _playerCamera.nearClipPlane * Mathf.Tan(_playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * _playerCamera.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, _playerCamera.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = _screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        // Debug.Log($"before: {screenT.localPosition}-{screenT.localScale}");
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        // Debug.Log($"after: {screenT.localPosition}-{screenT.localScale}");
        return screenThickness;
    }
}