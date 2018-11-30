using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Linq;

//esto es un fix

public class ControllerBoard : MonoBehaviour {

    //START PSB
    public SociedadesController SocController;
    //END PSB

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

    public ItemData item2;

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
    public List<Piece> ComponentesMapa; //Lista de pieza que guarda los componentes.

    Shader shader;
    Color colorA,colorPC;
    public Shader diffuse;

    Dictionary<string, int> ComponentsQ; //Diccionario de cantidad.
    public int socGeneral;

    private void Start() {

        //PSB ===================================================================================================================
        SocController = GameObject.Find("Code").GetComponent<SociedadesController>();
        //PSB ===================================================================================================================

        ComponentsQ = new Dictionary<string, int>();
        
        cantidades = new List<int>();
        cantidades.Add(0); //Posicion 0 inicializada en 0, Componente tipo 1
        cantidades.Add(0);
        cantidades.Add(0);
        ColorCell = GameObject.Find("ColorCell");
        item2 = GameObject.Find("Inventory").GetComponent<ItemData>();
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
        colorPC= new Color(0.7f, 0.7f, 0.7f, 1.00f);
        GenerateBoard();

        //sociedades
        socGeneral = 0; //si no se ha seleccionado otra sociedad, entonces es generica
        
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
                if(selectedPiece.type == 1){
                    TryMove((int)startDrag.x, (int)startDrag.y, x, y);
                }else if(selectedPiece.type == 2){
                    TryMoveP2((int)startDrag.x, (int)startDrag.y, x, y);
                    if (DisableSlave != null) {
                        DisableSlave.GetComponent<Collider>().enabled = true;
                    }
                }
                else if(selectedPiece.type == 3){
                    TryMoveP3((int)startDrag.x, (int)startDrag.y, x, y);
                    if (DisableSlave != null) {
                        DisableSlave.GetComponent<Collider>().enabled = true;
                    }
                }
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

    public IEnumerator RotComponente(int startx, int starty) {
        Piece p = pieces[startx, starty];
        if(p.getTypePiece()==2 || p.getTypePiece()==3){
            if (p.getName()=="Slave1" || p.getName()=="Slave2"){
                p = p.transform.parent.GetComponent<Piece>(); 
            }
        }
        
        bool band;

        band = UpdateRotatePositions(startx, starty);//primero se verifica si la pieza completa (incluyendo esclavos) se logra rotar, si es verdad
                                                     //se rota sin problemas, sino indica que la posicón de un esclavo esta ocupada.

        if (band) {
            float totalTime = 1.25f; // or whatever

            float startTime = Time.time;
            float endTime = startTime + totalTime;

            while (Time.time < endTime) {
                float timeSoFar = Time.time - startTime;
                float fractionTime = timeSoFar / totalTime;
                
                p.transform.rotation = Quaternion.Lerp(p.transform.rotation, Quaternion.Euler(0, (p.Rotation) * 90, 0), fractionTime);
                yield return null; // this goes IN HERE
            }

            if (p.Rotation == 3) { p.Rotation = 0; }
            else { p.Rotation++; }

            yield break;
        }
        else {
            print("Esta ocupado");
        }
    }

    public IEnumerator RotTablero() {
        
        float totalTime = 1.25f; // or whatever

        float startTime = Time.time;
        float endTime = startTime + totalTime;

        while (Time.time < endTime) {
            float timeSoFar = Time.time - startTime;
            float fractionTime = timeSoFar / totalTime;

            //do NOT USE underscores in variable names

            transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(0, (RotacionTablero + 1) * 90, 0), fractionTime);

            yield return null; // this goes IN HERE
        }

        if (RotacionTablero == 4) { RotacionTablero = 1; }
        else { RotacionTablero++; }
        UpdatePieces();
        yield break;
    }

  	public void UpdatePieces() {

        for (int i = 0; i < 23; i++) {
            for (int j = 0; j < 23; j++) {
                Piece p = pieces[i, j];
                //actualiza la lista de posiciones de cada pieza
                if (p != null && p.getName()!="camino") {
                    switch (p.getName()) {
                        case "XPCRI1":
                            int xP = p.posiciones[0].xP;
                            int yP = p.posiciones[0].yP;
                            
                            p.posiciones[0].xP = yP;
                            p.posiciones[0].yP = 23 - xP;
                            break;
                        case "XPCRI2":
                            xP = p.posiciones[0].xP;
                            yP = p.posiciones[0].yP;

                            p.posiciones[0].xP = yP;
                            p.posiciones[0].yP = 23 - xP;
                            p.posiciones[1].xP = yP;
                            p.posiciones[1].yP = (23 - xP) - 1;

                            break;
                        case "XPCRI3":
                            xP = p.posiciones[0].xP;
                            yP = p.posiciones[0].yP;

                            p.posiciones[0].xP = yP;
                            p.posiciones[0].yP = 23 - xP;
                            p.posiciones[1].xP = yP;
                            p.posiciones[1].yP = (23 - xP) - 1;
                            p.posiciones[2].xP = yP - 1;
                            p.posiciones[2].yP = (23 - xP) - 1;
                            break;
                        case "Slave1":
                            xP = p.posiciones[0].xP;
                            yP = p.posiciones[0].yP;

                            p.posiciones[0].xP = yP;
                            p.posiciones[0].yP = 23 - xP;

                            break;
                        case "Slave2":
                            xP = p.posiciones[0].xP;
                            yP = p.posiciones[0].yP;

                            p.SetX(yP);
                            p.SetY(23 - xP);

                            break;
                    }
                    int posX = p.posiciones[0].xP;
                    int posY = p.posiciones[0].yP;
                    piecesAux[posX, posY] = p;

                    //actualiza la rotacion del componente al rotar el tablero
                    if (p.Rotation == 3) {
                        p.Rotation = 0;
                    }
                    else {
                        p.Rotation = p.Rotation + 1;
                    }

                }// fin if   
            } //fin for indice j
        }//fin for indice i


        //vaciado de la matriz pieces
        for (int i = 0; i < 23; i++) {
            for (int j = 0; j < 23; j++) {
                pieces[i, j] = null;
            }
        }

        //actualizacion de la matriz pieces 
        for (int i = 0; i < 23; i++) {
            for (int j = 0; j < 23; j++) {
                if (piecesAux[i, j] != null) {
                    pieces[i, j] = piecesAux[i, j];
                    if (pieces[i, j].getName() != "Slave1" && pieces[i, j].getName() != "Slave2" && pieces[i, j].getName() != "camino") {
                        UpdatePositions(i, j);
                    }
                }
                StatusCell(i, j);
            }
        }

        //vaciado de la matriz auxiliar
        for (int i = 0; i < 23; i++) {
            for (int j = 0; j < 23; j++) {
                piecesAux[i, j] = null;
            }
        }
    }

    //visualizacion de la descripcion de cada componente
    public void ViewInfo(int x, int y) {
        panelInfo.SetActive(true);
        Piece pd = pieces[x, y];
        // la informacion mostrada esta contenida en el archivo json items
        if (pd.type == 1) {
            Item itemAux = database.FetchItemByID(0);
            info = itemAux.Info;
        }else if (pd.type == 2) {
            Item itemAux = database.FetchItemByID(1);
            info = itemAux.Info;
        }else if (pd.type == 3) {
            Item itemAux = database.FetchItemByID(2);
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
                if (itemAux.Spaces >= 2) {
                    selectedCell1 = casillas[equis + 1, ye];
                    selectedCell1.GetComponent<Renderer>().enabled = true;
                    if(itemAux.Spaces >= 3) {
                    selectedCell2 = casillas[equis+1, ye-1];
                    selectedCell2.GetComponent<Renderer>().enabled = true;
                    }
                }
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
            UpdateRotatePositions(startx, starty);
        }

    }

