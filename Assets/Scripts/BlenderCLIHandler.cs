using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using Dummiesman;
using Debug = UnityEngine.Debug;

public class BlenderCLIHandler : MonoBehaviour
{
    //Folder Path
    public string BLENDER_EXE = "";
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public string savePath = "/Assets/Generated/";
    
    public enum BLENDER_VERSION {BLENDER_4_X_X, BLENDER_X_X};
    
    public BLENDER_VERSION blenderVersion = BLENDER_VERSION.BLENDER_4_X_X;
    
    private List<Texture2D> _roomTextures = new List<Texture2D>();
    
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
            Debug.LogError(e.Message);
            return false;
        }
        return true;
    }
    
    string generateCreatureScriptFile(string Fname, int seed, int toGenerate)
    {
        string script = "Assets/Scripts/BlenderPy/Creature.py";
        
        // Replace the placeholder values in the script with the actual values
        string scriptText = System.IO.File.ReadAllText(script);
        scriptText = scriptText.Replace("$$WIDTH$$", textureWidth.ToString());
        scriptText = scriptText.Replace("$$HEIGHT$$", textureHeight.ToString());
        scriptText = scriptText.Replace("$$SAVE_LOCATION$$", savePath);
        scriptText = scriptText.Replace("$$FNAME$$", Fname);
        scriptText = scriptText.Replace("$$POP$$", toGenerate.ToString());
        scriptText = scriptText.Replace("$$SEED$$", seed.ToString());

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

    public GameObject generate(int seed, int toGenerate)
    {
        savePath = savePath.Replace(@"\\", @"\").Replace(@"\", @"\\");
        
        if (!canConnect())
        {
            Debug.LogError("Blender not found at path: " + BLENDER_EXE);
        }

        string script = generateCreatureScriptFile("creatures", seed, toGenerate);
        if(generateCreature(script))
        {
            string creaturePath = savePath + "creatures.obj";
            //Runtime OBJ importer
            GameObject creatures = new OBJLoader().Load(creaturePath);
            for (int i = 0; i < toGenerate; i++)
            {
                try
                {
                    // Get Child
                    GameObject obj = creatures.transform.GetChild(i).gameObject;
                    Texture2D texture = new Texture2D(textureWidth, textureHeight);
                    texture.LoadImage(File.ReadAllBytes(savePath + "creatures"+i+".png"));
                    var currMat = obj.GetComponent<MeshRenderer>().material;
                    currMat.mainTexture = texture;
                    currMat.SetColor("_Color", Color.white);
                    currMat.SetColor("_SpecColor", Color.black);
                    
                    Texture2D roomTexture = new Texture2D(textureWidth, textureHeight);
                    roomTexture.LoadImage(File.ReadAllBytes(savePath + "room"+i+".png"));
                    _roomTextures.Add(roomTexture);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to generate creature "+i);
                    Debug.LogException(ex, this);
                }
            }
            return creatures;
        }
        else
        {
            Debug.LogError("Failed to generate creatures");
        }

        return null;
    }
    
    public Texture2D GetRandomRoomTexture()
    {
        return _roomTextures[UnityEngine.Random.Range(0, _roomTextures.Count)];
    }
}
