using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update

    public static string ConnectedIP;
    public static string ConnectedName;
    public static string ConnectedColor = "add8e6ff";

    //0 -> Client, 1-> Host, 2->Server
    public static int netstate;
    

    public TMP_InputField NickInput;
    public TMP_InputField AddressInput;

    public TextMeshProUGUI Title;
    public RawImage Backgronud;
    public TextMeshProUGUI ErrorText;

    private bool titleAnim;
  


    void Start()
    {
        if(Application.platform == RuntimePlatform.WindowsServer)
        {
            netstate = 2;
            Loadlevel();


        }
    }

    // Update is called once per frame
    void Update()
    {


        titleAnimation();
        BackgroundAnimation();
    }



    void BackgroundAnimation()
    {
        Backgronud.uvRect = new Rect(Backgronud.uvRect.position + new Vector2(0,0.1F)*Time.deltaTime,Backgronud.uvRect.size);
    }

    void titleAnimation()
    {

        if(titleAnim == false)
        {
            Title.rectTransform.localScale = Title.rectTransform.localScale + Title.rectTransform.localScale * Time.deltaTime * 1.25f;
            if(Title.rectTransform.localScale.x >= 2.0f) titleAnim = true;

            Color tc = Title.color;

            Title.color = new Color(tc.r, tc.g - Time.deltaTime * 2,  tc.g - Time.deltaTime * 2);

        }
        else
        {
            Title.rectTransform.localScale = Title.rectTransform.localScale - Title.rectTransform.localScale * Time.deltaTime * 1.25f;
            if (Title.rectTransform.localScale.x <= 1.0f) titleAnim = false;

            Color tc = Title.color;

            Title.color = new Color(tc.r, tc.g + Time.deltaTime * 2, tc.g + Time.deltaTime * 2);
        }

    }


    void ErrorMessage(string message)
    {
        if (message == null) return;

        ErrorText.text = message;
        ErrorText.gameObject.SetActive(true);

        StopCoroutine(HideErrorText());
        StartCoroutine(HideErrorText());    

    }

    IEnumerator HideErrorText()
    {
        yield return new WaitForSeconds(3);

        ErrorText.gameObject.SetActive(false);
    }

    //Network

    void Loadlevel()
    {
        if (NickInput.text.Length < 2 && netstate != 2)
        {
            ErrorMessage("Enter a valid nickname!");
            return;
        }
        ConnectedName = NickInput.text;
        SceneManager.LoadScene("Game");
    }


    public void Conneect()
    {
        if(AddressInput.text.Length < 3 && AddressInput.text.Length != 0)
        {
            ErrorMessage("Enter a valid IP adress!");
            return;
        }
        if (AddressInput.text.Length == 0) AddressInput.text = "127.0.0.1";
        ConnectedIP = AddressInput.text;
     //   ConnectedIP = ConnectedIP.Remove(ConnectedIP.Length - 1); //textmeshpro bug
        netstate = 0;
        Loadlevel();
    }

    public void Host()
    {
        netstate = 1;
        Loadlevel();
    }


    

}

 