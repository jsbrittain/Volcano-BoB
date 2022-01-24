using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class GameController : MonoBehaviour
{

    private static int levelNo = 1;
    [SerializeField] private int debugLevelNo = 0;      // 0=ignore
    private int lastLevel = 3;

    [SerializeField]
    private GameObject marker;

    [SerializeField]
    private GameObject island;

    [SerializeField]
    private List<GameObject> propPrefab;

    [SerializeField]
    private GameObject islanderPrefab;

    [SerializeField]
    private GameObject hutPrefab;
    private List<GameObject> huts = new List<GameObject>();

    [SerializeField]
    private GameObject treePrefab;

    [SerializeField]
    private GameObject propParent;

    public List<GameObject> props;

    [SerializeField]
    private GameObject volcanoGO;
    private VolcanoController volcano;

    [SerializeField]
    private GameObject boat;

    [SerializeField]
    private float islandRadius = 15f;

    private Vector3 cameraOrigin;
    private float maxShakeAngle = 0.8f;

    [SerializeField]
    private GameObject fadeOut;

    [SerializeField]
    private Text sacrificesUIText;
    [SerializeField]
    private Text levelInstructionUIText;

    private float dropHeight = 2f;
    private float propKillDrop = -1f;
    private float spawnsPerSecond = 1/2.5f;

    private Vector3[] islandVertices;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        // Get references
        audioSource = GetComponent<AudioSource>();

        // Report level number (static variable)
        if ( debugLevelNo > 0 )
            levelNo = debugLevelNo;     // Override level number for debugging purposes
        Debug.Log("Level No: " + levelNo);

        // Start volcano
        volcano = volcanoGO.GetComponent<VolcanoController>();

        // Set level parameters
        int noPropsInScene = 5, noIslanderInScene = 3, noHutsInScene = 4, noTreesInScene = 0;
        string levelInstr = "UNKNOWN LEVEL";
        switch (levelNo)
        {
            case 1:
                noPropsInScene = 1;
                noIslanderInScene = 3;
                noHutsInScene = 4;
                noTreesInScene = 3;
                volcano.setMaxAnger(75);
                volcano.setSacrificesRequired(3);
                volcano.setAppeaseCoolDown(1000f);
                levelInstr = "Volcano Irlene is calm\nMake 3 sacrifices and leave on the boat\nIrlene likes livestock, but sacrificing people makes her angry";
                break;
            case 2:
                noPropsInScene = 7;
                noIslanderInScene = 3;
                noHutsInScene = 4;
                noTreesInScene = 10;
                volcano.setMaxAnger(100);
                volcano.setSacrificesRequired(7);
                levelInstr = "Volcano Sam is waking up\nMake 7 sacrifice then escape while you can!";
                break;
            case 3:
                noPropsInScene = 7;
                noIslanderInScene = 8;
                noHutsInScene = 10;
                noTreesInScene = 25;
                volcano.setMaxAnger(80);
                volcano.setSacrificesRequired(12);
                levelInstr = "Volcano B.o.B is not easily appeased!\nMake 12 sacrifices before time runs out!";
                break;
            case 99:        // Test level
                noPropsInScene = 0;
                noIslanderInScene = 0;
                noHutsInScene = 0;
                noTreesInScene = 1;
                volcano.setMaxAnger(100);
                volcano.setSacrificesRequired(10);
                volcano.setAppeaseCoolDown(1000f);
                levelInstr = "Test level";
                break;
        }
        levelInstructionUIText.text = levelInstr;

        // Convert island mesh to world for faster access later
        markIslandMeshSurface();

        // Spawn objects
        for ( int k = 0; k < noPropsInScene; k++ )
            AddPropToScene();
        for (int k = 0; k < noIslanderInScene; k++)
            AddIslanderToScene();
        for (int k = 0; k < noHutsInScene; k++)
            AddHutToScene();
        for (int k = 0; k < noTreesInScene; k++)
            AddTreeToScene();

        // Initialise camera position
        cameraOrigin = Camera.main.transform.position;

        // Update GUI text
        updateUIText();

        // Start with volcano speaking
        audioSource.PlayOneShot(volcano.getAudioGreeting());
    }

    // Update is called once per frame
    void Update()
    {
        // Camera shake based on anger levels
        float shakeamount;
        Vector3 pos;
        if (isVolcanoAppeased())
        {
            volcano.resetAnger();
            pos = Vector3.zero;
        }
        else
        {
            shakeamount = maxShakeAngle * volcano.getAngerProportion();
            pos = new Vector3(Random.Range(-shakeamount, shakeamount), Random.Range(-shakeamount, shakeamount), 0f);
        }
        Camera.main.transform.position = cameraOrigin + pos;

        // Check for end of level --- Has the volcano erupted?
        if ( volcano.hasErupted() )
        {
            GameOver();
        }

        // Check if each prop has fallen and destroy if so (needs to be done here to monitor prop list)
        foreach ( GameObject prop in props.ToArray() )
        {
            if (prop.transform.position.y < propKillDrop)
            {
                props.Remove(prop);
                Destroy(prop);
            }
        }

        // Spawn new objects based on spawn timers
        if (Random.Range(0f, 1f) < (spawnsPerSecond * Time.deltaTime))
        {
            SpawnRandomItemFromHut();
        }

    }

    private void SpawnRandomItemFromHut()
    {
        // Select random hut to spawn from
        Vector3 pos = huts[Random.Range(0, huts.Count - 1)].transform.position;

        // Select random item to spawn
        switch (Random.Range(0, 5))    // [min,max)
        {
            case 0:     // Prop (i.e. goat)
            case 1:
                AddPropToScene(pos);
                break;
            case 2:
            case 3:
            case 4:     // Islander         // Twice as many islanders
                AddIslanderToScene(pos);
                break;
        }
    }

    private Vector3 findValidSpawnLocation()
    {
        Vector2 circlePos = 0.85f * islandRadius * Random.insideUnitCircle;
        Vector3 pos = new Vector3(circlePos.x, dropHeight, circlePos.y);
        pos.z = -Mathf.Abs(pos.z);      // Force into front half of island (away from volcano)
        return pos;
    }

    private void AddPropToScene( Vector3 pos )
    {
        int index = Random.Range(0, propPrefab.Count);
        pos.y = GetIslandGroundLevel(pos) + 0.1f;
        GameObject prop = Instantiate(propPrefab[index], pos, Quaternion.identity, propParent.transform);
        props.Add(prop);
    }

    private void AddPropToScene()
    {
        AddPropToScene(findValidSpawnLocation());
    }

    private void AddIslanderToScene(Vector3 pos)
    {
        pos.y = GetIslandGroundLevel(pos) + 0.1f;
        GameObject islander = Instantiate(islanderPrefab, pos, Quaternion.identity, propParent.transform);
        islander.GetComponent<PropController>().setColor(new Color(Random.Range(0.0f,1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
        props.Add(islander);
        return;
    }

    private void AddIslanderToScene()
    {
        AddIslanderToScene(findValidSpawnLocation());
    }

        private void AddHutToScene()
    {
        Vector3 pos = findValidSpawnLocation();
        pos.y = GetIslandGroundLevel(pos) - 0.1f;
        Quaternion rot = Quaternion.Euler(0, Random.Range(-180f,180f), 0);
        GameObject hut = Instantiate(hutPrefab, pos, rot, propParent.transform);
        huts.Add(hut);
        return;
    }

    private void AddTreeToScene()
    {
        Vector3 pos = findValidSpawnLocation();
        pos.y = GetIslandGroundLevel(pos) - 0.1f;
        GameObject hut = Instantiate(treePrefab, pos, Quaternion.identity, propParent.transform);
        return;
    }

    public void SacrificeProp( PropController propController )
    {
        volcano.giveSacrifice();
        if ( volcano.isAppeased() )
            audioSource.PlayOneShot(volcano.getAudioAppeased());
        updateUIText();
        volcano.appease(propController.appeaseAmount);
        props.Remove(propController.gameObject);
        Destroy(propController.gameObject);
        return;
    }

    private void updateUIText()
    {
        sacrificesUIText.text = string.Format("Sacrifices : {0} / {1}", volcano.getSacrificed(), volcano.sacrificedRequired());
    }

    public List<GameObject> getPropsList()
    {
        return props;
    }

    public void PassLevel()
    {
        FadeController fadeController = fadeOut.GetComponentInChildren<FadeController>();
        if (levelNo >= lastLevel)
        {
            levelNo = 1;
            fadeController.FadeToLevel("GameOverWin");
        }
        else
        {
            levelNo += 1;
            fadeController.FadeToLevel("Level1");
        }
    }

    public void GameOver()
    {
        levelNo = 1;        // Reset to first level
        FadeController fadeController = fadeOut.GetComponentInChildren<FadeController>();
        fadeController.FadeToLevel("GameOverLose");
    }

    public GameObject getBoat()
    {
        return boat;
    }

    public bool isVolcanoAppeased()
    {
        return volcano.isAppeased();
    }

    public void playerDied()
    {
        volcano.setAngerToMax();
        return;
    }

    private float GetIslandGroundLevel(Vector3 pos)
    {
        // Get the island ground level at the location specified by (x,z) ignoring y
        float dst, mindst = Mathf.Infinity;
        int index = 0;
        for (int i = 0; i < islandVertices.Length; i++)
        {
            dst = Vector3.Distance(pos, islandVertices[i]);
            if ( dst < mindst )
            {
                mindst = dst;
                index = i;
            }
        }

        // Debug --- place markers at starting location, and nearest mesh point
        //Instantiate(marker, pos, Quaternion.identity);
        //Instantiate(marker, islandVertices[index], Quaternion.identity);

        return islandVertices[index].y;
    }

    private void markIslandMeshSurface()
    {
        // Get the island ground level at the location specified by (x,z) ignoring y
        islandVertices = island.GetComponentInChildren<MeshFilter>().mesh.vertices;
        for (var i = 0; i < islandVertices.Length; i++)
        {
            // Translate to world coordinates
            islandVertices[i] = island.GetComponentInChildren<MeshFilter>().gameObject.transform.TransformPoint(islandVertices[i]);

            // Debug - mark vertices in world
            //Instantiate(marker, (island.GetComponentInChildren<MeshFilter>().gameObject.transform.TransformPoint(vertices[i])), Quaternion.identity);
        }

        return;
    }

    public void audioPlay(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
