
TestUICtrl = {};
local this = TestUICtrl;

local testbtn;
local transform;
local gameObject;

--构建函数--
function TestUICtrl.New()
	warn("TestUICtrlCtrl.New--->>");
	return this;
end

function TestUICtrl.Awake()
	warn("TestUICtrlCtrl.Awake--->>");
	PanelManager:CreatePanel('TestUI', this.OnCreate);
end

--启动事件--
function TestUICtrl.OnCreate(obj)
	gameObject = obj;

	local panel = gameObject:GetComponent('UIPanel');
	panel.depth = 20;	--设置纵深--

	testbtn = gameObject:GetComponent('LuaBehaviour');
	testbtn:AddClick(TestUIPanel.btnOpen, this.OnClick);

	warn("Start lua--->>"..gameObject.name);
end

--单击事件--
function TestUICtrl.OnClick(go)
	destroy(gameObject);
end

--关闭事件--
function TestUICtrl.Close()
	PanelManager:ClosePanel(CtrlName.TestUI);
end