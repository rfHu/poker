const fs  =  require("fs");

const defaultConfig = {"m_IconGlyphList":[{"m_Name":"left2start","m_Unicode":"E949"},{"m_Name":"right2end","m_Unicode":"E94a"},{"m_Name":"poker_11","m_Unicode":"E938"},{"m_Name":"poker_12","m_Unicode":"E939"},{"m_Name":"poker_13","m_Unicode":"E93a"},{"m_Name":"pattern_0","m_Unicode":"E944"},{"m_Name":"pattern_1","m_Unicode":"E945"},{"m_Name":"pattern_2","m_Unicode":"E946"},{"m_Name":"pattern_3","m_Unicode":"E947"},{"m_Name":"poker_1","m_Unicode":"E948"},{"m_Name":"poker_2","m_Unicode":"E93b"},{"m_Name":"poker_3","m_Unicode":"E93c"},{"m_Name":"poker_4","m_Unicode":"E93d"},{"m_Name":"poker_5","m_Unicode":"E93E"},{"m_Name":"poker_6","m_Unicode":"E93f"},{"m_Name":"poker_7","m_Unicode":"E940"},{"m_Name":"poker_8","m_Unicode":"E941"},{"m_Name":"poker_9","m_Unicode":"E942"},{"m_Name":"poker_10","m_Unicode":"E943"},{"m_Name":"menu_11","m_Unicode":"E937"},{"m_Name":"mtt","m_Unicode":"E936"},{"m_Name":"baoming_chip","m_Unicode":"E935"},{"m_Name":"rank","m_Unicode":"E934"},{"m_Name":"sng","m_Unicode":"E933"},{"m_Name":"biaoqian","m_Unicode":"E932"},{"m_Name":"write","m_Unicode":"E930"},{"m_Name":"menu_10","m_Unicode":"E931"},{"m_Name":"menu_9","m_Unicode":"E92f"},{"m_Name":"zoom","m_Unicode":"E92E"},{"m_Name":"kaishi","m_Unicode":"E92d"},{"m_Name":"next","m_Unicode":"E92c"},{"m_Name":"jieshu","m_Unicode":"E927"},{"m_Name":"xianzhi1","m_Unicode":"E928"},{"m_Name":"xianzhi2","m_Unicode":"E929"},{"m_Name":"xianzhi3","m_Unicode":"E92a"},{"m_Name":"zanting","m_Unicode":"E92b"},{"m_Name":"qipao","m_Unicode":"E926"},{"m_Name":"logo","m_Unicode":"E925"},{"m_Name":"delete","m_Unicode":"E920"},{"m_Name":"menu_7","m_Unicode":"E922"},{"m_Name":"menu_8","m_Unicode":"E923"},{"m_Name":"menu_6","m_Unicode":"E924"},{"m_Name":"selected","m_Unicode":"E901"},{"m_Name":"smile","m_Unicode":"E921"},{"m_Name":"clock","m_Unicode":"E917"},{"m_Name":"microphone","m_Unicode":"E912"},{"m_Name":"battery","m_Unicode":"E904"},{"m_Name":"lagan_7","m_Unicode":"E907"},{"m_Name":"asset","m_Unicode":"E900"},{"m_Name":"chip","m_Unicode":"E902"},{"m_Name":"end","m_Unicode":"E903"},{"m_Name":"fangzu","m_Unicode":"E905"},{"m_Name":"huigu","m_Unicode":"E906"},{"m_Name":"huigu_collection1","m_Unicode":"E908"},{"m_Name":"huigu_collection2","m_Unicode":"E909"},{"m_Name":"huigu_left","m_Unicode":"E90a"},{"m_Name":"huigu_right","m_Unicode":"E90b"},{"m_Name":"huigu_share","m_Unicode":"E90c"},{"m_Name":"lagan_1","m_Unicode":"E90d"},{"m_Name":"lagan_2","m_Unicode":"E90E"},{"m_Name":"lagan_3","m_Unicode":"E90f"},{"m_Name":"lagan_4","m_Unicode":"E910"},{"m_Name":"lagan_5","m_Unicode":"E911"},{"m_Name":"lock","m_Unicode":"E913"},{"m_Name":"menu","m_Unicode":"E914"},{"m_Name":"menu_1","m_Unicode":"E915"},{"m_Name":"menu_2","m_Unicode":"E916"},{"m_Name":"menu_4","m_Unicode":"E918"},{"m_Name":"menu_5","m_Unicode":"E919"},{"m_Name":"message","m_Unicode":"E91a"},{"m_Name":"real_time","m_Unicode":"E91b"},{"m_Name":"remove","m_Unicode":"E91c"},{"m_Name":"stand","m_Unicode":"E91d"},{"m_Name":"suspend","m_Unicode":"E91E"},{"m_Name":"time","m_Unicode":"E91f"}]};

var json = require("./selection.json");
var hash = {};

defaultConfig.m_IconGlyphList.forEach(function(o) {
	hash[o.m_Name] = o.m_Unicode;
	// console.log(o.m_Name + " " + o.m_Unicode);
});

json.icons.forEach(function(o) {
	var name = o.properties.name;

	if (!hash[name]) {
		console.error(`不支持的属性名${name}`);
	}
	
	o.properties.code = parseInt("0x" + hash[name]);	
});

fs.writeFileSync('restore.json', JSON.stringify(json), 'utf8');