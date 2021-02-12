using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Threading.Tasks;

public class BuildWorld : MonoBehaviour {

    public enum ContainerType { Box, Cylinder };

    [Header("Container")]
    public ContainerType containerType = ContainerType.Box;
    //string ContainerName = "Cylinder";
    public Vector3 ContainerScale = new Vector3(1, 1, 1);
    public bool modifyScaleAutomatically = false;
    public float bufferLength = 0f;
    [Range(0, 0.99f)]
    public float estimatedPorosity = .30f;
    [Range(0,1)]
    public float containerDynamicFriction = 0.6f;
    [Range(0, 1)]
    public float containerStaticFriction = 0.6f;
    [Range(0, 1)]
    public float containerBounciness = 0f;
    PhysicMaterialCombine containerFrictionCombine = PhysicMaterialCombine.Average;
    PhysicMaterialCombine containerBounceCombine = PhysicMaterialCombine.Average;

    GameObject Container;
    GameObject userCamera;
    public Color backgroundColor = Color.black;
    bool isContainerVisible = true;

    [Header("Simulation Parameters")]
    public float FixedDeltaTime = .01f;
    [Range(1,100)]
    public int SolverIterationCount = 100;
    public int velocitySolverIterationCount = 30;
    public float SleepThreshold = .5f;
    public float BounceThreshold = 20f;
    public float TimeScale = 5f;
    public int TargetFrameRate = 30;
    public float contactOffset = .001f;

    [Header("Shaking Parameters")]
    public bool shakeTransversly = false;
    public bool shakeRotationaly = false;
    public bool stabilizeCamera = false;
    public float ShakingFraction = .002f;
    public float ShakingRotationFraction = .002f;
    Shaker shaker;

    [Header("Folders Parameters")]
    public string projectFolderPath = "./";
    public string pdfFolderName = "PDFs";
    public string saveFolderName = "Output";
    public bool createNewFolderAutomatically;
    public bool overrideOutputFolder;

    [Header("Particle Groups")]
    public bool useVolumeProportion = false;
    public int grainCountGoal = 1000;
    public Bed[] beds;

    [Header("Deposion Parameters")]
    public bool automaticDepostion = false;
    public float depostionRatePerSec = 10;
    //public bool autoFixedSolverDeltaTimeAtEnd = false;
    float lastGenerationTime = 0;

    [Header("Saving Parameters")]
    public bool saveRockAutomatically = false;
    public bool saveDataFile = true;
    public bool saveRockFile = true;
    public bool saveGrainsFile = true;
    public bool exitAutomatically = false;

    [Header("Rock")]
    Rock rock;

    bool cementationActive = false;
    bool disappearActive = false;

    bool saveActive = false;
    bool exitActive = false;
    bool saveStatus = false;


    [Header("Project")]
    public bool autoLoadConfigFile = false;
    public string projectNotes;
    public string configFileName = "Parameters.txt";

    // UI
    GameObject uiText;

