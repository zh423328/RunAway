/*    
 * Copyright (c) 2014.9 , Yongda Chen 411416311.com
 * All rights reserved.
 * Use, modification and distribution are subject to the "New BSD License"
*/

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;



public class ExportLuaSyntax  {
    public static string CLASS_FOAMART = "{1} = class({1})\n"; 
    //ClassMethodInfo
	public class ClassMethodInfo{
		public static string HELP_FORMART =
			@"
--- <summary>
--- 全名:{0}{1}{2}
--- 返回值 : {3}
--- </summary>";

		public static string HELP_PARA_FORMAT = "\n--- arg[{0}] : {1}";
		public static string HELP_OVERRIDE_FORMAT = "\n--- 重载{0} :\n";
		public static string HELP_FUN_FORMAT = "--- function {0}{1}{2}({3}) end";
		public static string HELP_RETURN_FORMAT = "\n--- <returns type=\"{0}\"></returns>";
		public static string FUNCTION_FORMAT = "\nfunction {0}{1}{2}({3}) end";
		public static string FUNC_PARA_FORMAT = "--- arg[{0}] : {1}\n";
        public static string PF_FORMAT = "\n{0}.{1} = function() end";
        public static string PF_STATE = "[{0}]";

		
		public string fullName;
		public string className;
		public string name;
		public List< List< KeyValuePair<string,string> > >  overrideList = new List< List<KeyValuePair<string, string>> >();		
		public bool isStatic ;
        public bool isCanRead;
        public bool isCanWrite;
        public bool isPf = false;
        private string returnName_;
        public string returnName {
            get { return returnName_; }
            set {
                if (string.IsNullOrEmpty(returnName_) || returnName_ == "Void") {
                    returnName_ = value;
                }
            }
        }

        public void DisposReturnName() {
            returnName_ = returnName_.EndsWith("[]") ? "Array" : returnName_;
        }

		public string Desc(bool isLog){
            DisposReturnName();
            if (isPf){
                return DescPf(isLog);
            }
            else {
                return DescMethod(isLog);
            }
		}

        public string DescPf(bool isLog) {
            string strIsStatic = isStatic ? " [静态] " : "";
            string strReadWrite = isCanRead && isCanWrite ? " [读写] " : "";
            string strCanRead = isCanRead ? " [只读] " : "";
            string strCanWrite = isCanWrite ? " [只写] " : "";
            string midStr = isCanRead && isCanWrite ? strReadWrite : strCanRead + strCanWrite;
            string helpStr = string.Format(HELP_FORMART, fullName, strIsStatic, midStr, returnName);
            string func = string.Format(PF_FORMAT, className, name);
            string returnStr = string.Format(HELP_RETURN_FORMAT, returnName);
            string all = helpStr + returnStr + func;
            return all;
        }

        public string DescMethod(bool isLog) {
            string argStr = string.Empty;
            string symbol = isStatic ? "." : ":";
            for (int a = 0; a < overrideList.Count; a++)
            {
                var paraNameList = overrideList[a];
                string paraArg = string.Empty;
                string paraStr = string.Empty;
                string overrideTile = overrideList.Count == 1 ? "\n" : string.Format(HELP_OVERRIDE_FORMAT, a);
                for (int i = 0; i < paraNameList.Count; i++)
                {
                    string argHelp = paraNameList[i].Key + " " + paraNameList[i].Value;
                    //string argFunc = paraNameList[i].Key + "_" + paraNameList[i].Value; 
                    paraArg += i == 0 ? argHelp : "," + argHelp;
                    paraStr += string.Format(HELP_PARA_FORMAT, i, argHelp);
                }
                string funcStr = string.Format(HELP_FUN_FORMAT, className, symbol, name, paraArg);
                argStr += overrideTile + funcStr + paraStr;
            }
            string strIsStatic = isStatic ? " [静态] " : "";
            string helpStr = string.Format(HELP_FORMART, fullName, strIsStatic, argStr, returnName);
            string returnStr = string.Format(HELP_RETURN_FORMAT, returnName);
            string func = string.Format(FUNCTION_FORMAT, className, symbol, name, "");
            string all = helpStr + returnStr + func;
            if (isLog)
            {
                Debug.Log(all);
            }
            return all;
        }
	}
    public static BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

    public static string FisrtCharToLower(string name)
    {

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < name.Length;++i )
        {
            if (i == 0)
            {
                if (name[0] >= 'a' && name[0] <= 'z')
                {
                   builder.Append(name[0] - 32);
                }
            }
            else
            {
                builder.Append(name[i]);
            }
        }

