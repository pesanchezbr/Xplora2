using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SociedadesController : MonoBehaviour {
	public GameObject panel;
	public GameObject btnAceptar;
	public GameObject btnCerrar;
	public GameObject btnFilSoc;
	private ControllerBoard tablero;
	List<Piece> listaOcultados;
	int cantFiltros;

    //PSB Lista de Sociedades y los componentes que tienen cada uno
    public Dictionary<int, List<string>> Sociedades;
    //PSB

    // Use this for initialization
    void Start () {

        Sociedades = new Dictionary<int, List<string>>(); //PSB inicializo el diccionario de las sociedades

		tablero = GameObject.Find("Tablero").GetComponent<ControllerBoard>();
		panel = GameObject.Find("PanelFiltrosSociedades");
		btnAceptar = GameObject.Find("BtnAceptarfilSoc");
		btnCerrar = GameObject.Find("BtnCerrarPanelFilSoc");
		btnFilSoc = GameObject.Find("BtnFilSoc");
		//cantidad de filtros
		cantFiltros = 3;
		
		//crear una lista para saber cuales elementos se han quitado del mapa
		listaOcultados = new List<Piece>();

		//listeners de botones
		Listeners();
		
		//ocultar panel
		panel.GetComponent<CanvasGroup>().alpha = 0;
		panel.SetActive(false);
	}
	private void Listeners(){
		btnFilSoc.GetComponent<Button>().onClick.AddListener(()=>{
			panel.SetActive(true);
			panel.GetComponent<CanvasGroup>().alpha = 1;
			//cuando se abre el panel ver si se limpio el tablero
			if(GameObject.Find("Inventory").GetComponent<Inventory>().cleared){
				RestoreFilters();
				GameObject.Find("Inventory").GetComponent<Inventory>().cleared = false;
			}
		});
		btnCerrar.GetComponent<Button>().onClick.AddListener(()=>{
			panel.GetComponent<CanvasGroup>().alpha = 0;
			panel.SetActive(false);
		});
		AceptarFiltros();
		Sociedad();
	}


    //PSB ===================================================================================================================
    //Operaciones con el diccionario de sociedades
    public bool ValidarElementoSociedad(int sociedad, string idElem) //Valida si un elemento puede ser insertado en el tablero segun la sociedad
    {
        bool addok = false;
        if (!Sociedades.ContainsKey(sociedad))
        {
            Sociedades.Add(sociedad, new List<string>());
            Sociedades[sociedad].Add(idElem);
            addok = true;
        }
        else
        {
            //Existe la sociedad
            if (!Sociedades[sociedad].Exists(v => v == idElem)) //No se encuentra el componente en la sociedad
            {
                Sociedades[sociedad].Add(idElem);
                addok = true;
            }
        }
        return addok;
    }

    public void EliminarComponenteDiccionario(int sociedad, string idElem) //Elimina un componente de la sociedad si es el unico elimina la sociedad
    {//Si sociedad no existe colocar -1
        if (Sociedades.ContainsKey(sociedad))
        {
            if (Sociedades[sociedad].Exists(p => p == idElem))
            {
                Sociedades[sociedad].Remove(idElem);
            }
            if (Sociedades[sociedad].Count == 0)
                Sociedades.Remove(sociedad);
        }
        else
        {
            print("borrando por componente individual");
        }
    }

    public void VaciarDiccionarioComponentes()
    {
        Sociedades.Clear();
    }
    //PSB ===================================================================================================================


    public void Sociedad(){
		//los botones de eleccion de sociedad
        GameObject.Find("BtnSoc0").GetComponent<Button>().onClick.AddListener(()=>{
            tablero.socGeneral = 0;
        });
        GameObject.Find("BtnSoc1").GetComponent<Button>().onClick.AddListener(()=>{
            tablero.socGeneral = 1;
        });
        GameObject.Find("BtnSoc2").GetComponent<Button>().onClick.AddListener(()=>{
            tablero.socGeneral = 2;
        });        
    }//fSociedad

    public void AceptarFiltros()
    {
        //funcion que se ejecuta al presionar el boton aceptar, consigue los elementos de la sociedades y segun el toggle
        //crear una lista con los componentes que se han eliminado y despues que se recoloquen borrarlos de esa lista
        List<Piece> listaP;
        List<Piece> listaAdd;
        btnAceptar.GetComponent<Button>().onClick.AddListener(() => {
            for (int i = 0; i < cantFiltros; i++)
            { //para cada uno de los toggles de las sociedades
                if (!GameObject.Find("FiltroSoc" + i).GetComponent<Toggle>().isOn)
                { //si esta desactivado
                  //busco todos los componentes donde la sociedad sea la actual para cada toggle
                    listaP = tablero.ComponentesMapa.FindAll(pc => pc.getSociedad() == i);
                    Debug.Log(listaP.Count);
                    //añadir elementos a lista de ocultados
                    listaOcultados.AddRange(listaP);
                    //eliminar elementos
                    foreach (Piece p in listaP)
                    {
                        tablero.DestroyFarm(p.GetX(0), p.GetY(0));
                    }//fforeach
                }
                else
                { //si esta activado
                    int AuxSoc = tablero.socGeneral;
                    listaAdd = listaOcultados.FindAll(pc => pc.getSociedad() == i);
                    listaOcultados.RemoveAll(pc => pc.getSociedad() == i); //eliminar de la lista
                                                                           //crear de nuevo los elementos
                    tablero.socGeneral = i;
                    foreach (Piece p in listaAdd)
                    {
                        tablero.GeneratePiece(p.GetX(0), p.GetY(0), p.getIDPiece(), p.getTypePiece());
                    }//fforeach
                    tablero.socGeneral = AuxSoc;
                }//felse
            }
            GameObject.Find("Code").GetComponent<GranjeroControllerAux>().WrapperCrearCaminos(); //redibujar caminos
        });
    }//fAceptarFiltros

    public void RestoreFilters(){

		for(int i=0; i< cantFiltros; i++){ //para cada uno de los toggles de las sociedades
			GameObject.Find("FiltroSoc"+i).GetComponent<Toggle>().isOn = true;
		}

		listaOcultados.Clear();
	}//fRestoreFilters
	
	
}
