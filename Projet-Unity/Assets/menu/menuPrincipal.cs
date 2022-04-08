using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuPrincipal : MonoBehaviour
{

    // la fonction peut s'appeler autrement
    public void PlayGame () 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // la fonction peut s'appeler autrement
    public void QuitGame ()
    {
        Debug.Log("L'application a quitté !");
        Application.Quit();
    }

}