        return builder.ToString();
    }
	public static string Export(System.Type type){
		if (!type.IsGenericType && type.BaseType != typeof(System.MulticastDelegate) &&
		    !typeof(YieldInstruction).IsAssignableFrom(type) )
		{
            string typeName = type.Name;          
			Dictionary<string,ClassMethodInfo> cmfDict = new Dictionary<string,ClassMethodInfo>();

            DisposCtor(type,cmfDict);
            DisposMethods(type,cmfDict);
            DisposProperties(type,cmfDict);
            DisposField(type,cmfDict);
            //PropertyInfo[] propertyArray = type.GetProperties();            
            string aliasName = FisrtCharToLower(typeName);
            string classStr = string.Format(CLASS_FOAMART, typeName, typeName);// +string.Format(CLASS_FOAMART, aliasName, aliasName);
           // string newFuncStr = string.Format(CONSTRUCOR_FORMAT, typeName, typeName, typeName);
            string content = classStr ;
			foreach(ClassMethodInfo f in cmfDict.Values){
				content +=f.Desc(false);
			}
            return content;
		}
        return "";
	}

    public static void DisposField(System.Type type, Dictionary<string, ClassMethodInfo> cmfDict) {
       FieldInfo[] array =  type.GetFields(BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance | BindingFlags);
       foreach (FieldInfo info in array)
       {
           string key = type.Namespace + "." + type.Name + "." + info.Name;
           ClassMethodInfo cmf = !cmfDict.ContainsKey(key) ? new ClassMethodInfo() : cmfDict[key];
           cmf.fullName = key;
           cmf.className = type.Name;
           cmf.name = info.Name;
           cmf.returnName = info.FieldType.Name;
           cmf.isStatic = info.IsStatic;
           cmf.isCanRead = true;
           cmf.isCanWrite = true;
           cmf.isPf = true;
           cmfDict[key] = cmf;
       }
    }

    public static void DisposProperties(System.Type type, Dictionary<string, ClassMethodInfo> cmfDict)
    {
        PropertyInfo[] array = type.GetProperties(BindingFlags | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        foreach(PropertyInfo info in array){
            string key = type.Namespace + "." + type.Name + "." + info.Name;
            ClassMethodInfo cmf = !cmfDict.ContainsKey(key) ? new ClassMethodInfo() : cmfDict[key];
            cmf.fullName = key;
            cmf.className = type.Name;
            cmf.name = info.Name;
            cmf.returnName = info.PropertyType.Name;
            cmf.isStatic = false;
            cmf.isCanRead = info.CanRead;
            cmf.isCanWrite = info.CanRead;
            cmf.isPf = true;
            cmfDict[key] = cmf;
        }       
    }

    public static void DisposCtor(System.Type type, Dictionary<string, ClassMethodInfo> cmfDict)
    {
        string fullNamePrefix = type.Namespace + "." + type.Name + "." +  "New";
        string newStr = "New";
        ConstructorInfo[] ctorArray = type.GetConstructors(BindingFlags.Instance | BindingFlags);
        foreach (ConstructorInfo cInfo in ctorArray)
        {            
            ClassMethodInfo ctorCmf = !cmfDict.ContainsKey(newStr) ? new ClassMethodInfo() : cmfDict[newStr];
            ctorCmf.fullName = fullNamePrefix + newStr;
            ctorCmf.className = type.Name;
            ctorCmf.name = newStr;
            ctorCmf.returnName = type.Name;
            ctorCmf.isStatic = true;
            cmfDict[newStr] = ctorCmf;
            ctorCmf.overrideList.Add(DisposMethodArgs(cInfo.GetParameters()));
        }

    }

	public static void DisposMethods(System.Type type,Dictionary<string,ClassMethodInfo> cmfDict){
        BindingFlags options = BindingFlags | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        MethodInfo[] infoArray = type.GetMethods(options);

		foreach(MethodInfo info in infoArray){
			if(info.IsGenericMethod) continue;			
			if(info.Name.IndexOf('_') > 0 ) continue;
			string key = type.Namespace +"." + type.Name +"."+ info.Name;
			ClassMethodInfo cmf = !cmfDict.ContainsKey(key)?  new ClassMethodInfo() :cmfDict[key];
			cmf.fullName =  key;
			cmf.className = type.Name;
			cmf.name = 	info.Name;
			cmf.returnName = info.ReturnType.Name;
			cmf.isStatic = info.IsStatic;
			cmfDict[key] = cmf;
            cmf.overrideList.Add(DisposMethodArgs( info.GetParameters() ) );
		}
	}

    public static List<KeyValuePair<string, string>> DisposMethodArgs(ParameterInfo[] pInfoArray)
    {
        List<KeyValuePair<string, string>> tmpList = new List<KeyValuePair<string, string>>();
        foreach (ParameterInfo pInfo in pInfoArray)
        {
            KeyValuePair<string, string> pair = new KeyValuePair<string, string>(pInfo.ParameterType.Name, pInfo.Name);
            tmpList.Add(pair);
        }
        return tmpList;
    }

    [MenuItem("Tool/ExportLuaSyntax")]
    public static void ExportLua()
    {
        string all  = string.Empty;
        all += Export( typeof(System.Object) );    
        foreach(var luaB in LuaBinding.binds){//这里出错了？将BindLua文件里的LuaBinding.binds修饰为public
            all += "\n";
            all +=Export(luaB.type);          
        }
         string path = Application.dataPath + "/U3DApi.lua";
        if (File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, all, System.Text.Encoding.UTF8);
        AssetDatabase.Refresh();        
	}

    [MenuItem("Tool/ExportLuaSyntaxTest")]
    public static void ExportLuaSyntax_Test()
    {
        string all = string.Empty;
        all += Export(typeof(GameObject));

        string path = Application.dataPath + "/U3DApi_GameObject.lua";
        if (File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, all, System.Text.Encoding.UTF8);
        AssetDatabase.Refresh();
    }

}
