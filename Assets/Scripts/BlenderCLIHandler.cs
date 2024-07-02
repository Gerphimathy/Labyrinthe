using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using Dummiesman;
using Dummiesman;
using Debug = UnityEngine.Debug;

public class BlenderCLIHandler : MonoBehaviour
{
    //Folder Path
    public string BLENDER_EXE = "";
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public string savePath = "/Assets/Generated/";
    public int toGenerate = 1;
    
    public enum BLENDER_VERSION {BLENDER_4_X_X, BLENDER_X_X};
    
    public BLENDER_VERSION blenderVersion = BLENDER_VERSION.BLENDER_4_X_X;
    
    bool canConnect()
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(BLENDER_EXE);
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.CreateNoWindow = true;
            startInfo.Arguments = "--help";
            Process.Start(startInfo);
        }catch(System.Exception e)
        {
            return false;
        }
        return true;
    }
    
    string generateCreatureScriptFile(string Fname)
    {
        string script = "Assets/Scripts/BlenderPy/Creature.py";
        
        // Replace the placeholder values in the script with the actual values
        string scriptText = System.IO.File.ReadAllText(script);
        scriptText = scriptText.Replace("$$WIDTH$$", textureWidth.ToString());
        scriptText = scriptText.Replace("$$HEIGHT$$", textureHeight.ToString());
        scriptText = scriptText.Replace("$$SAVE_LOCATION$$", savePath);
        scriptText = scriptText.Replace("$$FNAME$$", Fname);

        switch (blenderVersion)
        {
            case BLENDER_VERSION.BLENDER_4_X_X:
                scriptText = scriptText.Replace("$$BLENDER_VERSION$$", "4");
                break;
            case BLENDER_VERSION.BLENDER_X_X:
                scriptText = scriptText.Replace("$$BLENDER_VERSION$$", "3");
                break;
        }
        
        //Write the script to a file in savepath
        string scriptPath = savePath + Fname + ".py";
        System.IO.File.WriteAllText(scriptPath, scriptText);
        
        return scriptPath;
    }

    bool generateCreature(string scriptPath)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(BLENDER_EXE); 
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            //startInfo.CreateNoWindow = true;
            startInfo.Arguments =  "--background --python \"" + scriptPath + "\"";

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            Debug.Log("Out : " + output);
            Debug.LogError("Error : " + error);
            //process.WaitForExit();
            
        }catch(System.Exception e) {
            Debug.LogError(e);
            
            return false;
        }
        return true;
    }

    void Start()
    {
        savePath = savePath.Replace(@"\\", @"\").Replace(@"\", @"\\");
        
        if (!canConnect())
        {
            Debug.LogError("Blender not found at path: " + BLENDER_EXE);
        }
        
        if(generateCreature(generateCreatureScriptFile("creature1")))
        {
            string creaturePath = savePath + "creature1.obj";
            //Runtime OBJ importer
            GameObject obj = new OBJLoader().Load(creaturePath).transform.GetChild(0).gameObject;
            
            if(obj != null)
            {
                obj.transform.position = new Vector3(0, 0, 0);
                
                Texture2D texture = new Texture2D(textureWidth, textureHeight);
                texture.LoadImage(File.ReadAllBytes(savePath + "creature1.png"));
                obj.GetComponent<MeshRenderer>().material.mainTexture = texture;
            }
            else
            {
                Debug.LogError("Failed to generate creature 1");
            }
        }
        else
        {
            Debug.LogError("Failed to generate creature 1");
        }

    }

    void Update()
    {
        
    }
}
