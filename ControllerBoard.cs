using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Linq;

public class ControllerBoard : MonoBehaviour {

    public SociedadesController SocController;

    public Animator anim;
    public Piece[,] pieces = new Piece[24, 24];
    public Piece[,] piecesAux = new Piece[24, 24];
    public Piece selectedPiece, p, auxiliar, selected, SlaveAux1, SlaveAux2;
    public bool Drag = false;
    public Casilla[,] casillas = new Casilla[24, 24];
    public Casilla selectedCell, selectedCell1, selectedCell2;

    private List<Piece> forcedPieces;

    private GameObject FarmPrefab, FarmPrefab2, FarmPrefab3, panelInfo, panelTitle;
    private Inventory inv;
    public GameObject ColorCell, Slave1, Slave2, DisableSlave, panelInventory;
    public Renderer rend;

    public GameObject CasillaPrefab;
    public int ancho = 24, alto = 24;

    private Vector3 boardOffset = new Vector3(-12f, 0, -12f);

    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);
    private Vector3 casillaOffset = new Vector3(0.5f, -0.09f, 0.5f);

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;
    private Vector2 vectorSlave, vectorSlave2;
    private Vector2 vectorAux, vectorAux2;
    ItemDatabase database;
    public Piece pOpenAux;
    public int x, y;

    public Camera m_OrthographicCamera;
    private Vector2 removeGO;

    private Menu Menu;

    int contador = 0;
    public int openX, openY;
   

    public bool currentFarm, rotate2, isWhite;

    private string info, data;

    public bool rotationS, invisible;

    public CameraController _CameraController;

    private int contadorclicks;
    public float clickTimer = 0.2f;
    public bool preguntopordobleclick = false;
    public bool esdobleclick = false;

    public List<int> cantidades;

    public float TiempoPresionado = 0f;
    public float TiempoParaMenu = 1.0f;
    public bool Arrastrando = false;
    private int RotacionTablero = 0;

    public GameObject granjero;
    public GameObject granjero1;

    public GranjeroController granjeroController;
    public List<string>[] destinos = new List<string>[3]; //3 componentes
    public List<DestinoComponentes>[] destinosComp = new List<DestinoComponentes>[3];
    public List<Piece> ComponentesMapa;

    Shader shader;
    Color colorA,colorPC;
    public Shader diffuse;

    Dictionary<string, int> ComponentsQ;

    public int socGeneral;

    private void Start() {

        SocController = GameObject.Find("Code").GetComponent<SociedadesController>();

        ComponentsQ = new Dictionary<string, int>();

        cantidades = new List<int>();
        cantidades.Add(0); //Posicion 0 inicializada en 0, Componente tipo 1
        cantidades.Add(0);
        cantidades.Add(0);
        ColorCell = GameObject.Find("ColorCell");
        panelInfo = GameObject.Find("PanelInfo");
        inv=GameObject.Find("Inventory").GetComponent<Inventory>();
        database = GameObject.Find("Inventory").GetComponent<ItemDatabase>();
        Menu = GameObject.Find("Code").GetComponent<Menu>();
        currentFarm = true;
        rotate2 = true;
        rotationS = true;
        panelInfo.SetActive(false);
        //GenerarGranjero();
        //lista de destinos
        destinos[0] = new List<string>(); //componente 1
        destinos[1] = new List<string>(); //componente 2
        destinos[2] = new List<string>(); //componente 3
        destinosComp[0] = new List<DestinoComponentes>();
        destinosComp[1] = new List<DestinoComponentes>();
        destinosComp[2] = new List<DestinoComponentes>();
        //añadir destinos
        AddDestino(0,"XPCRI2",true,"Granjero2"); //añado el componente 2 como destino para el componente 1
        AddDestino(0,"XPCRI3",true,"Granjero1"); //añado el componente 3 como destino para el componente 1
        AddDestino(1,"XPCRI3",true,"Granjero1"); //añado el componente 3 como destino para el componente 2
        AddDestino(2,"XPCRI1",true,"Granjero2"); //añado el componente 1 como destino para el componente 3
        AddDestino(2,"XPCRI2",true,"Granjero2"); //añado el componente 1 como destino para el componente 3
        //salidas hacia el exterior, no hacia otro componente
        AddDestino(0,"Export",true,"Granjero3");
        AddDestino(1,"Export",true,"Granjero3");        
        AddDestino(2,"Export",true,"Granjero3");
        m_OrthographicCamera = GameObject.Find("Camera").GetComponent<Camera>();
        ComponentesMapa = new List<Piece>();

        colorA = new Color(0.294f, 0.290f, 0.290f, 1.00f);
        colorPC= new Color(0.99f, 0.99f, 0.99f, 1.00f);

        //sociedades
        socGeneral = 0; //si no se ha seleccionado otra sociedad, entonces es generica

        //PSB esto es para el boton de eliminar todos los componentes
        GameObject.Find("deleteAll").GetComponent<Button>().onClick.AddListener(() =>
        {
            print("aqui");
            inv.GetComponent<Inventory>().Destroyer();
        });
        //PSB

        GenerateBoard();

    }//quizas podria colocarle al destino el tipo de granjero que envia para que cambie el tipo de granjero segun la ruta

    // Update is called once per frame
    void Update() {
        UpdateMouseOver();

        x = (int)mouseOver.x;
        y = (int)mouseOver.y;
        if((x > -1 && x < 24) && (y > -1 && y < 24)){ //corregir error de index out of range cuando el mouse se sale del tablero
            p = pieces[x, y];
        }else{
            p = null;
        }

        if (Input.GetMouseButtonDown(0)  && !EventSystem.current.IsPointerOverGameObject()) {
            SelectPiece(x, y);
        }

        if (p == null && Menu.ready) {    
            if (Input.GetMouseButtonDown(0)) {
                esdobleclick = false;
                contadorclicks++;
                if (contadorclicks == 1) {
                    StartCoroutine("dobleClick");
                }
            }
        }

        if (Input.GetMouseButton(0)) {
            TiempoPresionado += Time.deltaTime;

            if (selectedPiece != null && TiempoPresionado > TiempoParaMenu && Arrastrando == false && Menu.ready && Menu.move){
                
                StartCoroutine(GameObject.Find("Code").GetComponent<GranjeroControllerAux>().BorrarGranjeros());
                UpdatePieceDrag(selectedPiece, x, y, (int)startDrag.x, (int)startDrag.y);
                GameObject.Find("Camera").GetComponent<CameraController>().pan = false;
            }
        }else if (Input.GetMouseButtonUp(0)) {
            Arrastrando = false;
            GameObject.Find("Camera").GetComponent<CameraController>().pan = true;
            if (selectedPiece != null && Menu.ready && TiempoPresionado > TiempoParaMenu) {
                
                    TryMove((int)startDrag.x, (int)startDrag.y, x, y);
                
            }
            
            if (p != null && p.getIDPiece()!="Export") {
                if (TiempoPresionado < TiempoParaMenu){
                    RemoveGO(x, y);//guarda la posicion de la granja a la cual se activo el menu de opciones 
                    StartCoroutine("MenuOp");
                }           
            }
            TiempoPresionado = 0f;
        }
    }
    

   


    //visualizacion de la descripcion de cada componente
    public void ViewInfo(int x, int y) {
        panelInfo.SetActive(true);
        Piece pd = pieces[x, y];
        // la informacion mostrada esta contenida en el archivo json items
        if (pd.id =="XPCRI1") {
            Item itemAux = database.FetchItemByID(0);
            info = itemAux.Info;
        }else if (pd.id =="XPCRI2") {
            Item itemAux = database.FetchItemByID(1);
            info = itemAux.Info;
        }else if (pd.id =="XPCRI3") {
            Item itemAux = database.FetchItemByID(2);
            info = itemAux.Info;
        }
        else if (pd.id == "XPCRI4") {
            Item itemAux = database.FetchItemByID(3);
            info = itemAux.Info;
        }
        else if (pd.id == "XPCRI5") {
            Item itemAux = database.FetchItemByID(4);
            info = itemAux.Info;
        }
        else if (pd.id == "XPCRI6") {
            Item itemAux = database.FetchItemByID(5);
            info = itemAux.Info;
        }
        else if (pd.id == "XPCRI7") {
            Item itemAux = database.FetchItemByID(6);
            info = itemAux.Info;
        }
        else if (pd.id == "XPCRI8") {
            Item itemAux = database.FetchItemByID(7);
            info = itemAux.Info;
        }
        else if (pd.id == "XPCRI9") {
            Item itemAux = database.FetchItemByID(8);
            info = itemAux.Info;
        }
        else if (pd.id == "XPCRI10") {
            Item itemAux = database.FetchItemByID(9);
            info = itemAux.Info;
        }
        else if (pd.id == "XPCRI11") {
            Item itemAux = database.FetchItemByID(10);
            info = itemAux.Info;
        }
        

        //data = "<color=#DF0174><b>" + pd.nombreUI + "\n</b></color>\n" + info;
        data = "<b>" + info + "\n</b>\n";
        panelInfo.transform.GetChild(0).GetComponent<Text>().text = data;
    }

    //devuelve una pieza de la matriz pieza segun las posiciones dadas
    public Piece GetPiece(int x, int y) {
        Piece pd = pieces[x, y];
        return pd;
    }

    //activa las casillas al arrastrar un item del teclado dependiendo del tipo de componente
    public void UpdateItemDrag(int equis, int ye, Item itemAux) {
        RaycastHit hit;
        
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200.0f, LayerMask.GetMask("Board"))) {
                selectedCell = casillas[equis, ye];
                selectedCell.GetComponent<Renderer>().enabled = true;
                
            }
    }

	//crea un vector con la posicion que desea borrar
    public void RemoveGO(int x, int y) {

        if (x < 0 || x >= 24 || y < 0 || y >= 24)
            return;

        Piece pb = pieces[x, y];
        if (pb != null && Menu.ready) {
            removeGO = mouseOver;
            removeGO = mouseOver;
        }

        if (pb.getName()=="Slave1"||pb.getName()=="Slave2"){
            pb = pb.transform.parent.GetComponent<Piece>();
            removeGO.x=pb.posiciones[0].xP;
            removeGO.y=pb.posiciones[0].yP;
        }

    }

    public void RotateFarm(int startx, int starty) {
        Piece p = pieces[startx, starty];

        if (p != null) {
            if (Menu.cont == 1 && rotate2) {
                p.transform.Rotate(0, 90, 0);
                rotate2 = false;
                if (p.Rotation < 3) {
                    p.Rotation++;
                }
                else {
                    p.Rotation = 0;
                }
            }

            if (Menu.cont == 2 && !rotate2) {
                p.transform.Rotate(0, 90, 0);
                rotate2 = true;
                Menu.cont = 0;
                if (p.Rotation < 3) {
                    p.Rotation++;
                }
                else {
                    p.Rotation = 0;
                }
            }
        }

    }


    //destruye el GameObject(granja) que tiene el menu de opciones activo
    public void DestroyFarm(int startx, int starty) {

        Piece p = pieces[startx, starty];
        string idp;
        if (p != null) {
            idp = p.getIDPiece();

            if (p.getTypePiece() == 1) {
                cantidades[0] = cantidades[0] - 1;
                BreakFree(startx, starty);
            }
            else if (p.getTypePiece() == 2) {
                cantidades[1] = cantidades[1] - 1;
                BreakFree(startx, starty);
                BreakFree(startx + 1, starty);
            }
            else if (p.getTypePiece() == 3) {
                cantidades[2] = cantidades[2] - 1;
                BreakFree(startx, starty);
                BreakFree(startx + 1, starty);
                BreakFree(startx + 1, starty - 1);
            }
            pieces[startx, starty] = null;

            if (ComponentsQ[idp] - 1 == 0) {
                ComponentsQ.Remove(idp);
                print("Se elimino el elemento " + idp + " del diccionario");
            }
            else {
                ComponentsQ[idp] -= 1;
                print("Eliminando: " + idp + " " + ComponentsQ[idp]);
            }


            DestroyImmediate(p.gameObject);
            Menu.DestroyGameObject();
            Menu.CloseMenuPanel();
            Menu.ModXY();
            currentFarm = true;

            //borrar componente de la lista de componentes y resetear simulacion
            int index = ComponentesMapa.IndexOf(p);
            ComponentesMapa.RemoveAt(index);
            GameObject.Find("Code").GetComponent<GranjeroControllerAux>().WrapperCrearCaminos();
            
            //PSB
            GameObject.Find("Code").GetComponent<SociedadesController>().EliminarComponenteDiccionario(p.getSociedad(), p.getIDPiece());
            //PSB        
        }


    }

    //desactiva todas las casillas del tablero
    public void UpdateBoard() {

        for (int i = 0; i < 23; i++) {
            for (int j = 0; j < 23; j++) {
                selectedCell = casillas[i, j];
                selectedCell.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void UpdateMouseOver() {
        
        if (!Camera.main) {
            Debug.Log("No se consigue la camara principal");
            return;
        }

        RaycastHit hit;                                                                                                  

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200.0f, LayerMask.GetMask("Board"))){
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        } else {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }

   public void UpdatePieceDrag(Piece p, int equis, int ye, int startequis, int startye) {
        Drag = true;
        if (!Camera.main) {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        bool bandDrag;

        //Si el mapa se debe agrandar, el 25.5 debe ser mayor, para que el Raycast aumente su alcance
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200.0f, LayerMask.GetMask("Board"))) {
            anim = GetComponent<Animator>();
            if(p.gameObject.transform.childCount == 0) {
                if(p.getName()=="Slave2")
                   p.GetComponent<Collider>().enabled = false;
                
                Piece Ps;
                Ps = p;
                

            }
           
          
            p.transform.position = hit.point + Vector3.up / 2;
        }

        contador = contador + 1;
        for (int i = 0; i < ancho; i++) {
            for (int j = 0; j < alto; j++){
                selectedCell = casillas[i, j];
                selectedCell.GetComponent<Renderer>().enabled = false;
            }
        }

        vectorAux = Vector2.zero;
        vectorAux2 = Vector2.zero;
        selectedCell = casillas[equis, ye];
        selectedCell.ChangeColor(true, casillas, startequis, startye);
        selectedCell.GetComponent<Renderer>().enabled = true;
        
        vectorAux = Vector2.zero;
        vectorAux2 = Vector2.zero;
    }

    public void SelectPiece(int x, int y) {

        if (x < 0 || x >= 24 || y < 0 || y >= 24)
            return;

        Piece p = pieces[x, y];
        if (p != null) {
            selectedPiece = p;
            startDrag = mouseOver;
        }

    }

    public void TryMove(int x1, int y1, int x2, int y2) {

        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        // Out of bounds
        if (x2 < 0 || x2 >= 24 || y2 < 0 || y2 >= 24) {

            if (selectedPiece != null) {
                MovePiece(selectedPiece, x1, y1);
            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            StatusCell(x1, y1);
            return;
        }

        if (selectedPiece != null) {
            // If it has not moved   
            if (endDrag == startDrag) {
                selectedCell = casillas[x1, y1];
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                StatusCell(x1, y1);
                return;
            }

            // Check if its a valid move
            if (selectedPiece.ValidMove(pieces, x2, y2)) {
                BreakFree(x1, y1);
                MovePiece(selectedPiece, x2, y2);
                selectedPiece.posiciones[0].xP=x2;
                selectedPiece.posiciones[0].yP=y2;
                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                StatusCell(x2, y2);
                selectedCell = casillas[x2, y2];
                selectedPiece = null;
              }
            else {
                MovePiece(selectedPiece, x1, y1);
                StatusCell(x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                selectedCell = casillas[x1, y1];
            }
            GameObject.Find("Code").GetComponent<GranjeroControllerAux>().WrapperCrearCaminos(); //aarb
            return;
        }
    }


    //llenar los vectores que tiene la posicon de los hijos de las piezas en base a la nueva posición
    
    //x2 y y2 son las posiciones de destino
    
    public bool CompareCell(int x1, int y1, int x2, int y2){
        
        Piece p1, p2;      
        Vector2 vec1, vec2;
        p1 = pieces[x1, y1];
        p2 = pieces[x2, y2];

        vec1.x = x1;
        vec1.y = y1;
        vec2.x = x2;
        vec2.y = y2;

        return((vec1 != vec2) ? false : true);
    }

    public void GenerateBoard() {
        int cont = 0;
        //Generar las casillas vacias
        for (int i = 0; i < ancho; i++) {
            for (int j = 0; j < alto; j++) {
                GenerateCell(i, j, cont);
                cont++;
            }
        }
        //colocar pieza donde se "iran" del mapa
        //puedo colocar 4 piezas de export pero cada granja solo debe enviar a una sola de ellas
        //deberia enviarse a la que le quede mas cerca
        
    }


    //Llena la matriz de las celdas en color verde (original)
    public void GenerateCell(int x, int y, int cont) {
        GameObject pr = Instantiate(CasillaPrefab, new Vector3(x, 0, y), Quaternion.identity);
        Casilla c = pr.GetComponent<Casilla>();
        pr.transform.SetParent(ColorCell.transform,false);
        c.GetComponent<Casilla>().numC = cont;

        casillas[x, y] = c;
        c.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + casillaOffset;

        //Desactivas el renderer de cada casilla
        StatusCell(x, y);
    }

    public void GeneratePiece(int x, int y, string id, int type) {


        if (pieces[x, y] == null) {
            //PSB   
            bool addok = GameObject.Find("Code").GetComponent<SociedadesController>().ValidarElementoSociedad(socGeneral, id); // addok indica si se puede colocar el componente o no.
            //PSB                                                                              
            if (addok)
            {
                cantidades[0] = cantidades[0] + 1;
                FarmPrefab = Resources.Load<GameObject>("Sprites/Prefabs/Comp/" + id);

                Material newMat = Resources.Load("diffuse", typeof(Material)) as Material;

#if UNITY_STANDALONE
                newMat.color = colorPC;
#elif UNITY_ANDROID
                                            newMat.color = colorA;
#endif

                FarmPrefab.GetComponent<Renderer>().material = newMat;

                GameObject go = Instantiate(FarmPrefab) as GameObject;
                go.transform.SetParent(transform);

                Piece p = go.GetComponent<Piece>();
                p.id = id;
                p.nombre = id;
                p.nombreUI = id;
                p.type = 1;
                p.sociedad = socGeneral;
                pieces[x, y] = p;
                //agregar posiciones a la lista
                pieces[x, y].posiciones.Add(new Posicion() { xP = x, yP = y });

                MovePiece(p, x, y);// Permite mover la pieza dentro del tablero, en este caso se usa para colocar las piezas dispersas en el tablero
                StatusCell(x, y);
                ComponentesMapa.Add(pieces[x, y]);


                if (id == "Export")
                {
                    //cuando se llama a crear una pieza ID export podria crear una pieza en una esquina del tablero o algo asi?
                    //y que cuando recorra cierto tiempo se vaya volviendo invisible y luego se destruya
                    GameObject salida = Instantiate(Resources.Load<GameObject>("Sprites/Prefabs/Comp/Salida")) as GameObject;
                    salida.transform.SetParent(transform);
                    Piece pE = salida.GetComponent<Piece>();

                    pE.id = "Export";
                    pE.nombre = "camino";
                    pieces[x, y] = pE;
                    MovePiece(pE, x, y);
                    ComponentesMapa.Add(pieces[x, y]);
                }


                Material newMat1 = new Material(Resources.Load("DiffuseMat", typeof(Material)) as Material);
                if (pieces[x, y].getSociedad() == 0)
                {
                    newMat1.color = colorPC;
                    //pieces[x, y].GetComponentInParent<Renderer>().material = newMat1;
                    pieces[x, y].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = newMat1.color;
                }
                else if (pieces[x, y].getSociedad() == 1)
                {
                    newMat1.color = new Color(0f, 0.263581f, 0.6981132f, 1f);
                    pieces[x, y].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = newMat1.color;
                }
                else if (pieces[x, y].getSociedad() == 2)
                {
                    newMat1.color = new Color(1f, 0.1118307f, 0f, 1f);
                    pieces[x, y].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = newMat1.color;
                    ComponentsDictionary(id, type);
                    //ComponentesMapa.Add(pieces[x, y]);
                }
            }
            else {
                //PSB ===================================================================================================================
                print("No puedes colocar el componente: " + type + "/" + socGeneral);
                GameObject Error = Instantiate(Resources.Load<GameObject>("ErrorSociedad"), GameObject.Find("Canvas").transform, false);
                Error.name = "ErrorSociedad";
                GameObject.Find(Error.name).transform.GetChild(0).transform.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Destroy(GameObject.Find("ErrorSociedad"));
                });
                //PSB ===================================================================================================================
            }


        }
        else {
            //PSB ===================================================================================================================
            if (id != "Export")
            {
                print("Esta posicion esta ocupada");
            }
            //PSB ===================================================================================================================

        }
    }

    public void MovePiece(Piece p, int x, int y) {
        Sprite spriteAct = Resources.Load<Sprite>("Sprites/Items/" + p.getIDPiece());
        SpriteRenderer spriteRenderer = p.GetComponent<SpriteRenderer>();
        if(p.getIDPiece()!="Export")
            spriteRenderer.sprite = spriteAct;

        Drag = false;
        p.transform.position = (Vector3.right * (x + 0.119f)) + (Vector3.up * 0.481f) + (Vector3.forward * (y + 0.12f)) + boardOffset;
       
    }
    
    public void StatusCell(int x, int y) {
        selectedCell = casillas[x, y];
        selectedCell.ChangeColor(pieces[x, y] == null, casillas, x, y);
        selectedCell.GetComponent<Renderer>().enabled = false;
    }

    //modifica el color a la casilla en las coordenadas dadas segun este ocupada o no
    public void BreakFree(int x1, int y1) {
        selectedCell = casillas[x1, y1];
        selectedCell.ChangeColor(true, casillas, x1, y1);
        selectedCell.GetComponent<Renderer>().enabled = false;
    }

 
    
    //corrutina que invoca el menu
    IEnumerator MenuOp() {
        Piece P2, Ps, p = pieces[x, y];

        Vector3 VectorMenu = Vector3.zero;

        if (p != null) {

            if (Menu.ready && Menu.ModificarXY && !EventSystem.current.IsPointerOverGameObject()) {//si el menu de la granja esta desactivado
                
                VectorMenu = p.transform.position;
              
                StartCoroutine(_CameraController.Center(VectorMenu));
                yield return new WaitForSeconds(1.5f);//tiempo que tarda en centrarse la camara para luego abrir el menu
                
                openX=0;
                openY=0;
                openX=(int)removeGO.x;
                openY=(int)removeGO.y;
                pOpenAux = pieces[openX,openY];
                Menu.DeployMenuPanel((int)removeGO.x, (int)removeGO.y);// se abre el menu

            }else if(!Menu.ready){

                bool close = false;
                Piece pOpen = pieces[openX,openY];

            	//if(p.getIDPiece() == "XPCRI1" && pOpenAux.getIDPiece() == "XPCRI1"){
                    close=(openX == mouseOver.x && openY == mouseOver.y);

                
                if(close){
                    Menu.DeployMenuPanel(openX,openY); //se cierra el menu 
                }   
            }	
            yield return null;
        }
    }

    IEnumerator dobleClick() {
        yield return new WaitForSeconds(clickTimer);

        if (contadorclicks > 1) {
            Zoom(x, y);
            esdobleclick = true;
        }
        else {
            esdobleclick = false;
        }

        yield return new WaitForSeconds(.05f);
        contadorclicks = 0;

        preguntopordobleclick = true;
    }

    public void Zoom(int x, int y) {
        if (x < 0 || x >= 24 || y < 0 || y >= 24)
            return;
        if(inv.activeInv==false && inv.activeCat==false){
            Piece p = pieces[x, y];;
            if (p != null) {
                StartCoroutine(_CameraController.CenterZoom(p.transform.position));

            }
            else {
                StartCoroutine(_CameraController.Reset());
            }
        }
        
    }
     public void ResetColor(int a, int b) { }
    //modifica el color al cargar un archivo
    public void AddDestino(int comp, string destino,bool val, string tipo){
        //Agregar destino al granjero que sale de un componente
        destinos[comp].Add(destino);
        destinosComp[comp].Add(new DestinoComponentes(destino,val,tipo));
    }

    public bool TableroNoEsVacio(){
        return (cantidades[0] > 0 || cantidades[1] > 0 || cantidades[2] > 0);
    }

    public void ComponentsDictionary(string id, int total) {

        if (ComponentsQ.ContainsKey(id)) {
            ComponentsQ[id] += 1;
        }
        else {
            ComponentsQ.Add(id, 1);
        }
        
    }
}
