using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AppFacade : Facade
{
    private static AppFacade _instance;

    public AppFacade() : base()
    {

    }

    public static AppFacade Instance
    {
        get
        {
            if (_instance == null) 
            {
                _instance = new AppFacade();
            }
            return _instance;
        }
    }

    protected override  void InitFramework()
    {
        base.InitFramework();
        //注册命令
        RegisterCommand(NotiConst.START_UP, typeof(StartUpCommand));    
    }

    /// <summary>
    /// 启动框架
    /// </summary>
    public void StartUp() 
    {
        SendMessageCommand(NotiConst.START_UP);

        //移除命令
        RemoveCommand(NotiConst.START_UP);
    }
}