    //funcion usada para actualizar la lista de posiciones de las piezas al rotar el tablero
    public bool UpdatePositions(int startx, int starty) {
        p = pieces[startx, starty];
        //actualiza las rotaciones segun el tipo de componente y la rotacion actual del mismo
        if (p.getTypePiece() == 2) {
            if (p.Rotation == 1) {
                p.posiciones[1].xP = p.posiciones[0].xP;
                p.posiciones[1].yP = p.posiciones[0].yP-1;
                Debug.Log("Actualizando Rotacion 1 " + p.posiciones[0].xP + " " + p.posiciones[0].yP);
                Debug.Log("Actualizando Rotacion 1 " + p.posiciones[1].xP + " " + p.posiciones[1].yP);
            }
            else if (p.Rotation == 2) {
                p.posiciones[1].xP = p.posiciones[0].xP -1;
                p.posiciones[1].yP = p.posiciones[0].yP;
            }
            else if (p.Rotation == 3) {
                p.posiciones[1].xP = p.posiciones[0].xP;
                p.posiciones[1].yP = p.posiciones[0].yP + 1;
            }
            else if (p.Rotation == 0) {
                p.posiciones[1].xP = p.posiciones[0].xP+1;
                p.posiciones[1].yP = p.posiciones[0].yP;
            }
        }

        if (p.getTypePiece() == 3) {
            if (p.Rotation == 1) {
                p.posiciones[1].xP = p.posiciones[0].xP;
                p.posiciones[1].yP = p.posiciones[0].yP - 1;
                p.posiciones[2].xP = p.posiciones[0].xP - 1;
                p.posiciones[2].yP = p.posiciones[0].yP - 1;
            }
            else if (p.Rotation == 2) {
                p.posiciones[1].xP = p.posiciones[0].xP - 1;
                p.posiciones[1].yP = p.posiciones[0].yP;
                p.posiciones[2].xP = p.posiciones[0].xP - 1;
                p.posiciones[2].yP = p.posiciones[0].yP + 1;
            }
            else if (p.Rotation == 3) {
                p.posiciones[1].xP = p.posiciones[0].xP;
                p.posiciones[1].yP = p.posiciones[0].yP + 1;
                p.posiciones[2].xP = p.posiciones[0].xP + 1;
                p.posiciones[2].yP = p.posiciones[0].yP + 1;
            }
            else if (p.Rotation == 0) {
                p.posiciones[1].xP = p.posiciones[0].xP + 1 ;
                p.posiciones[1].yP = p.posiciones[0].yP;
                p.posiciones[2].xP = p.posiciones[0].xP + 1;
                p.posiciones[2].yP = p.posiciones[0].yP - 1;
            }
        }
        return true;
    }

    //actualiza la lista de posiciones de cada componente segun su tipo al ser rotado
    public bool UpdateRotatePositions(int startx, int starty) {
        bool rot = false;
        int xA, yA;
        Piece p = pieces[startx, starty];
        //actualiza las rotaciones en orden
        if (p.getTypePiece() == 1) {
            rot = true;
        }
        else if (p.getTypePiece() == 2) {
            if (p.getName()=="Slave1"){
                p = p.transform.parent.GetComponent<Piece>();  
            }
            vectorSlave.x = p.GetX(1);
            vectorSlave.y = p.GetY(1);
            SlaveAux1 = pieces[p.GetX(1), p.GetY(1)];
            //el xA y yA son las posiciones donde el esclavo se coloca al rotar, para cada caso es una posición diferente.
            //ants de rotar la pieza, se verifica si en la posición (xA,yA) esta libre para el esclavo, sino lo esta, no se rota la pieza
            if (p.Rotation == 0) {
                xA = p.posiciones[0].xP;
                yA = p.posiciones[0].yP - 1;

                if (pieces[xA, yA] == null) {
                    rot = true;
                    rotationS = true;
                    p.posiciones[1].xP = xA;
                    p.posiciones[1].yP = yA;

                    SlaveAux1.posiciones[0].xP = xA;
                    SlaveAux1.posiciones[0].yP = yA;

                    pieces[xA, yA] = SlaveAux1;
                    pieces[(int)vectorSlave.x, (int)vectorSlave.y] = null;
                    BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                }
                else {
                    rot = false;
                }
            }
            else if (p.Rotation == 1) {
                xA = p.posiciones[0].xP - 1;
                yA = p.posiciones[0].yP;

                if (pieces[xA, yA] == null) {
                    rot = true;
                    rotationS = false;
                    p.posiciones[1].xP = xA;
                    p.posiciones[1].yP = yA;
                    
                    SlaveAux1.posiciones[0].xP = xA;
                    SlaveAux1.posiciones[0].yP = yA;

                    pieces[xA, yA] = SlaveAux1;
                    pieces[(int)vectorSlave.x, (int)vectorSlave.y] = null;
                    BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                }
                else {
                    rot = false;
                }
            }
            else if (p.Rotation == 2) {
                xA = p.posiciones[0].xP;
                yA = p.posiciones[0].yP + 1;

                if (pieces[xA, yA] == null) {
                    rot = true;
                    rotationS = true;
                    p.posiciones[1].xP = xA;
                    p.posiciones[1].yP = yA;

                    SlaveAux1.posiciones[0].xP = xA;
                    SlaveAux1.posiciones[0].yP = yA;
                    
                    pieces[xA, yA] = SlaveAux1;
                    pieces[(int)vectorSlave.x, (int)vectorSlave.y] = null;
                    BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                }
                else {
                    rot = false;
                }
            }
            else if (p.Rotation == 3) {

                xA = p.posiciones[0].xP + 1;
                yA = p.posiciones[0].yP;

                if (pieces[xA, yA] == null) {
                    rot = true;
                    rotationS = false;
                    p.posiciones[1].xP = xA;
                    p.posiciones[1].yP = yA;

                    SlaveAux1.posiciones[0].xP = xA;
                    SlaveAux1.posiciones[0].yP = yA;
                    
                    pieces[xA, yA] = SlaveAux1;
                    pieces[(int)vectorSlave.x, (int)vectorSlave.y] = null;
                    BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                }
                else {
                    rot = false;
                }
            }
        }
        else if (p.getTypePiece() == 3) {

            if (p.getName()=="Slave1" || p.getName()=="Slave2"){
                p = p.transform.parent.GetComponent<Piece>();  
            }

            vectorSlave.x = p.GetX(1);
            vectorSlave.y = p.GetY(1);

            vectorSlave2.x = p.GetX(2);
            vectorSlave2.y = p.GetY(2);

            SlaveAux1 = pieces[p.GetX(1), p.GetY(1)];
            SlaveAux2 = pieces[p.GetX(2), p.GetY(2)];

            if (p.Rotation == 0) {
                xA = p.posiciones[0].xP;
                yA = p.posiciones[0].yP;

                if (p.ValidMove(pieces, xA, yA - 1) && p.ValidMove(pieces, xA - 1, yA - 1)) {

                    rot = true;

                    p.posiciones[1].xP = xA;
                    p.posiciones[1].yP = yA - 1;
                    p.posiciones[2].xP = xA - 1;
                    p.posiciones[2].yP = yA - 1;

                    SlaveAux1.posiciones[0].xP = xA;
                    SlaveAux1.posiciones[0].yP = yA-1;
                    SlaveAux2.posiciones[0].xP = xA-1;
                    SlaveAux2.posiciones[0].yP = yA-1;
                   
                    pieces[p.GetX(1), p.GetY(1)] = SlaveAux1;
                    pieces[p.GetX(2), p.GetY(2)] = SlaveAux2;
                    pieces[(int)vectorSlave.x, (int)vectorSlave.y] = null;
                    pieces[(int)vectorSlave2.x, (int)vectorSlave2.y] = null;
                    BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                    BreakFree((int)vectorSlave2.x, (int)vectorSlave2.y);
                }
                else {
                    rot = false;
                }
            }
            else if (p.Rotation == 1) {

                xA = p.posiciones[0].xP;
                yA = p.posiciones[0].yP;

                if (p.ValidMove(pieces, xA - 1, yA) && p.ValidMove(pieces, xA - 1, yA + 1)) {
                    rot = true;

                    p.posiciones[1].xP = xA - 1;
                    p.posiciones[1].yP = yA;
                    p.posiciones[2].xP = xA - 1;
                    p.posiciones[2].yP = yA + 1;

                    SlaveAux1.posiciones[0].xP = xA- 1;
                    SlaveAux1.posiciones[0].yP = yA;
                    SlaveAux2.posiciones[0].xP = xA-1;
                    SlaveAux2.posiciones[0].yP = yA+1;
                    
                    pieces[p.GetX(1), p.GetY(1)] = SlaveAux1;
                    pieces[p.GetX(2), p.GetY(2)] = SlaveAux2;
                    pieces[(int)vectorSlave.x, (int)vectorSlave.y] = null;
                    pieces[(int)vectorSlave2.x, (int)vectorSlave2.y] = null;
                    BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                    BreakFree((int)vectorSlave2.x, (int)vectorSlave2.y);
                }
                else {
                    rot = false;
                }
            }
            else if (p.Rotation == 2) {

                xA = p.posiciones[0].xP;
                yA = p.posiciones[0].yP;

                if (p.ValidMove(pieces, xA, yA + 1) && p.ValidMove(pieces, xA + 1, yA + 1)) {
                    rot = true;
                    rotationS = true;

                    p.posiciones[1].xP = xA;
                    p.posiciones[1].yP = yA + 1;
                    p.posiciones[2].xP = xA + 1;
                    p.posiciones[2].yP = yA + 1;

                    SlaveAux1.posiciones[0].xP = xA;
                    SlaveAux1.posiciones[0].yP = yA + 1;
                    SlaveAux2.posiciones[0].xP = xA + 1;
                    SlaveAux2.posiciones[0].yP = yA + 1;
                    
                    pieces[p.GetX(1), p.GetY(1)] = SlaveAux1;
                    pieces[p.GetX(2), p.GetY(2)] = SlaveAux2;
                    pieces[(int)vectorSlave.x, (int)vectorSlave.y] = null;
                    pieces[(int)vectorSlave2.x, (int)vectorSlave2.y] = null;
                    BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                    BreakFree((int)vectorSlave2.x, (int)vectorSlave2.y);
                }
                else {
                    rot = false;
                }
            }
            else if (p.Rotation == 3) {

                xA = p.posiciones[0].xP;
                yA = p.posiciones[0].yP;

                if (p.ValidMove(pieces, xA + 1, yA) && p.ValidMove(pieces, xA + 1, yA - 1)) {
                    rot = true;
                    rotationS = true;

                    p.posiciones[1].xP = xA + 1;
                    p.posiciones[1].yP = yA;
                    p.posiciones[2].xP = xA + 1;
                    p.posiciones[2].yP = yA - 1;

                    SlaveAux1.posiciones[0].xP = xA + 1;
                    SlaveAux1.posiciones[0].yP = yA;
                    SlaveAux2.posiciones[0].xP = xA + 1;
                    SlaveAux2.posiciones[0].yP = yA - 1;
                    
                    pieces[p.GetX(1), p.GetY(1)] = SlaveAux1;
                    pieces[p.GetX(2), p.GetY(2)] = SlaveAux2;
                    pieces[(int)vectorSlave.x, (int)vectorSlave.y] = null;
                    pieces[(int)vectorSlave2.x, (int)vectorSlave2.y] = null;
                    BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                    BreakFree((int)vectorSlave2.x, (int)vectorSlave2.y);
                }
                else {
                    rot = false;
                }
            }
        }
        return rot;
    }

