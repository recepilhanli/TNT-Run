using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject Menu;
    public KeyCode pauseKey = KeyCode.Escape;
    public static bool paused = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(pauseKey))
        {
            if(Menu.activeInHierarchy == false)
            {
                Menu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                paused = true;
            }
            else
            {
                Resume();
            }
        }

    }


    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(ConnectionHandler.netManager.gameObject);
        SceneManager.LoadScene("Menu");
        paused = false;
    }

    public void Resume()
    {
        Menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        paused = false;
    }


}
