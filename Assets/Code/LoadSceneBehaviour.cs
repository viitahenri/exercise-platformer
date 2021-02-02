using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneBehaviour : MonoBehaviour
{
    public void LoadScene(string name)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
}
