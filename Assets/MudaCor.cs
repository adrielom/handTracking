using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MudaCor : MonoBehaviour
{

    Color color;
    MeshRenderer mesh;

    void Start() {
        color = Color.red;
        mesh = GetComponent<MeshRenderer>();
    }

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.name == "Sphere(Clone)" && mesh.material.color != color) {
            mesh.material.color = color;
            Debug.Log("hey");
        }
    }
}
