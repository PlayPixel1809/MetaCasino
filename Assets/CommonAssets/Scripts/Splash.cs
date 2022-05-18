using System.Collections;
using UnityEngine;

public class Splash : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(3);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    
}
