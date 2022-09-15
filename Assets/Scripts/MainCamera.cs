using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MainCamera : MonoBehaviour {
    [SerializeField] private Portal[] _portals = new Portal[2];
    [SerializeField] private Camera _portalCamera;
    [SerializeField] private int _iterations = 7;

    private RenderTexture _tmpTexture1;
    private RenderTexture _tmpTexture2;

    private Camera _mainCamera;

    private void Awake() {
        _mainCamera = Camera.main;
        _tmpTexture1 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        _tmpTexture2 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        // This camera should be disabled so it only renders when we tell it to.
        _portalCamera.enabled = false;
    }

    private void Start() {
        _portals[0].Renderer.material.mainTexture = _tmpTexture1;
        _portals[1].Renderer.material.mainTexture = _tmpTexture2;
    }

    private void OnEnable() {
        RenderPipelineManager.beginCameraRendering += UpdateCamera;
    }

    private void OnDisable() {
        RenderPipelineManager.beginCameraRendering -= UpdateCamera;
    }

    private void UpdateCamera(ScriptableRenderContext SRC, Camera camera) {
        if (_portals[0].Renderer.isVisible) {
            _portalCamera.targetTexture = _tmpTexture1;
            for (int i = _iterations - 1; i >= 0; i--) {
                RenderCamera(_portals[0], _portals[1], i, SRC);
            }
        }

        if (_portals[1].Renderer.isVisible) {
            _portalCamera.targetTexture = _tmpTexture2;
            for (int i = _iterations - 1; i >= 0; i--) {
                RenderCamera(_portals[1], _portals[0], i, SRC);
            }
        }

        foreach (var portal in _portals) {
            portal.ProtectScreenFromClipping(_mainCamera.transform.position);
        }
    }

    private void RenderCamera(Portal inPortal, Portal outPortal, int iterationID, ScriptableRenderContext SRC) {
        var inTransform = inPortal.transform;
        var outTransform = outPortal.transform;
        // Place the virtual camera at the main camera's position and orientation
        var cameraTransform = _portalCamera.transform;
        cameraTransform.position = transform.position;
        cameraTransform.rotation = transform.rotation;
        for (var i = 0; i <= iterationID; i++) {
            // Place the camera behind the other portal:
            //  1. Go from world-space to inPortal local-space
            var relativePos = inTransform.InverseTransformPoint(cameraTransform.position);
            //  2. Rotate by 180 degrees to go behind the portal
            // relativePos = Quaternion.Euler(0f, 180f, 0f) * relativePos;
            //  3. Go back to world-space but as if the point was in the local-space of the other portal
            cameraTransform.position = outTransform.TransformPoint(relativePos);
            //  4. Same steps for rotation
            var relativeRot = Quaternion.Inverse(inTransform.rotation) * cameraTransform.rotation;
            // relativeRot = Quaternion.Euler(0f, 180f, 0f) * relativeRot;
            cameraTransform.rotation = outTransform.rotation * relativeRot;
        }

        // Create the camera's oblique view frustum.
        // We create this new clip plane so we don't render the walls and objects behind walls.
        // https://docs.unity3d.com/Manual/ObliqueFrustum.html
        // It's -outTransform.forward when we get behind the other portal.
        var p = new Plane(-outTransform.forward, outTransform.position);
        // It's outTransform.forward when we get in front of the other portal.
        if (Vector3.Dot(_portalCamera.transform.forward, outTransform.forward) > 0) {
            p = new Plane(outTransform.forward, outTransform.position);
        }

        var clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
        var clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(_portalCamera.worldToCameraMatrix)) *
                                   clipPlaneWorldSpace;

        // Calculated with _mainCamera so that player camera settings (fov, etc) are used
        _portalCamera.projectionMatrix = _mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        // In case we are not directly looking at the portal, use the normal projection matrix
        // Might be possible to improve this by removing the abs. Need to think more!
        if (Math.Abs(Vector3.Dot(outTransform.forward, _portalCamera.transform.forward)) < 0.6) {
            _portalCamera.projectionMatrix = _mainCamera.projectionMatrix;
        }

        outPortal.ProtectScreenFromClipping(_portalCamera.transform.position);
        // Render the camera
        UniversalRenderPipeline.RenderSingleCamera(SRC, _portalCamera);
    }
}