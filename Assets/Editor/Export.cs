using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using SimpleFramework;

public class Export:Editor
{
    [MenuItem("Export/ExportScene")]
    static void ExportSceneToXml()
    {
        //把场景动态导出
        string strFileName = EditorApplication.currentScene;

        strFileName = strFileName.Replace("unity", "xml");
        strFileName = strFileName.Replace("/", ".");

        //保存到Asset
        string strFilePath = Application.dataPath + @"/StreamingAssets/" + strFileName;

        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        string strPerfabFloder = Application.streamingAssetsPath + @"/Perfab/";
        if (!Directory.Exists(strPerfabFloder))
        {
            Directory.CreateDirectory(strPerfabFloder);
        }
 
        //删除老文件
        if (File.Exists(strFilePath))
        {
            File.Delete(strFilePath);
        }


        XmlDocument xmlDoc = new XmlDocument();
        // 创建XML属性
        XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8",null);
        xmlDoc.AppendChild(xmlDeclaration);

        //创建root属性
        XmlElement root = xmlDoc.CreateElement("root");

        //创建scene属性
        XmlElement sceneXmlElement = xmlDoc.CreateElement("scene");
        sceneXmlElement.SetAttribute("sceneName", EditorApplication.currentScene);

        //遍历所有object
        foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
        {
            //如果对象是激活状态
            if (obj.transform.parent == null && obj.activeSelf)
            {
                //如果是预设
               
                PrefabType perfabType = PrefabUtility.GetPrefabType(obj);
                if (perfabType == PrefabType.PrefabInstance 
                    || perfabType == PrefabType.ModelPrefabInstance)
                {
                     //获取预设根物体
                     Object prefabRoot = PrefabUtility.GetPrefabParent(obj);
                    if (prefabRoot!= null)
                    {
                        //资源
                        string scenePath = AssetDatabase.GetAssetPath(prefabRoot);

                        string[] path = scenePath.Split('/');
                        //拷贝到StreamingAssets
                        string perfab = path[path.Length - 1];

                        if (File.Exists(strPerfabFloder+perfab))
                        {
                            File.Delete(strPerfabFloder + perfab);
                        }

                        File.Copy(scenePath, strPerfabFloder+perfab);
                        //获取预设引用对象
                        XmlElement gameObjectXmlElement = xmlDoc.CreateElement("gameObject");
                        gameObjectXmlElement.SetAttribute("objectName", obj.name);
                        gameObjectXmlElement.SetAttribute("objectAsset",prefabRoot.name + ".prefab");

                        //获取transform
                        XmlElement trans = xmlDoc.CreateElement("transform");

                        //获取position
                        XmlElement pos = xmlDoc.CreateElement("position");
                        XmlElement pos_x = xmlDoc.CreateElement("x");
                        pos_x.InnerText = obj.transform.position.x + "";
                        XmlElement pos_y = xmlDoc.CreateElement("y");
                        pos_y.InnerText = obj.transform.position.y + "";
                        XmlElement pos_z = xmlDoc.CreateElement("z");
                        pos_z.InnerText = obj.transform.position.z + "";
                        pos.AppendChild(pos_x);
                        pos.AppendChild(pos_y);
                        pos.AppendChild(pos_z);

                        //获取rotation
                        XmlElement rotation = xmlDoc.CreateElement("rotation");
                        XmlElement rotation_x = xmlDoc.CreateElement("x");
                        rotation_x.InnerText = obj.transform.rotation.eulerAngles.x + "";

                        XmlElement rotation_y = xmlDoc.CreateElement("y");
                        rotation_y.InnerText = obj.transform.rotation.eulerAngles.y + "";

                        XmlElement rotation_z = xmlDoc.CreateElement("z");
                        rotation_z.InnerText = obj.transform.rotation.eulerAngles.z + "";

                        rotation.AppendChild(rotation_x);
                        rotation.AppendChild(rotation_y);
                        rotation.AppendChild(rotation_z);

                        //获取scale
                        XmlElement scale = xmlDoc.CreateElement("scale");
                        XmlElement scale_x = xmlDoc.CreateElement("x");
                        scale_x.InnerText = obj.transform.localScale.x + "";
                        XmlElement scale_y = xmlDoc.CreateElement("y");
                        scale_y.InnerText = obj.transform.localScale.y + "";
                        XmlElement scale_z = xmlDoc.CreateElement("z");
                        scale_z.InnerText = obj.transform.localScale.z + "";
                        scale.AppendChild(scale_x);
                        scale.AppendChild(scale_y);
                        scale.AppendChild(scale_z);

                        trans.AppendChild(pos);
                        trans.AppendChild(rotation);
                        trans.AppendChild(scale);

                        //添加物体
                        gameObjectXmlElement.AppendChild(trans);
                        sceneXmlElement.AppendChild(gameObjectXmlElement);
                        root.AppendChild(sceneXmlElement);
                    }
                }
            }
        }

        xmlDoc.AppendChild(root);
        xmlDoc.Save(strFilePath);

        //刷新Project视图， 不然需要手动刷新哦
        AssetDatabase.Refresh();
    }

