using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MudaCor : MonoBehaviour
{

    public GameObject objeto;
    public TextAsset texto;
    int algumaPosicao = 0;
    string[] nomes;

    void Start() {
        nomes = texto.text.Split('\n');
    }

    public void InstanciarObjeto () {
        GameObject go = Instantiate (objeto, transform.position, Quaternion.identity);
        go.name = nomes[algumaPosicao];
        algumaPosicao++;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            InstanciarObjeto ();
            Debug.Log(texto.text);
        } 
       
    }
}
