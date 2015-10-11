using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Xml;
using SimpleFramework;
using UnityEngine;



public class Main : MonoBehaviour {

    private int scaleWidth = 0;
    private int scaleHeight = 0;

	// Use this for initialization
	void Start () 
    {
        string strXml = Util.DataPath + "Assets.SceneGame.TitleScene.xml";

        if (!File.Exists(strXml))
        {
            return;
        }

        StartCoroutine(AnalyzXml(strXml));
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    IEnumerator AnalyzXml(string strXml)
    {
        //存在
        XmlDocument doc = new XmlDocument();
        doc.Load(strXml);

        XmlElement root = doc.DocumentElement;//doc.SelectSingleNode("root");

        if (root != null)
        {
            //nodelist
            XmlNodeList scenelist = root.SelectNodes("scene");
            foreach (XmlNode scene in scenelist)
            {
                foreach (XmlNode gameobj in scene.ChildNodes)
                {
                    XmlElement obj = (XmlElement)gameobj;
                    if (obj != null)
                    {
                        string objname = obj.GetAttribute("objectName");
                        string perfab = obj.GetAttribute("objectAsset");

                        Vector3 pos = Vector3.zero;
                        Vector3 scale = Vector3.zero;
                        Vector3 rotate = Vector3.zero;

                        //trans
                        XmlNodeList trans = obj.SelectSingleNode("transform").ChildNodes;

                        foreach (XmlElement prs in trans)
                        {
                            if (prs.Name == "position")
                            {
                               // string x = prs["position"]["x"].InnerText;
                                foreach (XmlElement position in prs.ChildNodes)
                                {
                                    switch(position.Name)
                                    {
                                        case "x":
                                            pos.x = float.Parse(position.InnerText);
                                            break;
                                        case "y":
                                            pos.y = float.Parse(position.InnerText);
                                            break;
                                        case "z":
                                            pos.z = float.Parse(position.InnerText);
                                            break;
                                    }
                                }
                            }
                            else if (prs.Name == "rotation")
                            {
                                foreach (XmlElement rotation in prs.ChildNodes)
                                {
                                    switch (rotation.Name)
                                    {
                                        case "x":
                                            rotate.x = float.Parse(rotation.InnerText);
                                            break;
                                        case "y":
                                            rotate.y = float.Parse(rotation.InnerText);
                                            break;
                                        case "z":
                                            rotate.z = float.Parse(rotation.InnerText);
                                            break;
                                    }
                                }
                            }
                            else if (prs.Name == "scale")
                            {
                                foreach (XmlElement sca in prs.ChildNodes)
                                {
                                    switch (sca.Name)
                                    {
                                        case "x":
                                            scale.x = float.Parse(sca.InnerText);
                                            break;
                                        case "y":
                                            scale.y = float.Parse(sca.InnerText);
                                            break;
                                        case "z":
                                            scale.z = float.Parse(sca.InnerText);
                                            break;
                                    }
                                }
                            }
                            else
                                continue;
                        }

                        
                        perfab = perfab.Replace(".prefab",".assetbundle");
                        Object perfabobj = null;// Resources.LoadAssetAtPath("Assets/StreamingAssets/Perfab/" + perfab, typeof(GameObject));

                        //加载prefabs,CreateFromFile只适合在pc端使用
                        //AssetBundle bundle = AssetBundle.CreateFromFile(Application.streamingAssetsPath + @"/AssetBundle/" + perfab);
                        //本地路径
                        //string url = Util.LocalUrl + @"/AssetBundle/" + perfab;
                        //WWW www = new WWW(url);

                        //yield return www;

                        //AssetBundleRequest req = www.assetBundle.LoadAsync(perfab.Replace(".assetbundle",""), typeof(Object));
                        //while (req.isDone == false)
                        //   yield return null;

                        //perfabobj = req.asset;

                        string uri = Util.DataPath + @"/AssetBundle/" + perfab;
                        UnityEngine.Debug.LogWarning("LoadFile::>> " + uri);

                        byte[]stream = File.ReadAllBytes(uri);
                        AssetBundle bundle = AssetBundle.CreateFromMemoryImmediate(stream);

                        AssetBundleRequest req = bundle.LoadAsync(perfab.Replace(".assetbundle",""), typeof(Object));
                        while (req.isDone == false)
                           yield return null;

                        perfabobj = req.asset;

                        //创建一个物体
                        GameObject ob = (GameObject)Instantiate(perfabobj, pos, Quaternion.Euler(rotate));
                        ob.transform.localScale = scale;

                        Animation an = ob.GetComponent<Animation>();
                        if (an != null)
                        {
                            TitleCharacter.players.Add(ob);
                        }

                        //www.assetBundle.Unload(false);

                        //www.Dispose();
                    } 
                }
                
            }
        }
    }



	public void setDesignContentScale()
	{
#if UNITY_ANDROID
		if(scaleWidth ==0 && scaleHeight ==0)
		{
			int width = Screen.currentResolution.width;
			int height = Screen.currentResolution.height;
			int designWidth = 640;
			int designHeight = 940;
			float s1 = (float)designWidth / (float)designHeight;
			float s2 = (float)width / (float)height;
			if(s1 < s2) {
				designWidth = (int)Mathf.FloorToInt(designHeight * s2);
			} else if(s1 > s2) {
				designHeight = (int)Mathf.FloorToInt(designWidth / s2);
			}
			float contentScale = (float)designWidth/(float)width;
			if(contentScale < 1.0f) { 
				scaleWidth = designWidth;
				scaleHeight = designHeight;
			}
		}
		if(scaleWidth >0 && scaleHeight >0)
		{
			if(scaleWidth % 2 == 0) {
				scaleWidth += 1;
			} else {
				scaleWidth -= 1;					
			}
			Screen.SetResolution(scaleWidth,scaleHeight,true);
		}
#elif UNITY_STANDALONE_WIN
       // Screen.SetResolution(640, 940, false);
        if (scaleWidth == 0 && scaleHeight == 0)
        {
            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;
            int designWidth = 320;
            int designHeight = 480;
            float s1 = (float)designWidth / (float)designHeight;
            float s2 = (float)designHeight / (float)designWidth;

            //fixedheight
            if (designHeight >= height)
            {
                //按比例缩小
                designHeight = height;
                designWidth = (int)Mathf.FloorToInt(designHeight * s1);
            }
            else
            {
                if (designWidth >= width)
                {
                    designWidth = width;
                    designHeight = (int)Mathf.FloorToInt(designHeight * s2);
                }
            }

            scaleWidth = designWidth;
            scaleHeight = designHeight;
        }


        if (scaleWidth > 0 && scaleHeight > 0)
        {
            if (scaleWidth % 2 == 0)
            {
                scaleWidth += 1;
            }
            else
            {
                scaleWidth -= 1;
            }
            Screen.SetResolution(scaleWidth, scaleHeight, false);
        }
#else
#endif
	}


    //当检测到暂停
    void OnApplicationPause(bool isPause)
    {
        //
        if (isPause)
        {
            //暂定
        }
        else
        {
            //游戏开始
            setDesignContentScale();
        }
    }

    //焦点
    void OnApplicationFocus(bool isFocus)
    {
        //焦点
        if (isFocus)
        {
            //设置分辨率
            setDesignContentScale();
        }
        else
        {
            //失去焦点
        }
    }
}
