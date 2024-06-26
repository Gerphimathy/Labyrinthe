using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class BlenderCLIHandler : MonoBehaviour
{
    //Folder Path
    public string BLENDER_EXE = "";
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public string savePath = "/Assets/Generated/";
    public int toGenerate = 1;
    
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
        
        return scriptText;
    }

    bool generateCreature(string scriptText)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(BLENDER_EXE);
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            //startInfo.CreateNoWindow = true;
            startInfo.Arguments = "--help --background --python-expr \"" + scriptText + "\"";
            Process.Start(startInfo);
            
            //Pause
            System.Threading.Thread.Sleep(5000);
            
        }catch(System.Exception e)
        {
            return false;
        }
        return true;
    }
    
    void Start()
    {
        if (!canConnect())
        {
            Debug.LogError("Blender not found at path: " + BLENDER_EXE);
        }
        
        string creature1Content = generateCreatureScriptFile("creature1");
        generateCreature(creature1Content);

    }

    void Update()
    {
        
    }
}
