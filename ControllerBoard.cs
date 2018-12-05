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

    void paquitaBarrio(){
        print("Maldita Lisiada");
    //vivavv
    void OlaKAse(){

      print("Hola");
      print("Que");
      print("Hace");
    }

    /* aarb */
    public void AddDestino(int comp, string destino,bool val, string tipo){
        //Agregar destino al granjero que sale de un componente
        destinos[comp].Add(destino);
        destinosComp[comp].Add(new DestinoComponentes(destino,val,tipo));
    }
}