    //destruye el GameObject(granja) que tiene el menu de opciones activo
    public void DestroyFarm(int startx, int starty) {
        Piece p = null;
        if(startx > 0 && starty > 0){ //aarb
            p = pieces[startx, starty];
        }
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
                    BreakFree(startx+1, starty);
                }
                else if (p.getTypePiece() == 3) {
                    cantidades[2] = cantidades[2] - 1;
                    BreakFree(startx, starty);
                    BreakFree(startx+1, starty);
                    BreakFree(startx+1, starty-1);
                }
                pieces[startx, starty] = null;

                if(ComponentsQ[idp] - 1 == 0){
                	ComponentsQ.Remove(idp);
                	print("Se elimino el elemento " + idp + " del diccionario");
                }else{
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
                GameObject.Find("Code").GetComponent<GranjeroControllerAux>().WrapperCrearCaminos(); //aarb
                //print("Eliminando: " + idp + " " + ComponentsQ[idp]);
        
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
                if(p.getTypePiece()!=1 && p.getTypePiece()!=2)//para la prueba con Sprites2D
                    p = Ps.transform.parent.GetComponent<Piece>();

            }
            else {
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    DisableSlave = p.gameObject.transform.GetChild(i).gameObject;
                    if (DisableSlave.name == "Slave1(Clone)") {
                        DisableSlave.GetComponent<Collider>().enabled = false;
                    }
                    if (DisableSlave.name == "Slave2(Clone)") {
                        DisableSlave.GetComponent<Collider>().enabled= false;
                    }
                }
            }
            if(p.getTypePiece()==1){
                Sprite spriteAct = Resources.Load<Sprite>("Sprites/Items/GranjaFlechas-01");
                SpriteRenderer spriteRenderer = p.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = spriteAct;
            }
            if(p.getTypePiece()==2){
                Sprite spriteAct = Resources.Load<Sprite>("Sprites/Items/GranjaFlechas-01");
                SpriteRenderer spriteRenderer = p.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = spriteAct;
            }
            if(p.getTypePiece()==3){
                Sprite spriteAct = Resources.Load<Sprite>("Sprites/Items/GranjaFlechas-01");
                SpriteRenderer spriteRenderer = p.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = spriteAct;
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
        if (p.getTypePiece() == 1){
            selectedCell = casillas[equis, ye];
            selectedCell.ChangeColor(true, casillas, startequis, startye);
            selectedCell.GetComponent<Renderer>().enabled = true;
        }else if (p.getTypePiece() == 2){
            BreakFree(p.posiciones[0].xP, p.posiciones[0].yP);
            BreakFree(p.posiciones[1].xP, p.posiciones[1].yP);

            selectedCell = casillas[equis, ye];
            selectedCell.ChangeColor(true, casillas, startequis, startye);
            selectedCell.GetComponent<Renderer>().enabled = true;

            GetChildren(p, equis, ye);

            selectedCell1 = casillas[(int)vectorAux.x, (int)vectorAux.y];

            GetChildren(p, startequis, startye);
            selectedCell1.ChangeColor(true, casillas, (int)vectorAux.x, (int)vectorAux.y);
            selectedCell1.GetComponent<Renderer>().enabled = true;  
        }else if (p.getTypePiece() == 3) {
            BreakFree(p.posiciones[0].xP, p.posiciones[0].yP);
            BreakFree(p.posiciones[1].xP, p.posiciones[1].yP);
            BreakFree(p.posiciones[2].xP, p.posiciones[2].yP);

            selectedCell = casillas[equis, ye];
            selectedCell.ChangeColor(true, casillas, startequis, startye);
            selectedCell.GetComponent<Renderer>().enabled = true;

            GetChildren(p, equis, ye);
            selectedCell1 = casillas[(int)vectorAux.x, (int)vectorAux.y];
            selectedCell2 = casillas[(int)vectorAux2.x, (int)vectorAux2.y];

            GetChildren(p, startequis, startye);
            selectedCell1.ChangeColor(true, casillas, (int)vectorAux.x, (int)vectorAux.y);
            selectedCell1.GetComponent<Renderer>().enabled = true;  
            selectedCell2.ChangeColor(true, casillas, (int)vectorAux2.x, (int)vectorAux2.y);
            selectedCell2.GetComponent<Renderer>().enabled = true;
        }

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
        if (x2 < 1 || x2 >= 23 || y2 < 1 || y2 >= 23) {//aarb
            if (selectedPiece != null) {
                MovePiece(selectedPiece, x1, y1);
                GameObject.Find("Code").GetComponent<GranjeroControllerAux>().WrapperCrearCaminos(); //aarb
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

    public void TryMoveP2(int x1, int y1, int x2, int y2) {

        bool bandAux;

        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        Piece AuxP = null;
        int xAux = -1, yAux = -1;

        // Out of bounds
        if (x2 < 1 || x2 >= 23 || y2 < 1 || y2 >= 23) {//aarb
            if (selectedPiece != null) {
                MovePiece(selectedPiece, x1, y1);
                GameObject.Find("Code").GetComponent<GranjeroControllerAux>().WrapperCrearCaminos(); //aarb
            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            StatusCell(x1, y1);
            return;
        }
       
        if (selectedPiece != null ){
            // If it has not moved   
            
            if(selectedPiece.gameObject.transform.childCount == 0){
                AuxP = selectedPiece.transform.parent.GetComponent<Piece>();

                //posicion anterior del padre
                xAux = AuxP.GetX(0);
                yAux = AuxP.GetY(0);  
                BreakFree(xAux, yAux);

                SlaveAux1 = selectedPiece;

                GetChildren(AuxP, x1, y1);
                
                selectedPiece = AuxP;

                selectedPiece.posiciones[0].xP = AuxP.posiciones[1].xP;
                selectedPiece.posiciones[0].yP = AuxP.posiciones[1].yP;
                selectedPiece.posiciones[1].xP = (int)vectorAux.x;
                selectedPiece.posiciones[1].yP = (int)vectorAux.y;

                pieces[xAux, yAux] = null;
                pieces[selectedPiece.GetX(0), selectedPiece.GetY(0)] = null;

                bandAux = ValidMoveSlave(AuxP, x1, y1);

            }else{
                AuxP = selectedPiece;

                //posicion anterior del padre
                xAux = AuxP.GetX(0);
                yAux = AuxP.GetY(0); 
                SlaveAux1 = pieces[selectedPiece.GetX(1), selectedPiece.GetY(1)];

                bandAux = false;

                pieces[selectedPiece.GetX(0), selectedPiece.GetY(0)] = null;
                pieces[selectedPiece.GetX(1), selectedPiece.GetY(1)] = null;
            }

            if (endDrag == startDrag) {
                //si entra aquí, es porque se sostiene desde un hijo
                if(bandAux){
                    GetChildren(selectedPiece, x1, y1);
                    MovePiece(AuxP, x1, y1);
                    pieces[x1, y1] = AuxP;

                    selectedPiece.posiciones[0].xP = x1;
                    selectedPiece.posiciones[0].yP = y1;

                    selectedPiece.posiciones[1].xP = (int)vectorAux.x;
                    selectedPiece.posiciones[1].yP = (int)vectorAux.y;
                    MovePiece(SlaveAux1,(int)vectorAux.x, (int)vectorAux.y);

                    pieces[(int)vectorAux.x, (int)vectorAux.y] = SlaveAux1;
                    StatusCell(selectedPiece.GetX(0), selectedPiece.GetY(0));
                    StatusCell(selectedPiece.GetX(1), selectedPiece.GetY(1));
                }else{
                    GetChildren(selectedPiece, xAux, yAux);

                    MovePiece(selectedPiece, xAux, yAux);
                    pieces[xAux, yAux] = selectedPiece;

                    selectedPiece.posiciones[1].xP = (int)vectorAux.x;
                    selectedPiece.posiciones[1].yP = (int)vectorAux.y;

                    selectedPiece.posiciones[0].xP = xAux;
                    selectedPiece.posiciones[0].yP = yAux;

                    MovePiece(SlaveAux1, selectedPiece.GetX(1), selectedPiece.GetY(1));
                    pieces[selectedPiece.GetX(1), selectedPiece.GetY(1)] = SlaveAux1;

                    StatusCell(selectedPiece.GetX(0), selectedPiece.GetY(0));
                    StatusCell(selectedPiece.GetX(1), selectedPiece.GetY(1));
                }
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }else
            
            //Verifica si es un movimiento valido, tanto del padre como del hijo
            if (ValidMoveSlave(selectedPiece, x2, y2)) {

                vectorSlave.x = selectedPiece.GetX(1);
                vectorSlave.y = selectedPiece.GetY(1);
                AuxP = pieces[selectedPiece.GetX(0), selectedPiece.GetY(0)];
            
                BreakFree(x1, y1);
                MovePiece(selectedPiece, x2, y2);
                pieces[x2, y2] = selectedPiece;
                
                StatusCell(x2, y2);

                //actualizar nueva posicion en la lista de posiciones
                pieces[x2, y2].posiciones[0].xP = x2;
                pieces[x2, y2].posiciones[0].yP = y2;

                pieces[x2, y2].posiciones[1].xP = (int)vectorAux.x;
                pieces[x2, y2].posiciones[1].yP = (int)vectorAux.y;
                
                SlaveAux1.posiciones[0].xP = (int)vectorAux.x;
                SlaveAux1.posiciones[0].yP = (int)vectorAux.y;

                MovePiece(SlaveAux1, (int)vectorAux.x, (int)vectorAux.y);
                pieces[(int)vectorAux.x, (int)vectorAux.y] = SlaveAux1;
                
                BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                StatusCell((int)vectorAux.x, (int)vectorAux.y);
            }else{

                if(bandAux){

                    GetChildren(selectedPiece, x1, y1);
                    MovePiece(selectedPiece, xAux, yAux);
                    pieces[xAux, yAux] = selectedPiece;
                    
                    selectedPiece.posiciones[0].xP = xAux;
                    selectedPiece.posiciones[0].yP = yAux;

                    selectedPiece.posiciones[1].xP = x1;
                    selectedPiece.posiciones[1].yP = y1;

                    MovePiece(SlaveAux1, x1, y1);
                    pieces[x1, y1] = SlaveAux1;
                    StatusCell(xAux, yAux);
                    StatusCell(x1, y1);
                    BreakFree((int)vectorAux.x, (int)vectorAux.y);

                }else{
                    GetChildren(selectedPiece, x1, y1);
                    MovePiece(selectedPiece, x1, y1);
                    pieces[x1, y1] = selectedPiece;
                    
                    StatusCell(x1, y1);

                    selectedPiece.posiciones[0].xP = x1;
                    selectedPiece.posiciones[0].yP = y1;

                    selectedPiece.posiciones[1].xP = (int)vectorAux.x;
                    selectedPiece.posiciones[1].yP = (int)vectorAux.y;

                    SlaveAux1.posiciones[0].xP = (int)vectorAux.x;
                    SlaveAux1.posiciones[0].yP = (int)vectorAux.y;

                    MovePiece(SlaveAux1, (int)vectorAux.x, (int)vectorAux.y);
                    pieces[(int)vectorAux.x, (int)vectorAux.y] = SlaveAux1;

                    StatusCell((int)vectorAux.x, (int)vectorAux.y);
                }
                startDrag = Vector2.zero;
            }
            
            vectorAux = Vector2.zero;
            selectedPiece = null;
            GameObject.Find("Code").GetComponent<GranjeroControllerAux>().WrapperCrearCaminos(); //aarb
            return;
        }
        
    }

    public void TryMoveP3(int x1, int y1, int x2, int y2) {

        bool bandAux;

        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        Piece AuxP = null;
        int xAux = -1, yAux = -1;

        // Out of bounds
        if (x2 < 1 || x2 >= 23 || y2 < 1 || y2 >= 23) {//aarb
            if (selectedPiece != null) {
                MovePiece(selectedPiece, x1, y1);
                GameObject.Find("Code").GetComponent<GranjeroControllerAux>().WrapperCrearCaminos(); //aarb
            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            StatusCell(x1, y1);
            return;
        }
       
        if (selectedPiece != null ){
            // If it has not moved   

            if(selectedPiece.gameObject.transform.childCount == 0){
                
                AuxP = selectedPiece.transform.parent.GetComponent<Piece>();

                //posicion original
                xAux = AuxP.GetX(0);
                yAux = AuxP.GetY(0);  

                BreakFree(xAux, yAux);
                BreakFree(AuxP.GetX(1), AuxP.GetY(1));
                BreakFree(AuxP.GetX(2), AuxP.GetY(2));

                SlaveAux1 = pieces[AuxP.GetX(1), AuxP.GetY(1)];
                SlaveAux2 = pieces[AuxP.GetX(2), AuxP.GetY(2)];

                pieces[AuxP.GetX(0), AuxP.GetY(0)] = null;
                pieces[AuxP.GetX(1), AuxP.GetY(1)] = null;
                pieces[AuxP.GetX(2), AuxP.GetY(2)] = null;

                GetChildren(AuxP, x1, y1);
                selectedPiece = AuxP;

                if(startDrag.x == AuxP.GetX(1) && startDrag.y == AuxP.GetY(1)){

                    selectedPiece.posiciones[0].xP = AuxP.posiciones[1].xP;
                    selectedPiece.posiciones[0].yP = AuxP.posiciones[1].yP;
                    
                }else if (startDrag.x == AuxP.GetX(2) && startDrag.y == AuxP.GetY(2)){

                    selectedPiece.posiciones[0].xP = AuxP.posiciones[2].xP;
                    selectedPiece.posiciones[0].yP = AuxP.posiciones[2].yP;
                }

                selectedPiece.posiciones[1].xP = (int)vectorAux.x;
                selectedPiece.posiciones[1].yP = (int)vectorAux.y;
                selectedPiece.posiciones[2].xP = (int)vectorAux2.x;
                selectedPiece.posiciones[2].yP = (int)vectorAux2.y;

                pieces[xAux, yAux] = null;

                bandAux = ValidMoveSlave(AuxP, x1, y1);    

            }else{
                AuxP = selectedPiece;
                
                //posicion anterior del padre
                xAux = AuxP.GetX(0);
                yAux = AuxP.GetY(0); 
                GetChildren(AuxP, xAux, yAux);
                SlaveAux1 = pieces[AuxP.GetX(1), AuxP.GetY(1)];
                SlaveAux2 = pieces[AuxP.GetX(2), AuxP.GetY(2)];
                
                BreakFree(AuxP.GetX(0), AuxP.GetY(0));
                BreakFree(AuxP.GetX(1), AuxP.GetY(1));
                BreakFree(AuxP.GetX(2), AuxP.GetY(2));

                pieces[AuxP.GetX(0), AuxP.GetY(0)] = null;
                pieces[AuxP.GetX(1), AuxP.GetY(1)] = null;
                pieces[AuxP.GetX(2), AuxP.GetY(2)] = null;
                

                bandAux = false;
            }

            if (endDrag == startDrag) {

                //si entra aquí, es porque se sostiene desde un hijo
                if(bandAux){
                    GetChildren(AuxP, x1, y1);

                    MovePiece(AuxP, x1, y1);
                    pieces[x1, y1] = AuxP;

                    selectedPiece.posiciones[0].xP = x1;
                    selectedPiece.posiciones[0].yP = y1;

                    selectedPiece.posiciones[1].xP = (int)vectorAux.x;
                    selectedPiece.posiciones[1].yP = (int)vectorAux.y;

                    selectedPiece.posiciones[2].xP = (int)vectorAux2.x;
                    selectedPiece.posiciones[2].yP = (int)vectorAux2.y; 
                    
                    SlaveAux1.posiciones[0].xP=(int)vectorAux.x;
                    SlaveAux1.posiciones[0].yP=(int)vectorAux.y;

                    MovePiece(SlaveAux1,(int)vectorAux.x, (int)vectorAux.y);
                    pieces[(int)vectorAux.x, (int)vectorAux.y] = SlaveAux1;

                    SlaveAux2.posiciones[0].xP=(int)vectorAux2.x;
                    SlaveAux2.posiciones[0].yP=(int)vectorAux2.y;

                    MovePiece(SlaveAux2,(int)vectorAux2.x, (int)vectorAux2.y);
                    pieces[(int)vectorAux2.x, (int)vectorAux2.y] = SlaveAux2;

                    StatusCell(selectedPiece.GetX(0), selectedPiece.GetY(0));
                    StatusCell(selectedPiece.GetX(1), selectedPiece.GetY(1));
                    StatusCell(selectedPiece.GetX(2), selectedPiece.GetY(2));
                }else{
                    
                    GetChildren(selectedPiece, xAux, yAux);

                    MovePiece(selectedPiece, xAux, yAux);
                    pieces[xAux, yAux] = selectedPiece;

                    selectedPiece.posiciones[2].xP = (int)vectorAux2.x;
                    selectedPiece.posiciones[2].yP = (int)vectorAux2.y;

                    selectedPiece.posiciones[1].xP = (int)vectorAux.x;
                    selectedPiece.posiciones[1].yP = (int)vectorAux.y;

                    selectedPiece.posiciones[0].xP = xAux;
                    selectedPiece.posiciones[0].yP = yAux;

                    SlaveAux1.posiciones[0].xP=(int)vectorAux.x;
                    SlaveAux1.posiciones[0].yP=(int)vectorAux.y;

                    MovePiece(SlaveAux1,(int)vectorAux.x, (int)vectorAux.y);
                    pieces[(int)vectorAux.x, (int)vectorAux.y] = SlaveAux1;

                    SlaveAux2.posiciones[0].xP=(int)vectorAux2.x;
                    SlaveAux2.posiciones[0].yP=(int)vectorAux2.y;

                    MovePiece(SlaveAux2,(int)vectorAux2.x, (int)vectorAux2.y);
                    pieces[(int)vectorAux2.x, (int)vectorAux2.y] = SlaveAux2;

                    StatusCell(selectedPiece.GetX(0), selectedPiece.GetY(0));
                    StatusCell(selectedPiece.GetX(1), selectedPiece.GetY(1));
                    StatusCell(selectedPiece.GetX(2), selectedPiece.GetY(2));
                }
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }else
            
            //Verifica si es un movimiento valido, tanto del padre como del hijo
            if (ValidMoveSlave(selectedPiece, x2, y2)) {
                
                vectorSlave.x = selectedPiece.GetX(1);
                vectorSlave.y = selectedPiece.GetY(1);

                vectorSlave2.x = selectedPiece.GetX(2);
                vectorSlave2.y = selectedPiece.GetY(2);

                BreakFree(x1, y1);

                MovePiece(AuxP, x2, y2);
                pieces[x2, y2] = AuxP;
                
                StatusCell(x2, y2);

                //actualizar nueva posicion en la lista de posiciones
                pieces[x2, y2].posiciones[0].xP = x2;
                pieces[x2, y2].posiciones[0].yP = y2;

                pieces[x2, y2].posiciones[1].xP = (int)vectorAux.x;
                pieces[x2, y2].posiciones[1].yP = (int)vectorAux.y;

                pieces[x2, y2].posiciones[2].xP = (int)vectorAux2.x;
                pieces[x2, y2].posiciones[2].yP = (int)vectorAux2.y;
                
                SlaveAux1.posiciones[0].xP=(int)vectorAux.x;
                SlaveAux1.posiciones[0].yP=(int)vectorAux.y;

                MovePiece(SlaveAux1, (int)vectorAux.x, (int)vectorAux.y);
                pieces[(int)vectorAux.x, (int)vectorAux.y] = SlaveAux1;

                SlaveAux2.posiciones[0].xP=(int)vectorAux2.x;
                SlaveAux2.posiciones[0].yP=(int)vectorAux2.y;
                MovePiece(SlaveAux2, (int)vectorAux2.x, (int)vectorAux2.y);
                pieces[(int)vectorAux2.x, (int)vectorAux2.y] = SlaveAux2;

                BreakFree((int)vectorSlave.x, (int)vectorSlave.y);
                StatusCell((int)vectorAux.x, (int)vectorAux.y);

                BreakFree((int)vectorSlave2.x, (int)vectorSlave2.y);
                StatusCell((int)vectorAux2.x, (int)vectorAux2.y);

            }else{

                if(bandAux){
                    
                    GetChildren(selectedPiece, xAux, yAux);
                    MovePiece(selectedPiece, xAux, yAux);
                    pieces[xAux, yAux] = selectedPiece;

                    selectedPiece.posiciones[0].xP = xAux;
                    selectedPiece.posiciones[0].yP = yAux;

                    selectedPiece.posiciones[1].xP = (int)vectorAux.x;
                    selectedPiece.posiciones[1].yP = (int)vectorAux.y;

                    selectedPiece.posiciones[2].xP = (int)vectorAux2.x;
                    selectedPiece.posiciones[2].yP = (int)vectorAux2.y;

                    SlaveAux1.posiciones[0].xP=(int)vectorAux.x;
                    SlaveAux1.posiciones[0].yP=(int)vectorAux.y;
                    MovePiece(SlaveAux1, (int)vectorAux.x, (int)vectorAux.y);
                    pieces[(int)vectorAux.x, (int)vectorAux.y] = SlaveAux1;

                    SlaveAux2.posiciones[0].xP=(int)vectorAux2.x;
                    SlaveAux2.posiciones[0].yP=(int)vectorAux2.y;
                    MovePiece(SlaveAux2, (int)vectorAux2.x, (int)vectorAux2.y);
                    pieces[(int)vectorAux2.x, (int)vectorAux2.y] = SlaveAux2;

                    StatusCell(xAux, yAux);
                    StatusCell((int)vectorAux.x, (int)vectorAux.y);
                    StatusCell((int)vectorAux2.x, (int)vectorAux2.y);                  

                }else{
                    
                    GetChildren(selectedPiece, x1, y1);
                    MovePiece(selectedPiece, x1, y1);
                    pieces[x1, y1] = selectedPiece;

                    selectedPiece.posiciones[0].xP = x1;
                    selectedPiece.posiciones[0].yP = y1;

                    selectedPiece.posiciones[1].xP = (int)vectorAux.x;
                    selectedPiece.posiciones[1].yP = (int)vectorAux.y;

                    selectedPiece.posiciones[2].xP = (int)vectorAux2.x;
                    selectedPiece.posiciones[2].yP = (int)vectorAux2.y;

                    SlaveAux1.posiciones[0].xP=(int)vectorAux.x;
                    SlaveAux1.posiciones[0].yP=(int)vectorAux.y;
                    MovePiece(SlaveAux1, (int)vectorAux.x, (int)vectorAux.y);
                    pieces[(int)vectorAux.x, (int)vectorAux.y] = SlaveAux1;

                    SlaveAux2.posiciones[0].xP=(int)vectorAux2.x;
                    SlaveAux2.posiciones[0].yP=(int)vectorAux2.y;
                    MovePiece(SlaveAux2, (int)vectorAux2.x, (int)vectorAux2.y);
                    pieces[(int)vectorAux2.x, (int)vectorAux2.y] = SlaveAux2;

                    StatusCell(x1, y1);
                    StatusCell((int)vectorAux.x, (int)vectorAux.y);
                    StatusCell((int)vectorAux2.x, (int)vectorAux2.y);
                }
                startDrag = Vector2.zero;
            }
            
            vectorAux = Vector2.zero;
            vectorAux2 = Vector2.zero;
            selectedPiece = null;
            GameObject.Find("Code").GetComponent<GranjeroControllerAux>().WrapperCrearCaminos(); //aarb
            return;
        }
    }


    //llenar los vectores que tiene la posicon de los hijos de las piezas en base a la nueva posición
    public void GetChildren(Piece p, int x2, int y2){

        //bool bandS = false;
        int rot = p.Rotation;
        //Piece p2;

        vectorAux = Vector2.zero;
        vectorAux2 = Vector2.zero;

        switch(rot){
            case 0: 
                
                if (p.getTypePiece() == 2){

                    vectorAux.x = x2 + 1;
                    vectorAux.y = y2;

                }else if (p.getTypePiece() == 3){                    
                    
                    vectorAux.y = y2;
                    vectorAux.x = x2 + 1;
                    vectorAux2.x = x2 + 1;
                    vectorAux2.y = y2 - 1;
                }
            break;

            case 1:
                if (p.getTypePiece() == 2){
 
                    vectorAux.x = x2;
                    vectorAux.y = y2 - 1;

                }else if (p.getTypePiece() == 3){
  
                    vectorAux.x = x2;
                    vectorAux.y = y2 - 1;
                    vectorAux2.x = x2 - 1;
                    vectorAux2.y = y2 - 1;
                }
            break;

            case 2:
                if (p.getTypePiece() == 2){
                    
                    vectorAux.x = x2 - 1;
                    vectorAux.y = y2;

                }else if (p.getTypePiece() == 3){
                    
                    vectorAux.x = x2 - 1;
                    vectorAux.y = y2;
                    vectorAux2.x = x2 - 1;
                    vectorAux2.y = y2 + 1;
                }
            break;

            case 3:
                if (p.getTypePiece() == 2){
                    
                    vectorAux.x = x2;
                    vectorAux.y = y2 + 1;

                }else if (p.getTypePiece() == 3){
                    
                    vectorAux.x = x2;
                    vectorAux.y = y2 + 1;
                    vectorAux2.x = x2 + 1;
                    vectorAux2.y = y2 + 1;
                }
            break;
        }

    }
    //x2 y y2 son las posiciones de destino
    public bool ValidMoveSlave(Piece p, int x2, int y2){

        bool bandS = false;
        int rot = p.Rotation;
        //Piece p2;

        vectorAux = Vector2.zero;
        vectorAux2 = Vector2.zero;

        switch(rot){
            case 0: 
                if(p.getTypePiece() == 1){

                    bandS = true;
                }else if (p.getTypePiece() == 2){

                    bandS = selectedPiece.ValidMove(pieces, x2, y2);
                    if(bandS){
                        bandS = selectedPiece.ValidMove(pieces, (x2 + 1), y2);
                    }

                    vectorAux.x = x2 + 1;
                    vectorAux.y = y2;

                }else if (p.getTypePiece() == 3){
                    bandS = selectedPiece.ValidMove(pieces, x2, y2);
                    if(bandS){
                        bandS = selectedPiece.ValidMove(pieces, x2 + 1, y2);
                        if(bandS){
                            bandS = selectedPiece.ValidMove(pieces, x2 + 1, y2 - 1);
                        }
                    }
                    vectorAux.x = x2 + 1;
                    vectorAux.y = y2;
                    vectorAux2.x = x2 + 1;
                    vectorAux2.y = y2 - 1;
                }
            break;

            case 1:
                if(p.getTypePiece() == 1){

                    bandS = true;
                }else if (p.getTypePiece() == 2){

                    bandS = selectedPiece.ValidMove(pieces, x2, y2);
                    if(bandS){
                        bandS = selectedPiece.ValidMove(pieces, x2, y2 - 1);
                    }
                  
                    vectorAux.x = x2;
                    vectorAux.y = y2 - 1;

               }else if (p.getTypePiece() == 3){

                    bandS = selectedPiece.ValidMove(pieces, x2, y2);
                    if(bandS){
                        bandS = selectedPiece.ValidMove(pieces, x2, y2 - 1);
                        if(bandS){
                            bandS = selectedPiece.ValidMove(pieces, x2 - 1, y2 - 1);
                        }
                    }
                    
                    vectorAux.x = x2;
                    vectorAux.y = y2 - 1;
                    vectorAux2.x = x2 - 1;
                    vectorAux2.y = y2 - 1;
                }
            break;

            case 2:
                if(p.getTypePiece() == 1){

                    bandS = true;
                }else if (p.getTypePiece() == 2){
                    
                    bandS = selectedPiece.ValidMove(pieces, x2, y2);
                    if(bandS){
                        bandS = selectedPiece.ValidMove(pieces, x2 - 1, y2);
                    }
                    
                    vectorAux.x = x2 - 1;
                    vectorAux.y = y2;

                }else if (p.getTypePiece() == 3){
                    
                    bandS = selectedPiece.ValidMove(pieces, x2, y2);
                    if(bandS){
                        bandS = selectedPiece.ValidMove(pieces, x2 - 1, y2);
                        if(bandS){
                            bandS = selectedPiece.ValidMove(pieces, x2 - 1, y2 + 1);
                        }
                    }
                    
                    vectorAux.x = x2 - 1;
                    vectorAux.y = y2;
                    vectorAux2.x = x2 - 1;
                    vectorAux2.y = y2 + 1;
                }
            break;

            case 3:
                if(p.getTypePiece() == 1){

                    bandS = true;
                }else if (p.getTypePiece() == 2){
                    
                    bandS = selectedPiece.ValidMove(pieces, x2, y2);
                    if(bandS){
                        bandS = selectedPiece.ValidMove(pieces, x2, y2 + 1);
                    }
                    
                    vectorAux.x = x2;
                    vectorAux.y = y2 + 1;

                }else if (p.getTypePiece() == 3){
                    
                    bandS = selectedPiece.ValidMove(pieces, x2, y2);
                    if(bandS){
                        bandS = selectedPiece.ValidMove(pieces, x2, y2 + 1);
                        if(bandS){
                            bandS = selectedPiece.ValidMove(pieces, x2 + 1, y2 + 1);
                        }
                    }
 
                    vectorAux.x = x2;
                    vectorAux.y = y2 + 1;
                    vectorAux2.x = x2 + 1;
                    vectorAux2.y = y2 + 1;
                }
            break;
        }
        return (bandS);
    }
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
        //GeneratePiece(10, 10, "XPCRI1", 1);
        //GeneratePiece(10, 13, "XPCRI2", 2);
        //GeneratePiece(13, 10, "XPCRI3");
        GeneratePiece(1,11,"Export", 0);
        GeneratePiece(11,22,"Export", 0);
        GeneratePiece(22,11,"Export", 0);
        GeneratePiece(11,1,"Export", 0);
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

        //PSB ===================================================================================================================
        bool addok = SocController.ValidarElementoSociedad(socGeneral, id); // addok indica si se puede colocar el componente o no.
        //PSB ===================================================================================================================

        if (addok)
        {
            if (pieces[x, y] == null)
            {
                if (type == 1)
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
                    p.sociedad = socGeneral; //aarb
                    pieces[x, y] = p;
                    //agregar posiciones a la lista
                    pieces[x, y].posiciones.Add(new Posicion() { xP = x, yP = y });

                    MovePiece(p, x, y);// Permite mover la pieza dentro del tablero, en este caso se usa para colocar las piezas dispersas en el tablero
                    StatusCell(x, y);
                    ComponentesMapa.Add(pieces[x, y]);
                    Debug.Log("creada en sociedad: " + pieces[x, y].getSociedad());
                }
                else if (type == 2 && pieces[x + 1, y] == null)
                {
                    cantidades[1] = cantidades[1] + 1;

                    FarmPrefab2 = Resources.Load<GameObject>("Sprites/Prefabs/Comp/" + id);

                    Material newMat = Resources.Load("diffuse", typeof(Material)) as Material;
#if UNITY_STANDALONE
                    newMat.color = colorPC;
#elif UNITY_ANDROID
                     newMat.color = colorA;
#endif


                    FarmPrefab2.GetComponent<Renderer>().material = newMat;

                    GameObject go = Instantiate(FarmPrefab2) as GameObject;
                    go.transform.SetParent(transform);
                    Piece p = go.GetComponent<Piece>();

                    Slave1 = Resources.Load<GameObject>("Sprites/Prefabs/Comp/Slave");
                    Slave1.name = "Slave1";
                    GameObject Aux = Instantiate(Slave1) as GameObject;
                    Aux.transform.SetParent(p.transform);//probando el transform sobre una variable de tipo Piece (funciona :D)
                    Piece SlaveAux1 = Aux.GetComponent<Piece>();

                    SlaveAux1.GetComponent<Renderer>().enabled = false;

                    p.id = id;
                    p.nombre = id;
                    p.nombreUI = id;

                    SlaveAux1.id = id;
                    SlaveAux1.nombre = "Slave1";
                    SlaveAux1.nombreUI = id;
                    p.type = 2;
                    p.sociedad = socGeneral; //aarb
                    pieces[x, y] = p;

                    //se agregan ambas posiciones a la lista
                    pieces[x, y].posiciones.Add(new Posicion() { xP = x, yP = y });
                    pieces[x, y].posiciones.Add(new Posicion() { xP = x + 1, yP = y });

                    //se agrega posicion del esclavo a la lista
                    SlaveAux1.posiciones.Add(new Posicion() { xP = x + 1, yP = y });
                    pieces[(x + 1), y] = SlaveAux1;

                    MovePiece(p, x, y);// Permite mover la pieza dentro del tablero, en este caso se usa para colocar las piezas dispersas en el tableroy
                    MovePiece(SlaveAux1, x + 1, y);
                    StatusCell(x, y);
                    StatusCell(x + 1, y);
                    ComponentesMapa.Add(pieces[x, y]);
                    Debug.Log("creada en sociedad: " + pieces[x, y].getSociedad());
                }
                else if (type == 3 && pieces[x + 1, y] == null && pieces[x + 1, y - 1] == null)
                {
                    cantidades[2] = cantidades[2] + 1;
                    FarmPrefab3 = Resources.Load<GameObject>("Sprites/Prefabs/Comp/" + id);
                    Material newMat = Resources.Load("diffuse", typeof(Material)) as Material;
                #if UNITY_STANDALONE
                                    newMat.color = colorPC;
                #elif UNITY_ANDROID
                                    newMat.color = colorA;
                #endif

                    FarmPrefab3.GetComponent<Renderer>().material = newMat;

                    GameObject go = Instantiate(FarmPrefab3) as GameObject;
                    go.transform.SetParent(transform);
                    Piece p = go.GetComponent<Piece>();

                    Slave1 = Resources.Load<GameObject>("Sprites/Prefabs/Comp/Slave");
                    Slave1.name = "Slave1";
                    GameObject Aux = Instantiate(Slave1) as GameObject;
                    Aux.transform.SetParent(p.transform);//probando el transform sobre una variable de tipo Piece (funciona :D)
                    Piece SlaveAux1 = Aux.GetComponent<Piece>();

                    Slave2 = Resources.Load<GameObject>("Sprites/Prefabs/Comp/Slave");
                    Slave2.name = "Slave2";

                    Aux = Instantiate(Slave2) as GameObject;
                    Aux.transform.SetParent(p.transform, true);//probando el transform sobre una variable de tipo Piece (funciona :D)
                    Piece SlaveAux2 = Aux.GetComponent<Piece>();
                    Aux = null;

                    SlaveAux1.GetComponent<Renderer>().enabled = false;
                    SlaveAux2.GetComponent<Renderer>().enabled = false;

                    p.id = id;
                    p.nombre = id;
                    p.nombreUI = id;

                    SlaveAux1.id = id;
                    SlaveAux1.nombre = "Slave1";
                    SlaveAux1.nombreUI = id;

                    SlaveAux2.id = id;
                    SlaveAux2.nombre = "Slave2";
                    SlaveAux2.nombreUI = id;
                    p.type = 3;
                    p.sociedad = socGeneral; //aarb
                    pieces[x, y] = p;

                    //agregar posiciones a la lista //3 espacios
                    pieces[x, y].posiciones.Add(new Posicion() { xP = x, yP = y });
                    pieces[x, y].posiciones.Add(new Posicion() { xP = x + 1, yP = y });
                    pieces[x, y].posiciones.Add(new Posicion() { xP = x + 1, yP = y - 1 });

                    SlaveAux1.posiciones.Add(new Posicion() { xP = x + 1, yP = y });
                    SlaveAux2.posiciones.Add(new Posicion() { xP = x + 1, yP = y - 1 });

                    pieces[pieces[x, y].GetX(1), pieces[x, y].GetY(1)] = SlaveAux1;
                    pieces[pieces[x, y].GetX(2), pieces[x, y].GetY(2)] = SlaveAux2;

                    MovePiece(p, x, y);// Permite mover la pieza dentro del tablero, en este caso se usa para colocar las piezas dispersas en el tableroy
                    MovePiece(SlaveAux1, x + 1, y);
                    MovePiece(SlaveAux2, x + 1, y - 1);
                    StatusCell(x, y);
                    StatusCell(pieces[x, y].GetX(1), pieces[x, y].GetY(1));
                    StatusCell(pieces[x, y].GetX(2), pieces[x, y].GetY(2));
                    ComponentesMapa.Add(pieces[x, y]);
                    Debug.Log("creada en sociedad: " + pieces[x, y].getSociedad());
                }
                else if (id == "Export")
                {
                    //cuando se llama a crear una pieza ID export podria crear una pieza en una esquina del tablero o algo asi?
                    //y que cuando recorra cierto tiempo se vaya volviendo invisible y luego se destruya
                    GameObject salida = Instantiate(Resources.Load<GameObject>("Sprites/Prefabs/Comp/Salida")) as GameObject;
                    salida.transform.SetParent(transform);
                    Piece pE = salida.GetComponent<Piece>();

                    pE.id = "Export";
                    pE.nombre = "camino";
                    pE.sociedad = -1;
                    pieces[x, y] = pE;
                    MovePiece(pE, x, y);
                    ComponentesMapa.Add(pieces[x, y]);
                }
                if (invisible)
                {
                    Invisible(x, y);
                }
                ComponentsDictionary(id, type);


                //cambiar color segun el tipo de sociedad
                Material newMat1 = new Material(Resources.Load("diffuse", typeof(Material)) as Material);
                if (pieces[x, y].getSociedad() == 0)
                {
                    newMat1.color = colorPC;
                    pieces[x, y].GetComponentInParent<Renderer>().material = newMat1;
                }
                else if (pieces[x, y].getSociedad() == 1)
                {
                    newMat1.color = new Color(0f, 0.263581f, 0.6981132f, 1f);
                    pieces[x, y].GetComponentInParent<Renderer>().material = newMat1;
                }
                else if (pieces[x, y].getSociedad() == 2)
                {
                    newMat1.color = new Color(1f, 0.1118307f, 0f, 1f);
                    pieces[x, y].GetComponentInParent<Renderer>().material = newMat1;
                }
                //ComponentesMapa.Add(pieces[x,y]);
                //Debug.Log(ComponentesMapa.Count);//borrar
                //ComponentesMapa.Last().printPiece();
            }
        }
        else
        {
            //PSB ===================================================================================================================
            if (id != "Export")
            {
                print("No puedes colocar el componente: " + type + "/" + socGeneral);
                GameObject Error = Instantiate(Resources.Load<GameObject>("ErrorSociedad"), GameObject.Find("Canvas").transform, false);
                Error.name = "ErrorSociedad";
                GameObject.Find(Error.name).transform.GetChild(0).transform.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Destroy(GameObject.Find("ErrorSociedad"));
                });
            }
            //PSB ===================================================================================================================
        }

    }

    public void MovePiece(Piece p, int x, int y) {
        if(p.getIDPiece()=="XPCRI1"){
            Sprite spriteAct = Resources.Load<Sprite>("Sprites/Items/XPCRI1");
            SpriteRenderer spriteRenderer = p.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = spriteAct;
        }
        if(p.getName()=="XPCRI2"){
            Sprite spriteAct = Resources.Load<Sprite>("Sprites/Items/XPCRI2");
            SpriteRenderer spriteRenderer = p.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = spriteAct;
        }
        if(p.getName()=="XPCRI3"){
            Sprite spriteAct = Resources.Load<Sprite>("Sprites/Items/XPCRI3");
            SpriteRenderer spriteRenderer = p.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = spriteAct;
        }
        
        Drag = false;
        if(p.getIDPiece()=="XPCRI1"){
            p.transform.position = (Vector3.right * (x+0.120f)) +  (Vector3.up * 0.505f) +(Vector3.forward * (y+0.12f))+ boardOffset;
        }else if(p.getIDPiece()=="XPCRI2"){
            p.transform.position = (Vector3.right * (x+0.43599f)) +  (Vector3.up * 0.50009f) +(Vector3.forward * (y-0.09599f))+ boardOffset;
        }else{
            p.transform.position = (Vector3.right * (x+0.120f)) +  (Vector3.up * 0.505f) +(Vector3.forward * (y+0.12f))+ boardOffset;
            //p.transform.position = (Vector3.right * x) +(Vector3.forward * y) + boardOffset + pieceOffset;
        }
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

    //vuelve invisible la superficie del componente seleccionado dadas sus coordenadas (es usada en el menu del componente)
    //es usada al crear un pieza
    public void Invisible(int x, int y) {
        Piece Componente = pieces[x, y];
        for (int i = 0; i < Componente.gameObject.transform.childCount; i++) {
            GameObject g = Componente.gameObject.transform.GetChild(i).gameObject;
            rend = g.GetComponent<Renderer>();

            if (rend && g.name != "GranjaPiso1") {
                foreach (Material m in rend.materials) {

                    m.SetFloat("_Mode", 2);
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = 3000;
                }
                iTween.FadeTo(g, 0, 1);//original
            }
        }
    }

    //vuelve invisible la superficie del componente dado (es usada en el menu principal)
    public void InvisibleAlls(Piece Componente) {
        for (int i = 0; i < Componente.gameObject.transform.childCount; i++) {
            GameObject g = Componente.gameObject.transform.GetChild(i).gameObject;
            rend = g.GetComponent<Renderer>();

            if (rend && g.name != "GranjaPiso1") {
                foreach (Material m in rend.materials) {
                    m.SetFloat("_Mode", 2);
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = 3000;
                }
                iTween.FadeTo(g, 0, 1);
            }
        }
    }
    
    
    //corrutina que invoca el menu
    IEnumerator MenuOp() {
        Piece P2, Ps, p = pieces[x, y];

        Vector3 VectorMenu = Vector3.zero;

        if (p != null) {

            if (Menu.ready && Menu.ModificarXY && !EventSystem.current.IsPointerOverGameObject()) {//si el menu de la granja esta desactivado
                
                if(p.getIDPiece() == "XPCRI1"){ 
                    VectorMenu = p.transform.position;
                }else if(p.getIDPiece() == "XPCRI2"){

                    if(p.gameObject.transform.childCount == 0){
                        //si la pieza no posee un hijo, es un "esclavo", por ende se toman los valores del padre (pivote).
                        Ps = p.transform.parent.GetComponent<Piece>();//en este caso Ps toma los valores del padre
                        

                    }else{
                        //Si la pieza resulta ser el padre, debemos tomar los valores del hijo
                        Ps = pieces[p.GetX(1), p.GetY(1)];//En este caso Ps toma los valores del hijo
                   }

                    VectorMenu.x = (Ps.transform.position.x + p.transform.position.x)/2;
                    VectorMenu.y = (Ps.transform.position.y + p.transform.position.y)/2;
                    VectorMenu.z = (Ps.transform.position.z + p.transform.position.z)/2;

                }else if(p.getIDPiece() == "XPCRI3"){

                    if(p.gameObject.transform.childCount == 0){
                        //si la pieza no posee un hijo, es un "esclavo", por ende se toman los valores del padre (pivote).
                        Ps = p.transform.parent.GetComponent<Piece>();//en este caso Ps toma los valores del padre

                        //si la pieza no posee un hijo, es un "esclavo", por ende se toman los valores del padre (pivote).
                        Ps = p.transform.parent.GetComponent<Piece>();//en este caso Ps toa los valores del padre
                        
                        p = pieces[Ps.GetX(1), Ps.GetY(1)];
                        P2 = pieces[Ps.GetX(2), Ps.GetY(2)]; 

                    }else{
                        //Si la pieza resulta ser el padre, debemos tomar los valores del hijo
                        Ps = pieces[p.GetX(1), p.GetY(1)];//En este caso Ps toma los valores del hijo
                        P2 = pieces[p.GetX(2), p.GetY(2)]; 
                    }

                    VectorMenu.x = (Ps.transform.position.x + p.transform.position.x + P2.transform.position.x)/3;
                    VectorMenu.y = (Ps.transform.position.y + p.transform.position.y + P2.transform.position.y)/3;
                    VectorMenu.z = (Ps.transform.position.z + p.transform.position.z + P2.transform.position.z)/3;
                }

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

            	if(p.getIDPiece() == "XPCRI1" && pOpenAux.getIDPiece() == "XPCRI1"){
                    close=(openX == mouseOver.x && openY == mouseOver.y);

                }else if(p.getIDPiece() == "XPCRI2" && pOpenAux.getIDPiece() == "XPCRI2"){
                    if(pOpen.posiciones.Count >1){
                        int sl1X=pOpen.posiciones[1].xP;
                        int sl1Y=pOpen.posiciones[1].yP;
                    
                        close=((openX == mouseOver.x && openY == mouseOver.y)||(sl1X == mouseOver.x && sl1Y == mouseOver.y));
                    }
                }else if(p.getIDPiece() == "XPCRI3" && pOpenAux.getIDPiece() == "XPCRI3"){

                    if(pOpen.posiciones.Count >2){
                        int sl1X=pOpen.posiciones[1].xP;
                        int sl1Y=pOpen.posiciones[1].yP;
                        int sl2X=pOpen.posiciones[2].xP;
                        int sl2Y=pOpen.posiciones[2].yP;
                    
                        close=((openX == mouseOver.x && openY == mouseOver.y)||(sl1X == mouseOver.x && sl1Y == mouseOver.y)||(sl2X == mouseOver.x && sl2Y == mouseOver.y));
                    }
                }
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

    //reincia el color un componente
    public void ResetColor(int xN, int yN) {
        Piece p = pieces[xN, yN];
        for (int i = 0; i < p.gameObject.transform.childCount; i++) {
            GameObject g = p.gameObject.transform.GetChild(i).gameObject;
            rend = g.GetComponent<Renderer>();
            if (g.name == "GranjaMaderaBlanca") {
                Material newMat = Resources.Load("MaderaBlanca", typeof(Material)) as Material;
                g.GetComponent<Renderer>().material = newMat;
            }
            if (g.name == "GranjaTecho") {
                Material newMat = Resources.Load("TechoGranjaBasica", typeof(Material)) as Material;
                g.GetComponent<Renderer>().material = newMat;
            }
            if (g.name == "GranjaBardas") {
                Material newMat = Resources.Load("Bardas", typeof(Material)) as Material;
                g.GetComponent<Renderer>().material = newMat;
            }
        }
    }

    //modifica el color al cargar un archivo
    public void ChangeColorAtLoad(int xAux, int yAux, string color) {

        isWhite = false;
        Piece p = pieces[xAux, yAux];

        switch (color) {//lee el id y segun el tipo de id toma acciones diferentes
            case "Black":
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("Black", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                        ResetColor(xAux, yAux);
                    }
                }
                break;
            case "Blue":
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("Blue", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                        ResetColor(xAux, yAux);
                    }
                }
                break;
            case "Brown":
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("Brown", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                        ResetColor(xAux, yAux);
                    }
                }
                break;
            case "Orange":
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("Orange", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                        ResetColor(xAux, yAux);
                    }

                }
                break;
            case "Pink":
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("Pink", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                        ResetColor(xAux, yAux);
                    }
                }
                break;
            case "Purple":
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("Purple", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                        ResetColor(xAux, yAux);
                    }
                }
                break;
            case "Red":
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("BaseGranjaBasica", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                        ResetColor(xAux, yAux);
                    }
                }
                break;
            case "Yellow":
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("Yellow", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                        ResetColor(xAux, yAux);
                    }
                }
                break;
            case "White":
                isWhite = true;
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("White", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                    }
                    if (g.name == "GranjaMaderaBlanca") {
                        Material newMat = Resources.Load("Madera", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                    }
                    if (g.name == "GranjaTecho") {
                        Material newMat = Resources.Load("Dalmata", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                    }
                    if (g.name == "GranjaBardas") {
                        Material newMat = Resources.Load("Brown2", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                    }
                }
                break;
            case "Gray":
                for (int i = 0; i < p.gameObject.transform.childCount; i++) {
                    GameObject g = p.gameObject.transform.GetChild(i).gameObject;
                    rend = g.GetComponent<Renderer>();
                    if (g.name == "GranjaBase") {
                        Material newMat = Resources.Load("Gray", typeof(Material)) as Material;
                        g.GetComponent<Renderer>().material = newMat;
                        ResetColor(xAux, yAux);
                    }
                }
                break;
        }//fswitch
    }
    public void AddDestino(int comp, string destino,bool val, string tipo){
        //Agregar destino al granjero que sale de un componente
        destinos[comp].Add(destino);
        destinosComp[comp].Add(new DestinoComponentes(destino,val,tipo));
    }

    public bool TableroNoEsVacio(){
        return (cantidades[0] > 0 || cantidades[1] > 0 || cantidades[2] > 0);
    }

    public void ComponentsDictionary(string id, int total){

    	if(ComponentsQ.ContainsKey(id)){
    		ComponentsQ[id] += 1;
    	}else{
    		ComponentsQ.Add(id, 1);
    	}

    	//print("Holis Diccionario: " + id + " " + ComponentsQ[id]);
    }
}
