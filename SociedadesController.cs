using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SociedadesController : MonoBehaviour {
	public GameObject panel;
	public GameObject btnAceptar;
	public GameObject btnCerrar;
	public GameObject btnFilSoc;
    public GameObject BtnSocList;
	private ControllerBoard tablero;
	List<Piece> listaOcultados;
	int cantFiltros;
    private GameObject ContainerFiltros, ContainerList;
    public Dictionary<int, List<string> > Sociedades; //PSB Lista de Sociedades y los componentes que tienen cada uno
    public Dictionary<int, Color > ColorSociedad; //diccionario colores
    private Text TextPanelSociedad;
    Color colorA,colorPC;

    // Use this for initialization
    void Start () {

        Sociedades = new Dictionary<int, List<string>>(); //PSB inicializo el diccionario de las sociedades
        ColorSociedad = new Dictionary<int, Color >();

        //crear colores iniciales
        ColorSociedad.Add(0, new Color(0.99f, 0.99f, 0.99f, 1.00f));
        ColorSociedad.Add(1, new Color(0f, 0.263581f, 0.6981132f, 1f));
        ColorSociedad.Add(2, new Color(1f, 0.1118307f, 0f, 1f));

		tablero = GameObject.Find("Tablero").GetComponent<ControllerBoard>();
		panel = GameObject.Find("PanelFiltrosSociedades");
		btnAceptar = GameObject.Find("BtnAceptarfilSoc");
		btnCerrar = GameObject.Find("BtnCerrarPanelFilSoc");
		btnFilSoc = GameObject.Find("BtnFilSoc");
        BtnSocList = GameObject.Find("BtnSocList");
        ContainerFiltros = GameObject.Find("ContainerFiltros");
        ContainerList = GameObject.Find("ContainerList");
        TextPanelSociedad = GameObject.Find("TextPanelSociedad").GetComponent<Text>();
		//cantidad de filtros
		cantFiltros = 3;
		
		//crear una lista para saber cuales elementos se han quitado del mapa
		listaOcultados = new List<Piece>();

		//listeners de botones
		Listeners();
		
		//ocultar panel
        btnAceptar.SetActive(false);
        ContainerFiltros.GetComponent<CanvasGroup>().alpha = 0;
		ContainerFiltros.SetActive(false);
        ContainerList.GetComponent<CanvasGroup>().alpha = 0;
		ContainerList.SetActive(false);
        panel.GetComponent<CanvasGroup>().alpha = 0;
		panel.SetActive(false);
	}
	private void Listeners(){ //cambiado
        //Boton para activar panel de filtros de sociedades
		btnFilSoc.GetComponent<Button>().onClick.AddListener(()=>{
			panel.SetActive(true);
            btnAceptar.SetActive(true);
			panel.GetComponent<CanvasGroup>().alpha = 1;
            ContainerFiltros.SetActive(true);
            ContainerFiltros.GetComponent<CanvasGroup>().alpha = 1;

            //titulo
            TextPanelSociedad.text = "Filtrar por sociedad";

			//cuando se abre el panel ver si se limpio el tablero
			if(GameObject.Find("Inventory").GetComponent<Inventory>().cleared){
				RestoreFilters();
				GameObject.Find("Inventory").GetComponent<Inventory>().cleared = false;
			}
		});

        //boton para activar panel de lista de sociedades
        BtnSocList.GetComponent<Button>().onClick.AddListener(()=>{
			panel.SetActive(true);
			panel.GetComponent<CanvasGroup>().alpha = 1;
            ContainerList.SetActive(true);
            ContainerList.GetComponent<CanvasGroup>().alpha = 1;
            //titulo
            TextPanelSociedad.text = "Selecciona una sociedad";			
		});
        //boton para cerrar panel
		btnCerrar.GetComponent<Button>().onClick.AddListener(()=>{
            btnAceptar.SetActive(false);
            ContainerFiltros.GetComponent<CanvasGroup>().alpha = 0;
            ContainerFiltros.SetActive(false);
            ContainerList.GetComponent<CanvasGroup>().alpha = 0;
            ContainerList.SetActive(false);
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

   /*  public void AceptarFiltros()
                Sociedades.Remove(sociedad);  
        }
    } */
    //PSB ===================================================================================================================

    public void Sociedad(){ //cambiado
        //aarb
        //inicializar toggles y botones

        //indicador de sociedad activa
        GameObject ImgSocActiva = GameObject.Find("ImgSocActiva");

        for(int i=0; i< cantFiltros; i++){ //para cada uno de los toggles de las sociedades
            int iterador = i;
			GameObject.Find("FiltroSoc"+iterador).GetComponent<Toggle>().isOn = true;
            
            //listener boton seleccion de sociedad
            GameObject.Find("BtnSoc"+iterador).GetComponent<Button>().onClick.AddListener(()=>{
                tablero.socGeneral = iterador;
                print(tablero.socGeneral);
                //asignar color de sociedad activa
                ImgSocActiva.GetComponent<Image>().color = ColorSociedad[iterador];
                //cerrar panel al seleccionar una sociedad
                ContainerList.GetComponent<CanvasGroup>().alpha = 0;
                ContainerList.SetActive(false);
                panel.GetComponent<CanvasGroup>().alpha = 0;
                panel.SetActive(false);
            });
		}   
        
    }//fSociedad

    public void AceptarFiltros() //cambiado
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
                        //tablero.DestroyFarm(p.GetX(0), p.GetY(0));
                        p.gameObject.SetActive(false);
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
                        //tablero.GeneratePiece(p.GetX(0), p.GetY(0), p.getIDPiece(), p.getTypePiece());
                        p.gameObject.SetActive(true);
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
