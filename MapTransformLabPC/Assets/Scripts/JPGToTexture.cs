using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JPGToTexture : MonoBehaviour {


    public GameObject frame; 
    private bool gotURL = false;
    public string url;

    //API.URL; //manual paste"https://scontent.cdninstagram.com/vp/ff625bd9b0b4d4479b241fa0102ea7a8/5E0765BB/t51.2885-15/sh0.08/e35/s640x640/60390897_876460372688332_2725837853709323762_n.jpg?_nc_ht=scontent.cdninstagram.com";

    private void Start()
    {
        frame = GameObject.Find("PictureFrame");
    }

    public void Get() {
        StartCoroutine(GetPic());
    }

    private IEnumerator GetPic() //method using texture2d to turn .jpg into a texture to be pasted onto quad
    {
        yield return new WaitForSeconds(1);
        url = API.URL;

        Texture2D tex;
        tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        using (WWW www = new WWW(url)) //using the .jpg url obtained from JSON 
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            frame.GetComponent<MeshRenderer>().material.mainTexture = tex;
            
        }
    }
}
