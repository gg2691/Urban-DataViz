using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


public class API : MonoBehaviour
{

    public const string ENDPOINT = "https://api.instagram.com/v1/users/self/media/recent/?access_token="; //endpoint to access own picture
    //public static string ENDPOINT_2 = "https://api.instagram.com/v1/media/"+ CommentsLoader.MEDIA_ID + "/comments?access_token="; //endpoint to access comments of selected picture
    public const string ACCESS_TOKEN = "233067825.ff6ca63.5a7624d8b21f40d991ae54aa214d67dd";
    //public const string ACCESS_TOKEN = "20084197814.c9ed558.aa0d3def37864f5ebc473be1c5f19c94";
    private const string COUNT = "&count=2"; //number of photos you want to query
    public Text responseText; //text variable to store JSON data
    public static string returnedIGText; //string to store JSON text data
    public static string URL; //the .jpg url of the picture queried from the JSON data
    public static JSONObject instagramPic; //= new JSONObject(returnedIGText); //JSON object to hold JSON data of picture

    // TO DO 
    // Save string into Gameobject in order to call from JPGTOTexture 

    public void Request()
    {
        WWW request = new WWW(ENDPOINT + ACCESS_TOKEN + COUNT); //requests JSON data of desired picture
        StartCoroutine(OnResponse(request));
    }

    private IEnumerator OnResponse(WWW req)
    {
        yield return req;

        responseText.text = req.text;
        returnedIGText = responseText.text;
        returnedIGText = returnedIGText.Replace("\"", "").Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "").Replace(",","");

        string[] filteredIGText = returnedIGText.Split(' ');

        int pos = System.Array.IndexOf(filteredIGText, "standard_resolution:");

        if(filteredIGText[pos+5] == "url:")
        {
            URL = filteredIGText[pos + 6];
        }
        else
        {
            URL = null;
        }

        Debug.Log("Got URL FROM API");
        Debug.Log(URL);

        //urlResult.tag = URL;
         
        //instagramPic["standard_resolution"]["url"].ToString(); //format to parse only the .jpg url of a standard resolution picutre. this URL will then be used in the JPGToTexture class    

    }

    public static string sendURL()
    {
        Debug.Log("sendURL: " + URL);
        return URL;
    }
}

