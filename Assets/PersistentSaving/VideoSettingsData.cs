using UnityEngine;

[System.Serializable]
public class VideoSettingsData
{
    public string displayMode;
    public string antiAliasingMode;
    public int framerate;
    public int[] resolutionMode;
    
    public VideoSettingsData(string display, string antiAliasing, int fps, int[] resolution)
    {
        displayMode = display;
        antiAliasingMode = antiAliasing;
        framerate = fps;
        resolutionMode = resolution;
    }
}