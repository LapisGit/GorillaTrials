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
        public static List<Vector3> swingBoxes;
        public static List<Vector3> caveRunBoxes;
        
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
            canyonRunBoxes.Add(new Vector3(-82.14141f,6.796176f,-109.0485f));
            canyonRunBoxes.Add(new Vector3(-85.63686f,2.948982f,-118.4253f));
            canyonRunBoxes.Add(new Vector3(-86.141f,0.6218138f,-126.5483f));
            canyonRunBoxes.Add(new Vector3(-85.08427f,-1.954865f,-131.3694f));
            canyonRunBoxes.Add(new Vector3(-86.26131f,-4.313036f,-135.8721f));
            canyonRunBoxes.Add(new Vector3(-93.73781f,-4.547762f,-137.9174f));
            canyonRunBoxes.Add(new Vector3(-98.2252f,-4.504431f,-141.5589f));
            canyonRunBoxes.Add(new Vector3(-96.1962f,-4.504438f,-145.3024f));
            canyonRunBoxes.Add(new Vector3(-98.49719f,-4.504366f,-144.5255f));
            canyonRunBoxes.Add(new Vector3(-94.2793f,-4.504433f,-144.7257f));
            canyonRunBoxes.Add(new Vector3(-89.60837f,-4.576727f,-142.816f));
            canyonRunBoxes.Add(new Vector3(-86.1476f,-4.504436f,-142.2942f));
            canyonRunBoxes.Add(new Vector3(-82.35934f,-4.519087f,-145.8084f));
            canyonRunBoxes.Add(new Vector3(-80.47931f,-4.504488f,-149.3648f));
            canyonRunBoxes.Add(new Vector3(-82.05243f,-4.504526f,-153.2783f));
            canyonRunBoxes.Add(new Vector3(-85.27756f,-4.504524f,-156.3945f));
            canyonRunBoxes.Add(new Vector3(-88.57807f,-4.532732f,-159.3718f));
            canyonRunBoxes.Add(new Vector3(-92.55173f,-4.617002f,-161.6689f));
            canyonRunBoxes.Add(new Vector3(-96.24032f,-4.547529f,-163.7879f));
            canyonRunBoxes.Add(new Vector3(-99.86703f,-4.531107f,-165.8843f));
            canyonRunBoxes.Add(new Vector3(-101.9928f,-4.519701f,-162.8931f));
            canyonRunBoxes.Add(new Vector3(-101.9171f,-4.523211f,-158.2975f));
            canyonRunBoxes.Add(new Vector3(-104.4412f,-4.519056f,-155.527f));
            canyonRunBoxes.Add(new Vector3(-107.7236f,-4.622293f,-153.4762f));
            canyonRunBoxes.Add(new Vector3(-109.2883f,-4.529402f,-150.711f));
            canyonRunBoxes.Add(new Vector3(-111.3677f,-4.527182f,-147.2457f));
            canyonRunBoxes.Add(new Vector3(-111.0547f,-4.50444f,-142.76f));
            canyonRunBoxes.Add(new Vector3(-110.4223f,-4.504332f,-140.2714f));
            canyonRunBoxes.Add(new Vector3(-109.3129f,-4.504438f,-137.2581f));
            canyonRunBoxes.Add(new Vector3(-107.128f,-4.477491f,-134.72f));
            canyonRunBoxes.Add(new Vector3(-104.2506f,-4.504438f,-133.5794f));
            canyonRunBoxes.Add(new Vector3(-100.2772f,-4.58919f,-132.2174f));
            canyonRunBoxes.Add(new Vector3(-95.60734f,-4.504257f,-131.9234f));
            canyonRunBoxes.Add(new Vector3(-93.88969f,-2.305239f,-133.5171f));
            canyonRunBoxes.Add(new Vector3(-93.55498f,0.6577187f,-133.5697f));
            canyonRunBoxes.Add(new Vector3(-93.48483f,3.994421f,-133.5808f));
            canyonRunBoxes.Add(new Vector3(-92.50083f,6.765745f,-133.9864f));
            canyonRunBoxes.Add(new Vector3(-92.36173f,8.88239f,-133.0593f));
            canyonRunBoxes.Add(new Vector3(-89.62206f,9.734635f,-132.942f));
            
            swingBoxes = new List<Vector3>();
            swingBoxes.Add(new Vector3(-112.4501f,12.04811f,-122.3286f));
            swingBoxes.Add(new Vector3(-110.983f,12.30052f,-128.8504f));
            swingBoxes.Add(new Vector3(-109.4048f,11.49467f,-133.8225f));
            swingBoxes.Add(new Vector3(-105.5165f,10.14793f,-138.5076f));
            swingBoxes.Add(new Vector3(-107.4023f,10.02634f,-142.6389f));
            swingBoxes.Add(new Vector3(-120.9869f,17.83341f,-142.1236f));
            
            caveRunBoxes = new List<Vector3>();
            caveRunBoxes.Add(new Vector3(-65.97836f,-13.97495f,-44.06382f));
            caveRunBoxes.Add(new Vector3(-72.69279f,-15.36273f,-39.33559f));
            caveRunBoxes.Add(new Vector3(-74.78378f,-16.61712f,-34.55975f));
            caveRunBoxes.Add(new Vector3(-74.73085f,-19.96908f,-27.97591f));
            caveRunBoxes.Add(new Vector3(-69.96481f,-19.96586f,-24.98577f));
            caveRunBoxes.Add(new Vector3(-66.31364f,-20.96729f,-25.85603f));
            caveRunBoxes.Add(new Vector3(-64.46914f,-24.21059f,-31.92958f));
            caveRunBoxes.Add(new Vector3(-67.56017f,-27.19415f,-32.0918f));
            caveRunBoxes.Add(new Vector3(-73.40561f,-27.32014f,-34.65099f));
            caveRunBoxes.Add(new Vector3(-78.20628f,-27.13052f,-38.51123f));
            caveRunBoxes.Add(new Vector3(-80.57037f,-26.08961f,-40.24037f));
            caveRunBoxes.Add(new Vector3(-75.43208f,-24.91459f,-41.77291f));
            caveRunBoxes.Add(new Vector3(-75.32088f,-23.15848f,-42.73833f));
            caveRunBoxes.Add(new Vector3(-82.74161f,-23.10038f,-40.99244f));
            caveRunBoxes.Add(new Vector3(-86.9641f,-23.30623f,-38.27695f));
            caveRunBoxes.Add(new Vector3(-88.39337f,-23.14931f,-34.9588f));
            caveRunBoxes.Add(new Vector3(-88.33712f,-23.99468f,-25.89679f));
            caveRunBoxes.Add(new Vector3(-87.67078f,-24.89686f,-32.98079f));
            caveRunBoxes.Add(new Vector3(-89.60739f,-24.66315f,-30.12817f));
            caveRunBoxes.Add(new Vector3(-86.21545f,-24.08111f,-24.87182f));
            caveRunBoxes.Add(new Vector3(-83.4324f,-24.07364f,-21.26532f));
            caveRunBoxes.Add(new Vector3(-79.95529f,-23.96792f,-17.55959f));
            caveRunBoxes.Add(new Vector3(-72.61205f,-24.29199f,-17.19219f));
            caveRunBoxes.Add(new Vector3(-68.30682f,-24.11769f,-16.06422f));
            caveRunBoxes.Add(new Vector3(-63.18027f,-24.1005f,-16.39038f));
            caveRunBoxes.Add(new Vector3(-59.66733f,-24.29091f,-24.36161f));
            caveRunBoxes.Add(new Vector3(-60.37058f,-27.18257f,-29.99792f));
            caveRunBoxes.Add(new Vector3(-59.53126f,-26.86858f,-38.34457f));
            caveRunBoxes.Add(new Vector3(-65.24681f,-27.196f,-40.25776f));
            caveRunBoxes.Add(new Vector3(-70.74526f,-27.20573f,-40.85057f));
        }
    }
}