    [MenuItem("Export/ExportToAssetBundle")]
    static void ExportSceneToAssetBundle()
    {
        string assetPath = Application.streamingAssetsPath + @"/Perfab/";
        string bundlePath = Application.streamingAssetsPath + @"/AssetBundle/";

        if (!Directory.Exists(bundlePath))
        {
            Directory.CreateDirectory(bundlePath);
        }


        string[] filepath = Directory.GetFiles(assetPath);
        foreach (string strFileName in filepath)
        {
            if (strFileName.EndsWith(".meta"))
                continue;

            string strFilepath = strFileName.Substring(strFileName.LastIndexOf("/")+1);
            Object o = AssetDatabase.LoadAssetAtPath(@"Assets/StreamingAssets/Perfab/" + strFilepath,typeof(GameObject));

            if (o != null)
            {
                //buildAsset
                strFilepath = strFilepath.Replace(".prefab", ".assetbundle");
                BuildPipeline.BuildAssetBundle(o, null, bundlePath + strFilepath, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets 
                    | BuildAssetBundleOptions.DeterministicAssetBundle,BuildTarget.StandaloneWindows);
            }

        }

        AssetDatabase.Refresh();
    }

    static void  GetFileList(List<string> filepathlist,string filepath)
    {
        //获取里面所有的文件
        if (!Directory.Exists(filepath))
            return;

        string[] fileinfolist =Directory.GetFiles(filepath);
        foreach (string fileinfo in fileinfolist)
        {
            if (fileinfo.EndsWith(".svn") || fileinfo.EndsWith(".meta") || fileinfo.EndsWith(".prefab"))
                continue;
            //去除filetxt
            if (fileinfo.EndsWith("files.txt"))
                continue;

            filepathlist.Add(fileinfo);
        }

        //递归访问所有路径
        string[] dirpath = Directory.GetDirectories(filepath);

        for (int i = 0; i < dirpath.Length; ++i )
        {
            GetFileList(filepathlist, dirpath[i]);
        }
    }

    [MenuItem("Export/Create Md5")]
    static void CreateFileMd5()
    {
        if (!Directory.Exists(Util.AppContentPath()))
        {
            Directory.CreateDirectory(Util.AppContentPath());
        }
        //产生md5文件
        string files = Util.AppContentPath() + "files.txt";

        if (File.Exists(files))
        {
            File.Delete(files);
        }

        //打开文件路径
        List<string> filelist = new List<string>();

        GetFileList(filelist, Util.AppContentPath());


        using (FileStream write = new FileStream(files, FileMode.OpenOrCreate))
        {
            //计算md5
            for (int i = 0; i < filelist.Count; ++i)
            {
                string filepath = filelist[i];
                string md5value = Util.md5file(filepath);


                //取值
                filepath = filepath.Substring(Util.AppContentPath().Length);

                string value = filepath + "|" + md5value + "\r\n";

                byte[] data = System.Text.Encoding.Default.GetBytes(value);
                write.Write(data, 0, data.Length);
            }
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Export/ExportDepenceScene")]
    static void ExportScene()
    {
        Caching.CleanCache();
        //导出场景
        string sceneExport = Util.AppContentPath() + "Scene/";

        //创建文件夹
        if (!Directory.Exists(sceneExport))
        {
            Directory.CreateDirectory(sceneExport);
        }

        //打包场景，获取build里面的场景
        EditorBuildSettingsScene[] settingscenes = EditorBuildSettings.scenes;

        for (int i = 1; i < settingscenes.Length;++i )
        {
            EditorBuildSettingsScene scene = settingscenes[i];

           // if (scene.enabled)
            {
                string[] strlist = scene.path.Split('/');

                string strFilename = strlist[strlist.Length - 1];//最后一个元素

                strFilename = strFilename.Replace(".unity",".unity3d");

                //buildstreamsceneAssetBundle和Buildplayer一样
                BuildPipeline.BuildStreamedSceneAssetBundle(new string[1] { scene.path }, sceneExport + strFilename, BuildTarget.StandaloneWindows);
            }
        }
    }
}
