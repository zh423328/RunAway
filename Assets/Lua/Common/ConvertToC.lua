--为了转换lua为.h文件
local  luaiconv = require "luaiconv"
local g2u = luaiconv.new("utf-8", "gbk");
local u2g = luaiconv.new("gbk", "utf-8");

function luasys_utf82gbk(txt)
   return u2g:iconv(txt);
end

function luasys_gbk2utf8(txt)
   return g2u:iconv(txt);
end


function ExportToC(filename,filenameto)
	-- body--

	local  file = assert(io.open(filename,"rb"));
	local  fileto = io.open(filenameto,"wb");

	if file then
		while true do
			local str = file:read("*line");

			if not str then break end
				
			str = str .. "\n";
			str = luasys_utf82gbk(str);
			local  newstr = string.gsub(str,'(%a+)%s-=%s-[%"\'](%d+)[%"\']%s-(;?)',"const int %1 = %2;\t");

			newstr = string.gsub(newstr,"%s-%-%-.+",
				function (x)
					-- body
					return string.format("// %s",x);
				end)
			fileto:write(newstr);
		end
	end
	file:close();
	fileto:close();
end

--C# utf8文件--
function ExportToCS(filename,filenameto)
	local  file = assert(io.open(filename,"rb"));
	local  fileto = io.open(filenameto,"wb");

    --head--
    fileto:write(luasys_gbk2utf8("namespace SimpleFramework\n{\n\tpublic class Protocal\n\t{\n"));
	if file then
		while true do
			local str = file:read("*line");

			if not str then break end
				
			str = str .. "\n";
			--str = luasys_utf82gbk(str);
			local  newstr = string.gsub(str,'(%a+)%s-=%s-[%"\'](%d+)[%"\']%s-(;?)',"\t\tpublic const int %1 = %2;\t");

			newstr = string.gsub(newstr,"%s-%-%-.+",
				function (x)
					-- body
					return string.format("// %s",x);
				end)
			fileto:write(newstr);
		end
	end

    fileto:write(luasys_gbk2utf8("\t}\n}\n"));

	file:close();
	fileto:close();
end

--ExportToC("C:\\1\\protocal.lua","C:\\1\\protocal.h");
ExportToCS("E:\\GitHub\\RunAway\\Assets\\Lua\\Common\\protocal.lua",
	"E:\\GitHub\\RunAway\\Assets\\Scripts\\Network\\protocal.cs");