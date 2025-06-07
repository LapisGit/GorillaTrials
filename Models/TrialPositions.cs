using System.Collections.Generic;
using UnityEngine;

namespace GorillaTrials.Models
{
    public class TrialPositions
    {
        public static List<Vector3> stumpClimbBoxes;
        public static List<Vector3> shoppingSpreeBasicsBoxes;
        public static List<Vector3> wraparoundBoxes;
        public static List<Vector3> ctfBoxes;
        public static List<Vector3> canyonRunBoxes;
        
        public static void Initialize()
        {
            stumpClimbBoxes = new List<Vector3>();
            stumpClimbBoxes.Add(new Vector3(-65.5062f,2.556363f,-72.94588f));
            stumpClimbBoxes.Add(new Vector3(-69.15505f,4.061646f,-75.5445f));
            stumpClimbBoxes.Add(new Vector3(-68.22018f,5.746896f,-77.90948f));
            stumpClimbBoxes.Add(new Vector3(-67.33721f,8.802135f,-78.8488f));
            stumpClimbBoxes.Add(new Vector3(-66.6948f,12.32989f,-78.68581f));
            stumpClimbBoxes.Add(new Vector3(-66.51869f,16.78736f,-79.79868f));
            stumpClimbBoxes.Add(new Vector3(-66.83424f,21.94868f,-79.76862f));
            stumpClimbBoxes.Add(new Vector3(-63.53926f,21.88185f,-82.22379f));
            stumpClimbBoxes.Add(new Vector3(-50.99947f,20.56706f,-77.91679f));
            stumpClimbBoxes.Add(new Vector3(-43.39106f,21.32471f,-75.30914f));
            stumpClimbBoxes.Add(new Vector3(-40.28435f,21.79396f,-74.52564f));
            stumpClimbBoxes.Add(new Vector3(-36.86979f,18.28717f,-74.44807f));

            shoppingSpreeBasicsBoxes = new List<Vector3>();
            shoppingSpreeBasicsBoxes.Add(new Vector3(-66.29054f, 17.5087f, -127.8544f));
            shoppingSpreeBasicsBoxes.Add(new Vector3(-64.52206f,16.41127f,-132.9847f));
            shoppingSpreeBasicsBoxes.Add(new Vector3(-63.63432f,19.31639f,-140.1776f));
            shoppingSpreeBasicsBoxes.Add(new Vector3(-67.90149f,17.13175f,-148.1319f));
            shoppingSpreeBasicsBoxes.Add(new Vector3(-73.44447f,17.12069f,-145.6527f));
            shoppingSpreeBasicsBoxes.Add(new Vector3(-77.84386f,16.42966f,-139.1096f));
            
            wraparoundBoxes = new List<Vector3>();
            wraparoundBoxes.Add(new Vector3(-33.00232f,16.44123f,-106.7676f));
            wraparoundBoxes.Add(new Vector3(-37.39301f,17.59162f,-104.4286f));
            wraparoundBoxes.Add(new Vector3(-33.18779f,19.42347f,-105.0206f));
            wraparoundBoxes.Add(new Vector3(-28.63275f,19.38772f,-111.9691f));
            wraparoundBoxes.Add(new Vector3(-35.1727f,15.51514f,-113.6914f));

            ctfBoxes = new List<Vector3>();
            ctfBoxes.Add(new Vector3(-46.99637f,4.963249f,-29.63624f));
            ctfBoxes.Add(new Vector3(-47.38862f,3.277501f,-33.39424f));
            ctfBoxes.Add(new Vector3(-48.2356f,5.87801f,-42.433f));
            ctfBoxes.Add(new Vector3(-48.85529f,7.890433f,-49.30875f));
            ctfBoxes.Add(new Vector3(-47.95044f,10.19269f,-53.72191f));
            ctfBoxes.Add(new Vector3(-49.01791f,13.00496f,-59.56395f));
            ctfBoxes.Add(new Vector3(-47.13469f,17.63213f,-64.51656f));
            ctfBoxes.Add(new Vector3(-43.82056f,18.08991f,-68.964f));
            ctfBoxes.Add(new Vector3(-39.98576f,17.43815f,-73.62633f));
            ctfBoxes.Add(new Vector3(-40.3922f,17.79181f,-78.49303f));
            ctfBoxes.Add(new Vector3(-45.61953f,18.1948f,-79.74636f));
            ctfBoxes.Add(new Vector3(-49.99929f,20.37433f,-77.93771f));
            ctfBoxes.Add(new Vector3(-56.89211f,20.87017f,-79.26311f));
            ctfBoxes.Add(new Vector3(-65.93003f,21.95213f,-83.13269f));
            
            canyonRunBoxes = new List<Vector3>();
            
        }
    }
}