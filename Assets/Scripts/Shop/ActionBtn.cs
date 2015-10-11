using UnityEngine;
using System.Collections;
using SimpleFramework;

public class ActionBtn : MonoBehaviour 
{

	public Texture2D normal;
	public Texture2D actived;
	public string levelName;
	
	public AudioClip sfxButton;

	private bool ready;

	void OnMouseDown()
    {
		this.guiTexture.texture = actived;
		
		if(sfxButton != null)
			AudioSource.PlayClipAtPoint(sfxButton,transform.position);
	}


	void OnMouseUpAsButton()
    {
		this.guiTexture.texture = normal;
		//Application.LoadLevel (levelName);
        StartCoroutine(LoadLevelName(levelName));
	}

    IEnumerator LoadLevelName(string level)
    {
        //加载场景
        string url = Util.DataPath + "Scene/" + level + ".unity3d";

        AssetBundle bundle = SimpleFramework.Util.LoadSceneAssetBundle(url);

        if (bundle == null)
            yield break;

        //loadAsync
        AssetBundleRequest req = bundle.LoadAsync(level, typeof(Object));
        while (req.isDone == false)
            yield return null;

        Object asset = req.asset;
        yield return Application.LoadLevelAsync(level);

        Debug.Log(level);
    }

	void OnMouseExit()
    {
		this.guiTexture.texture = normal;
	}
}