    //===================================================================
    // Use this for initialization
    void Start () {

        Screen.fullScreen = false;

        // UI
        uiText = GameObject.FindGameObjectWithTag("UI");        


        Application.runInBackground = true;
        Application.targetFrameRate = TargetFrameRate;

        // Load configuration file
        if (autoLoadConfigFile == true)
        {
            LoadConfigurationFile();
        }


        // Construct rock
        rock = new Rock(beds, grainCountGoal, projectFolderPath, pdfFolderName, useVolumeProportion);
        
        // Construct container
        string ContainerName = containerType.ToString();
        GameObject ContainerPrefab = Resources.Load("Containers/" + ContainerName) as GameObject;
        ContainerPrefab.GetComponent<ContainerOperations>().setContainerScale(ContainerScale);
        ContainerPrefab.GetComponent<ContainerOperations>().bufferLength = bufferLength;

        if (modifyScaleAutomatically == true)
        {
            float grainVolume = rock.GetVolume() * rock.grainCountGoal;
            Debug.Log(grainVolume);
            ContainerScale = ContainerPrefab.GetComponent<ContainerOperations>().GetScaleFromVolume(grainVolume, estimatedPorosity);
            Debug.Log(ContainerScale);
            ContainerPrefab.GetComponent<ContainerOperations>().setContainerScale(ContainerScale);
        }

        ContainerPrefab.GetComponent<ContainerOperations>().containerDynamicFriction = containerDynamicFriction;
        ContainerPrefab.GetComponent<ContainerOperations>().containerStaticFriction = containerStaticFriction;
        ContainerPrefab.GetComponent<ContainerOperations>().containerBounciness = containerBounciness;
        ContainerPrefab.GetComponent<ContainerOperations>().containerFrictionCombine = containerFrictionCombine;
        ContainerPrefab.GetComponent<ContainerOperations>().containerBounceCombine = containerBounceCombine;
        Container = Instantiate(ContainerPrefab);

        // Attach Camera
        userCamera = Camera.main.gameObject;
        userCamera.GetComponent<CameraController>().Container = Container;
        userCamera.GetComponent<CameraController>().initializeCamera(Container.GetComponent<ContainerOperations>().getInitialCameraPosition(), Container.GetComponent<ContainerOperations>().getInitialLookLocation());
        userCamera.GetComponent<CameraController>().isStabilizeCamera = stabilizeCamera;

        // Load Beds and Grains
        rock.container = Container;
        Container.GetComponent<ContainerOperations>().rock = rock;

        // Attach Shaker to Container
        shaker = new Shaker();
        shaker.container = Container;
        shaker.rock = rock;

        OnValidate();

    }
    //===================================================================
    // Update is called once per frame
    void Update () {

        if (saveActive == true && saveStatus == false)
        {
            Debug.Log("Starting Saving");
            saveStatus = saveData();
            saveStatus = true;
        }

        bool userGeneratedInput = (Input.GetKey(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) && Time.time >= lastGenerationTime + (1f / depostionRatePerSec);
        bool autoGeneratedInput = automaticDepostion == true & Time.time >= lastGenerationTime + (1f / depostionRatePerSec);
        if (userGeneratedInput == true || autoGeneratedInput == true)
        {
            if (cementationActive == true)
            {
                rock.beds[rock.currentBed].CementAndUncement(true);
                cementationActive = false;
            }

            if (disappearActive == true)
            {
                rock.beds[rock.currentBed].AppearAndDisappear(false);
                disappearActive = false;
            }


            if (exitActive == true && saveStatus == true)
            {
                Application.Quit();
            }


            rock.instantiateGrain();
            lastGenerationTime = Time.time;
  
            // Check if we reached the next bed
            if (rock.beds[rock.currentBed].IsDeposted() == true)
            {
                AfterBedOperations();
            }
        }

        // Input
        if (Input.GetKeyUp(KeyCode.C)) { stabilizeCamera = !stabilizeCamera;}
        if (Input.GetKeyUp(KeyCode.F5) == true)
        {
            updateText("Saving...");
            saveActive = true;
            saveStatus = false;

        }

        if (Input.GetKeyDown(KeyCode.J)) { ShakingFraction += .0001f;}
        if (Input.GetKeyDown(KeyCode.H)) {ShakingFraction -= .0001f; }
        if (Input.GetKeyDown(KeyCode.N)) {ShakingRotationFraction += .0001f;}
        if (Input.GetKeyDown(KeyCode.B)) { ShakingRotationFraction -= .0001f;}
        if (Input.GetKeyUp(KeyCode.K)) { shakeTransversly = !shakeTransversly;}
        if (Input.GetKeyUp(KeyCode.M)) { shakeRotationaly = !shakeRotationaly;}

        if (Input.GetKeyUp(KeyCode.R)) { automaticDepostion = !automaticDepostion;}
        if (Input.GetKeyUp(KeyCode.Escape)) {CloseApplication();}
        if (Input.GetKeyUp(KeyCode.G)) {Physics.gravity = new Vector3(0, -1f * Mathf.Abs(Physics.gravity.y + 9.81f), 0);}
        if (Input.GetKeyUp(KeyCode.X)) { rock.CementAndUnCement(false); }
        if (Input.GetKeyUp(KeyCode.Z)) { rock.CementAndUnCement(true);}
        if (Input.GetKeyUp(KeyCode.P)) {
            if (backgroundColor == Color.black) { backgroundColor = Color.white; }
            else {backgroundColor = Color.black; }
            Camera.main.backgroundColor = backgroundColor; 
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            isContainerVisible = !isContainerVisible;
            Container.GetComponent<ContainerOperations>().SetVisibility(isContainerVisible);
        }
        
        if (Input.GetKeyDown(KeyCode.Equals)) { TimeScale += 0.2f; }
        if (Input.GetKeyDown(KeyCode.Minus)) { TimeScale -= 0.2f; }

        if (Input.GetKeyDown(KeyCode.Y)) { FixedDeltaTime += 0.001f; }
        if (Input.GetKeyDown(KeyCode.T)) { FixedDeltaTime -= 0.001f; }

        if (Input.GetKeyDown(KeyCode.F1)) { Application.Quit(); }

        // Shaker
        shaker.shakeBox(shakeTransversly, shakeRotationaly, ShakingFraction, ShakingRotationFraction);
        OnValidate();

    }
    //===================================================================
    // Editor function, does not affect the EXE
    void OnValidate()
    {
        // Validate
        SolverIterationCount = Mathf.RoundToInt(Mathf.Clamp(SolverIterationCount, 1, float.MaxValue));
        velocitySolverIterationCount = Mathf.RoundToInt(Mathf.Clamp(velocitySolverIterationCount, 1, float.MaxValue));

        TimeScale = Mathf.Clamp(TimeScale, 0, float.MaxValue);
        FixedDeltaTime = Mathf.Clamp(FixedDeltaTime, 0, float.MaxValue);

        SleepThreshold = Mathf.Clamp(SleepThreshold, 0, float.MaxValue);
        BounceThreshold = Mathf.Clamp(BounceThreshold, 0, float.MaxValue);

        ShakingFraction = Mathf.Clamp(ShakingFraction, 0, .1f);
        ShakingRotationFraction = Mathf.Clamp(ShakingRotationFraction, 0, .1f);

        // Apply
        Physics.defaultSolverIterations = SolverIterationCount;
        Physics.defaultSolverVelocityIterations = velocitySolverIterationCount;

        Time.timeScale = TimeScale;
        Time.fixedDeltaTime = FixedDeltaTime;
        Time.maximumDeltaTime = Time.fixedDeltaTime;

        Physics.sleepThreshold = SleepThreshold;
        Physics.bounceThreshold = BounceThreshold;
        Physics.defaultContactOffset = contactOffset;

        Camera.main.GetComponent<CameraController>().isStabilizeCamera = stabilizeCamera;
        Camera.main.backgroundColor = backgroundColor;

    }
    //===================================================================
    private void AfterBedOperations()
    {
        lastGenerationTime = Time.time + rock.beds[rock.currentBed].waitAfterDepostion;
        cementationActive = rock.beds[rock.currentBed].cementAfterDeposition;
        disappearActive = rock.beds[rock.currentBed].disappearAfterDeposition;


        if (rock.currentBed+1 == rock.nBeds)
        {
            AfterRockOperations();
        }
    }
    //===================================================================
    private void AfterRockOperations()
    {
        saveActive = saveRockAutomatically;
        if (saveActive)
        {
            updateText("Saving...");
        }
        exitActive = exitAutomatically;
    }
    //===================================================================
    private bool saveData()
    {
        string saveFolder;
        saveFolder = projectFolderPath + saveFolderName;
        DirectoryInfo directoryInfo  = new DirectoryInfo(saveFolder);

        if (directoryInfo.Exists == false)
        {
            if (createNewFolderAutomatically == true)
            {
                directoryInfo.Create();
                directoryInfo = new DirectoryInfo(saveFolder);
            }
            else {
                resetText();
                return false;
            }
        }

        if (FileOperations.IsDirectoryEmpty(directoryInfo) == false)
        {
            if (overrideOutputFolder == true)
            {
                FileOperations.EmptyDirectory(directoryInfo);
            }
            else {
                resetText();
                return false;
            }
        }

        if(saveDataFile) DataSaver.saveLocationData(rock, saveFolder);
        if (saveGrainsFile) DataSaver.saveSingleGrainsMesh(rock, saveFolder);
        if (saveRockFile) DataSaver.saveMeshData(rock, saveFolder);

        resetText();
        return true;
    }
    //===================================================================
    private void CloseApplication()
    {
        Application.Quit();
    }
    //===================================================================
    private void updateText(string text)
    {
        uiText.GetComponent<UnityEngine.UI.Text>().text = text;        
    }

    //===================================================================
    private void resetText()
    {
        uiText.GetComponent<UnityEngine.UI.Text>().text = "Mustafa Al Ibrahim (Mustafa.Geoscientist@Outlook.com)\nAll Rights Reserved, 2019, Version 1.12";
    }
    //===================================================================
    public void LoadConfigurationFile()
    {

        string folderPath = FileOperations.GetApplicationDirectory();
        string parameterFilePath = folderPath + configFileName;

        //uiText.GetComponent<UnityEngine.UI.Text>().text = parameterFilePath;
        ParameterGroup configFile = new ParameterGroup(parameterFilePath);

        // Project
        projectNotes = configFile.getString("Notes");

        // Container
        containerType = configFile.getEnum<ContainerType>("Container Type");
        ContainerScale = configFile.getVector3("Container Scale");
        containerDynamicFriction = configFile.getFloat("Container Dynamic Friction");
        containerStaticFriction = configFile.getFloat("Container Static Friction");
        containerBounciness = configFile.getFloat("Container Bounciness");
        modifyScaleAutomatically = configFile.getBoolean("Modify Scale Automatically");
        bufferLength = configFile.getFloat("Buffer Length");
        estimatedPorosity = configFile.getFloat("Estimated Porosity");
        backgroundColor = configFile.getColor("Background Color");

        // Simulation
        FixedDeltaTime = configFile.getFloat("Fixed Delta Time");
        SolverIterationCount = configFile.getInteger("Solver Iteration Count");
        velocitySolverIterationCount = configFile.getInteger("Velocity Solver Iteration Count");
        SleepThreshold = configFile.getFloat("Sleep Threshold");
        BounceThreshold = configFile.getFloat("Bounce Threashold");
        TimeScale = configFile.getFloat("Time Scale");
        TargetFrameRate = configFile.getInteger("Target Frame Rate");
        contactOffset = configFile.getFloat("Contact Offset");

        // Shaking
        shakeTransversly = configFile.getBoolean("Shake Transversly");
        shakeRotationaly = configFile.getBoolean("Shake Rotationally");
        stabilizeCamera = configFile.getBoolean("Stabilize Camera");
        ShakingFraction = configFile.getFloat("Shaking Fracton");
        ShakingRotationFraction = configFile.getFloat("Shaking Roation Fraction");

        // Folder
        projectFolderPath = configFile.getString("Project Folder Path");
        pdfFolderName = configFile.getString("PDF Folder Name");
        saveFolderName = configFile.getString("Save Folder Name");
        createNewFolderAutomatically = configFile.getBoolean("Create New Folder Automatically");
        overrideOutputFolder = configFile.getBoolean("Override Output Folder");

        // Particle Groups
        useVolumeProportion = configFile.getBoolean("Use Volume Proportion");
        grainCountGoal = configFile.getInteger("Grain Count Goal");
        int nbeds = configFile.getInteger("Number of Beds");

        beds = new Bed[nbeds];
        for (int bedNumber= 0; bedNumber < nbeds; bedNumber++)
        {
            int bedNumberOne = bedNumber+1;
            beds[bedNumber] = new Bed();
            beds[bedNumber].bedName = configFile.getString("Bed " + bedNumberOne + " Name");
            beds[bedNumber].proportion = configFile.getFloat("Bed " + bedNumberOne + " Proportion");
            beds[bedNumber].waitAfterDepostion = configFile.getInteger("Bed " + bedNumberOne + " Wait After Deposition");
            beds[bedNumber].cementAfterDeposition = configFile.getBoolean("Bed " + bedNumberOne + " Cement After Deposition");
            beds[bedNumber].disappearAfterDeposition = configFile.getBoolean("Bed " + bedNumberOne + " Disappear After Deposition");
            int nGrainGroups = configFile.getInteger("Bed " + bedNumberOne + " Number of Grains");

            if (nGrainGroups > 0)
            {
                Grain[] grains = new Grain[nGrainGroups];
                for (int grainGroupNumber = 0; grainGroupNumber < nGrainGroups; grainGroupNumber++)
                {

                    int grainGroupNumberOne = grainGroupNumber + 1;
                    grains[grainGroupNumber] = new Grain();
                    grains[grainGroupNumber].grainName = configFile.getString("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Name");
                    grains[grainGroupNumber].Type = configFile.getEnum<Grain.GrainType>("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Type");
                    grains[grainGroupNumber].parameters = configFile.getString("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Parameters");
                    grains[grainGroupNumber].PDF = configFile.getString("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " PDF");
                    grains[grainGroupNumber].PDFMultiplier = configFile.getFloat("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " PDF Multiplier");
                    grains[grainGroupNumber].PDFOffset = configFile.getFloat("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " PDF Offset");
                    grains[grainGroupNumber].density = configFile.getFloat("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Density");
                    grains[grainGroupNumber].exactVerticalCreation = configFile.getBoolean("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Disappear At Bottom");
                    grains[grainGroupNumber].disappearAtBottom = configFile.getBoolean("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Exact Vertical Creation");
                    grains[grainGroupNumber].proportion = configFile.getFloat("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Proportion");
                    grains[grainGroupNumber].depostionType = configFile.getEnum<Grain.DepostionType>("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Deposition Type");
                    grains[grainGroupNumber].dynamicFriction = configFile.getFloat("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Dynamic Friction");
                    grains[grainGroupNumber].staticFriction = configFile.getFloat("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Static Friction");
                    grains[grainGroupNumber].bounciness = configFile.getFloat("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Bounciness");
                    grains[grainGroupNumber].frictionCombine = configFile.getEnum<PhysicMaterialCombine>("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Friction Combine");
                    grains[grainGroupNumber].bounceCombine = configFile.getEnum<PhysicMaterialCombine>("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Bounce Combine");
                    grains[grainGroupNumber].colorType = configFile.getEnum<Grain.GrainColorType>("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Color Type");
                    grains[grainGroupNumber].baseColor = configFile.getColor("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Base Color");
                    grains[grainGroupNumber].secondaryColor = configFile.getColor("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Secondary Color");
                    grains[grainGroupNumber].scale = configFile.getVector3("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " Scale");

                }

                beds[bedNumber].grains = grains;
            }
        }

        // Deposition
        automaticDepostion = configFile.getBoolean("Automatic Deposition");
        depostionRatePerSec = configFile.getFloat("Deposition Rate Per Sec");

        // Saving
        saveRockAutomatically = configFile.getBoolean("Save Rock Automatically");
        saveDataFile = configFile.getBoolean("Save Data File");
        saveRockFile = configFile.getBoolean("Save Rock File");
        saveGrainsFile = configFile.getBoolean("Save Grains File");
        exitAutomatically = configFile.getBoolean("Exit Automatically");
    }

    //===================================================================
    public void SaveConfigurationFile()
    {
        string folderPath = FileOperations.GetApplicationDirectory();
        string parameterFilePath = folderPath + configFileName;

        System.Text.StringBuilder rawData = new System.Text.StringBuilder();

        Debug.Log(parameterFilePath);
        // Project
        rawData.AppendLine("Notes" + " = " + projectNotes);
        rawData.AppendLine("-----------------------------------------------");

        // Container
        rawData.AppendLine("- Container Parameters");
        rawData.AppendLine("Container Type" + " = " + containerType.ToString());
        rawData.AppendLine("Container Scale" + " = " + "[" + ContainerScale.x + " " + ContainerScale.y + " " + ContainerScale.z + "]");
        rawData.AppendLine("Container Dynamic Friction" + " = " + containerDynamicFriction);
        rawData.AppendLine("Container Static Friction" + " = " + containerStaticFriction);
        rawData.AppendLine("Container Bounciness" + " = " + containerBounciness);
        rawData.AppendLine("Modify Scale Automatically" + " = " + modifyScaleAutomatically.ToString());
        rawData.AppendLine("Buffer Length" + " = " + bufferLength);
        rawData.AppendLine("Estimated Porosity" + " = " + estimatedPorosity);
        rawData.AppendLine("Background Color" + " = "  + "[" + 255 * backgroundColor.r + " " + 255 * backgroundColor.g + " " + 255 * backgroundColor.b + " " + 255 * backgroundColor.a + "]");
        rawData.AppendLine("-----------------------------------------------");

        // Simulation Parameters
        rawData.AppendLine("- Simulation Parameters");
        rawData.AppendLine("Fixed Delta Time" + " = " + FixedDeltaTime);
        rawData.AppendLine("Solver Iteration Count" + " = " + SolverIterationCount);
        rawData.AppendLine("Velocity Solver Iteration Count" + " = " + velocitySolverIterationCount);
        rawData.AppendLine("Sleep Threshold" + " = " + SleepThreshold);
        rawData.AppendLine("Bounce Threashold" + " = " + BounceThreshold);
        rawData.AppendLine("Time Scale" + " = " + TimeScale);
        rawData.AppendLine("Target Frame Rate" + " = " + TargetFrameRate);
        rawData.AppendLine("Contact Offset" + " = " + contactOffset);
        rawData.AppendLine("-----------------------------------------------");

        // Shaking Parameters
        rawData.AppendLine("- Shaking Parameters");
        rawData.AppendLine("Shake Transversly" + " = " + shakeTransversly.ToString());
        rawData.AppendLine("Shake Rotationally" + " = " + shakeRotationaly.ToString());
        rawData.AppendLine("Stabilize Camera" + " = " + stabilizeCamera.ToString());
        rawData.AppendLine("Shaking Fracton" + " = " + ShakingFraction);
        rawData.AppendLine("Shaking Roation Fraction" + " = " + ShakingRotationFraction);
        rawData.AppendLine("-----------------------------------------------");

        // Folder Parameters
        rawData.AppendLine("- Folders Parameters");
        rawData.AppendLine("Project Folder Path" + " = " + projectFolderPath);
        rawData.AppendLine("PDF Folder Name" + " = " + pdfFolderName);
        rawData.AppendLine("Save Folder Name" + " = " + saveFolderName);
        rawData.AppendLine("Create New Folder Automatically" + " = " + createNewFolderAutomatically.ToString());
        rawData.AppendLine("Override Output Folder" + " = " + overrideOutputFolder.ToString());
        rawData.AppendLine("-----------------------------------------------");

        // Folder Parameters
        rawData.AppendLine("- Particle Groups");
        rawData.AppendLine("Use Volume Proportion" + " = " + useVolumeProportion);
        rawData.AppendLine("Grain Count Goal" + " = " + grainCountGoal);
        rawData.AppendLine("Number of Beds" + " = " + beds.Length);
        rawData.AppendLine("-----------------------------------------------");

        // Beds
        for (int bedNumber = 0; bedNumber < beds.Length; bedNumber++)
        {
            int bedNumberOne = bedNumber + 1;
            rawData.AppendLine("- Bed " + bedNumberOne);
            rawData.AppendLine("Bed " + bedNumberOne + " " + "Name" + " = " + beds[bedNumber].bedName);
            rawData.AppendLine("Bed " + bedNumberOne + " " + "Proportion" + " = " + beds[bedNumber].proportion);
            rawData.AppendLine("Bed " + bedNumberOne + " " + "Wait After Deposition" + " = " + beds[bedNumber].waitAfterDepostion);
            rawData.AppendLine("Bed " + bedNumberOne + " " + "Cement After Deposition" + " = " + beds[bedNumber].cementAfterDeposition);
            rawData.AppendLine("Bed " + bedNumberOne + " " + "Disappear After Deposition" + " = " + beds[bedNumber].disappearAfterDeposition);
            rawData.AppendLine("Bed " + bedNumberOne + " " + "Number of Grains" + " = " + beds[bedNumber].grains.Length);

            for (int grainGroupNumber = 0; grainGroupNumber < beds[bedNumber].grains.Length; grainGroupNumber++)
            {
                int grainGroupNumberOne = grainGroupNumber + 1;
                Grain grain = beds[bedNumber].grains[grainGroupNumber];
                rawData.AppendLine("-- Bed " + bedNumberOne + " Grain " + grainGroupNumberOne);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Name" + " = " + grain.grainName);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Type" + " = " + grain.Type.ToString());
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Parameters" + " = " + grain.parameters);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "PDF" + " = " + grain.PDF);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "PDF Multiplier" + " = " + grain.PDFMultiplier);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "PDF Offset" + " = " + grain.PDFOffset);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Density" + " = " + grain.density);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Exact Vertical Creation" + " = " + grain.exactVerticalCreation);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Disappear At Bottom" + " = " + grain.disappearAtBottom);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Proportion" + " = " + grain.proportion);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Deposition Type" + " = " + grain.depostionType.ToString());
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Dynamic Friction" + " = " + grain.dynamicFriction);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Static Friction" + " = " + grain.staticFriction);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Bounciness" + " = " + grain.bounciness);
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Friction Combine" + " = " + grain.frictionCombine.ToString());
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Bounce Combine" + " = " + grain.bounceCombine.ToString());
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Color Type" + " = " + grain.colorType.ToString());
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Base Color" + " = " + "[" + 255*grain.baseColor.r + " " + 255*grain.baseColor.g + " " + 255*grain.baseColor.b + " " + 255*grain.baseColor.a + "]");
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Secondary Color" + " = " + "[" + 255 * grain.secondaryColor.r + " " + 255 * grain.secondaryColor.g + " " + 255 * grain.secondaryColor.b + " " + 255 * grain.secondaryColor.a + "]");
                rawData.AppendLine("Bed " + bedNumberOne + " Grain " + grainGroupNumberOne + " " + "Scale" + " = " + "[" + grain.scale.x + " " + grain.scale.y + " " + grain.scale.z + "]");

            }

            rawData.AppendLine("-----------------------------------------------");

        }

        rawData.AppendLine("- Deposition Parameters");
        rawData.AppendLine("Automatic Deposition" + " = " + automaticDepostion.ToString());
        rawData.AppendLine("Deposition Rate Per Sec" + " = " + depostionRatePerSec);
        rawData.AppendLine("Save Rock Automatically" + " = " + saveRockAutomatically.ToString());
        rawData.AppendLine("Exit Automatically" + " = " + exitAutomatically.ToString());
        rawData.AppendLine("-----------------------------------------------");

        rawData.AppendLine("- Deposition Parameters");
        rawData.AppendLine("Save Rock Automatically" + " = " + saveRockAutomatically.ToString());
        rawData.AppendLine("Save Data File" + " = " + saveDataFile.ToString());
        rawData.AppendLine("Save Rock File" + " = " + saveRockFile.ToString());
        rawData.AppendLine("Save Grains File" + " = " + saveGrainsFile.ToString());

        rawData.AppendLine("Exit Automatically" + " = " + exitAutomatically.ToString());
        rawData.AppendLine("-----------------------------------------------");

        System.IO.File.WriteAllText(parameterFilePath, rawData.ToString());
        Debug.Log("Saving Parameter File Done");
    }
    //===================================================================






}


