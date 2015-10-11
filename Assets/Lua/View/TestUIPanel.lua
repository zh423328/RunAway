local transform;
local gameObject;

TestUIPanel = {};
local this = TestUIPanel;

--启动事件--
function TestUIPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

	this.InitPanel();
	warn("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function TestUIPanel.InitPanel()
	this.btnOpen = transform:FindChild("btn").gameObject;
end

--单击事件--
function TestUIPanel.OnDestroy()
	warn("OnDestroy---->>>");
end