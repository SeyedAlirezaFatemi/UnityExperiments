using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.Renderer))]
public class Portal : MonoBehaviour {
    // Components
    public Renderer Renderer { get; private set; }

    private void Awake() {
        Renderer = GetComponent<Renderer>();
    }
}