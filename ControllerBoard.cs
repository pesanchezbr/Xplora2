using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Linq;

//esto es un fix
//fix de pedro


  //VERGA
public class ControllerBoard : MonoBehaviour {



    private void Start() {

    }

<<<<<<< HEAD
    void paquitaBarrio(){
        print("Maldita Lisiada");
=======
    /* aarb */
    public void AddDestino(int comp, string destino,bool val, string tipo){
        //Agregar destino al granjero que sale de un componente
        destinos[comp].Add(destino);
        destinosComp[comp].Add(new DestinoComponentes(destino,val,tipo));
>>>>>>> 42240b4bd3679b17ee3497db7d4cef962192f747
    }
}