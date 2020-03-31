using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommentsLoader : MonoBehaviour { //this does not work as the IG comments endpoint has been retired

    public static string LAT_LONG;
    public Text comments;
    public static string loadedComments;
    JSONObject instagramComments = new JSONObject(loadedComments);

    public void Load()
    {
        WWW request = new WWW(API.ENDPOINT + API.ACCESS_TOKEN);
        StartCoroutine(OnResponse2(request));
    }

    private IEnumerator OnResponse2(WWW req)
    {
        yield return req;
        
        comments.text = req.text;
        loadedComments = comments.text;
        LAT_LONG = (instagramComments["location"]["latitude"].str);
        loadedComments = LAT_LONG;
    }
}
