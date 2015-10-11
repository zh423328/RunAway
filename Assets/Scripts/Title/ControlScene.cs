/// <summary>
/// This script use to control scene
/// </summary>

using UnityEngine;
using System.Collections;
using SimpleFramework;

public class ControlScene : MonoBehaviour {

	public AudioClip sfxButton;
	
	private bool oneshotSfx;
	
	// Update is called once per frame
	void Update () {
		
		//if press any key jump to gameplay scene
		if(Input.anyKeyDown)
		{
			if(!oneshotSfx)
			{
				AudioSource.PlayClipAtPoint(sfxButton,Vector3.zero);
				Invoke("LoadScene",0.5f);
				oneshotSfx = true;
			}
			
			
		}
	
	}
	
	void LoadScene()
	{
		//load gameplay scene
		//Application.LoadLevel("Shop");
        StartCoroutine(LoadShopScene());
	}

    IEnumerator LoadShopScene()
    {
        //º”‘ÿ≥°æ∞
        string url = Util.LocalUrl + "Scene/Shop.unity3d";

        WWW www = new WWW(url);

        yield return www;

        //yield return Application.LoadLevelAsync("Shop");
        Instantiate(www.assetBundle.mainAsset);
        www.assetBundle.Unload(false);
    }
	
}
