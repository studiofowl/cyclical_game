using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundShapesLogic : MonoBehaviour {

    // this is clearly the most important factor for performance and should not be too high for now
    public int numOfGraphics = 200;
    // decides if for the "sketchy" effect if the shapes should change more than just position every time interval
    // (it's probably way too costly to be worth it in its current state)
    public bool changesAttributes = true;
    // the space within (or outside) the camera where the shapes will appear (0-1 is within camera view)
    public double cameraMin = 0.3;
    public double cameraMax = 0.7;
    // set the range of colors that the shapes can be to be between the values of two colors
    public Color minColorValues = Color.black;
    public Color maxColorValues = Color.white;
    // set the opacity for all shapes, constant
    // seems to effect performance significantly
    public float opacity = 10;
    // list of the different types of shapes that can be spawned in
    public GameObject[] graphicTypes;
    // set the scale multiplier for all shapes (shapes have their own scale values but are all multipled by this)
    public float scaleFactor = 1;
    // time between each time the sketchy effect is applied
    public float timeBetweenAlteringGraphics = 1f;
    // decides if the sketchy effect should be used at all
    public bool sketchyOn = true;

    // list of all the shapes (not altered after initialization)
    private List<GameObject> allGraphics;
    // list of the lists that distinguish what move group each graphic is in which decides
    // when they are adjusted to produce the sketchy effect
    // (not adjusted all at once to improve performance)
    private List<List<GameObject>> listOfMoveObjectLists;
    // dictionary conncecting each shape GameObject with its SpriteRenderer so GetComponent<>() is only called
    // once during initialization
    private Dictionary<GameObject, SpriteRenderer> srDict;

    // keep track of the sketchy effect coroutine so that it can be turned off in editor
    private Coroutine alterGraphicsCo;
    // keep track of if the sketchy effect coroutine is on so that it can be turned off and back on in editor
    private bool coroutineOn = true;

    void Start()
    {
        // init lists and dictionaries needed
        allGraphics = new List<GameObject>();
        listOfMoveObjectLists = new List<List<GameObject>>();
        for (int i = 0; i <= 3; i++) {
            listOfMoveObjectLists.Add(new List<GameObject>());
        }
        srDict = new Dictionary<GameObject, SpriteRenderer>();
        // run the function to generate all the shapes
        // running this in real time causes major drops in frame rate
        // needs to be optmized or just done during a loading screen
        SpawnGraphics(numOfGraphics);
        // begin the sketchy effect coroutine
        alterGraphicsCo = StartCoroutine(AlterGraphicsCo());
    }

    void Update() {
        // logic to turn sketchy effect coroutine on or off when editing
        SketchyCoLogic();

        // checks to see if any graphic is located outside the set range based off of the camera
        // and alter the graphic if it is
        foreach (GameObject graphic in allGraphics) {
            if (!IsVisibleToCamera(graphic.transform)) {
                AlterGraphic(graphic);
            }
        }
    }

    public bool IsVisibleToCamera(Transform transform)
    {
        // uses Unity's camera as a reference to see if shapes are in the right place
        Vector3 visTest = Camera.main.WorldToViewportPoint(transform.position);
        return (visTest.x >= cameraMin && visTest.y >= cameraMin) && (visTest.x <= cameraMax && visTest.y <= cameraMax) && visTest.z >= 0;
    }

    void AlterGraphic(GameObject graphic)
    {
        // sets the graphic's position to a random position in the set range based around the camera's position
        Vector3 graphicPos = Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(Screen.width * (float)cameraMin, Screen.width * (float)cameraMax), Random.Range(Screen.height * (float)cameraMin, Screen.height * (float)cameraMax), 100));
        graphic.transform.position = graphicPos;
        // change rotation, scale, and color 
        if (changesAttributes)
        {
            SetGraphicAttributes(graphic);
        }
    }

    void SpawnGraphics(int numOfGraphicsNeeded)
    {
        for (int i = 0; i < numOfGraphicsNeeded; i++)
        {
            // sets the graphic's position to a random position in the set range based around the camera's position
            Vector3 graphicPos = Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(Screen.width * (float)cameraMin, Screen.width * (float)cameraMax), Random.Range(Screen.height * (float)cameraMin, Screen.height * (float)cameraMax), 100));
            // randomly chooses what type of shape the graphic will be
            GameObject randomGraphic = graphicTypes[Random.Range(0, graphicTypes.Length)];
            // generate the graphic as a new GameObject
            GameObject newGraphic = Instantiate(randomGraphic, graphicPos, Quaternion.identity);
            // get the graphics SpriteRenderer and add it to the dictionary for changing the color
            srDict.Add(newGraphic, newGraphic.GetComponent<SpriteRenderer>());
            // set graphic as child of original script's GameObject
            newGraphic.transform.SetParent(transform);
            // set rotation, scale, and color
            SetGraphicAttributes(newGraphic);
            // randomly decide what move list the graphic is placed in which decided when it is altered
            // for sketchy effect
            listOfMoveObjectLists[Random.Range(0, listOfMoveObjectLists.Count)].Add(newGraphic);
            // add graphic GameObject to list of graphics
            allGraphics.Add(newGraphic);
        }
    }

    void SetGraphicAttributes(GameObject newGraphic)
    {
        // use random number generation to change the rotation, scale, and color of the given graphic
        // doesn't produce much of a difference in most situations when used with sketchy effect
        // only need to change z value of euelerAngles for 2D rotation
        newGraphic.transform.eulerAngles = new Vector3(0, 0, Random.Range(0f, 360f));
        Vector3 theScale = newGraphic.transform.localScale;

        // uses a random number to decide if the shape should have a normal scale
        if (Random.Range(1, 3) == 1)
        {
            // only generate one magnitude for both x and y scale to create default shape
            float scaleMagnitude = Random.Range(1f, 3f) * scaleFactor;
            theScale.x = scaleMagnitude;
            theScale.y = scaleMagnitude;
        }
        else
        {
            // generate a different magnitude for x and y to distort shape
            theScale.x = Random.Range(1f, 3f) * scaleFactor;
            theScale.y = Random.Range(1f, 3f) * scaleFactor;
        }
        newGraphic.transform.localScale = theScale;
        // sets the color of the graphic based on the two different colors selected for a range
        srDict[newGraphic].color = new Color(Random.Range(minColorValues.r, maxColorValues.r), Random.Range(minColorValues.g, maxColorValues.g), Random.Range(minColorValues.b, maxColorValues.b), opacity / 255f);
    }

    void AlterAllGraphicsInList(List<GameObject> graphicsList) {
        // alter all graphics in list
        foreach (GameObject graphic in graphicsList) {
            AlterGraphic(graphic);
        }
    }

    void SketchyCoLogic() {
        // logic to turn sketchy effect coroutine on or off when editing
        if (!sketchyOn && coroutineOn) {
            StopCoroutine(alterGraphicsCo);
            coroutineOn = false;
        }
        else if (sketchyOn && !coroutineOn) {
            alterGraphicsCo = StartCoroutine(AlterGraphicsCo());
            coroutineOn = true;
        }
    }

    IEnumerator AlterGraphicsCo() {
        // sketchy effect is going on as long as it is turned on
        while (sketchyOn) {
            // at every set time interval, alter all the positions of the graphics in one of the move lists
            // which decide when a graphic will be altered
            for (int i = 0; i < listOfMoveObjectLists.Count; i++) {
                yield return new WaitForSeconds(timeBetweenAlteringGraphics);
                AlterAllGraphicsInList(listOfMoveObjectLists[i]);
            }
        }
        coroutineOn = false;
    }
}