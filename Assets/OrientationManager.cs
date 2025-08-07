using UnityEngine;
using System.Collections;

public class OrientationManager : MonoBehaviour
{
    // [System.Obsolete]
    // void Start()
    // {
    //     StartCoroutine(SetLandscapeAfterSplash());
    // }

    // [System.Obsolete]
    // IEnumerator SetLandscapeAfterSplash()
    // {
    //     while (Application.isShowingSplashScreen)
    //     {
    //         yield return null;
    //     }
    //     Screen.orientation = ScreenOrientation.LandscapeLeft;
    // }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void SetOrientation()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
    
    public void ReturnToGame()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}
