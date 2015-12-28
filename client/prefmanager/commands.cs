// EXTERNAL COMMANDS
// --------------------------------------
function clientCmdReceiveCategory(%id, %category, %icon) {
	newSettingsGui.addCategory(%id, %category, %icon);
}

function clientCmdReceivePref(%catId, %id, %name, %subcategory, %type, %params, %default, %returnName, %value) {
	// we have no need for half of these args but we'll send em over anyway
	%data = new ScriptObject()
	{
		// IMPORTANT
		catId = %catId;
		serverId = %id;
		name = %name;
		type = %type SPC %params; // hack to make my old system cooperate with our changes
		
		value = %value;
		oldValue = %value;
		returnName = %returnName;
		
		// USEFUL
		subcategory = %subcategory; // dictates where we add dividers
		variableDefault = %default; // naming this variable "default" causes a syntax error because "default" is a keyword in TS.
	};
	
	newSettingsGui.addSetting(%data);
}

function clientCmdBL_IDListPlayer(%name, %bl_id) {
	bl_idListGui_PlayerList.addRow(%bl_id, %bl_id @ ": " @ %name);
}

function clientCmdHasPrefSystem(%version, %canIUse) {
	$BLPrefs::Connected = true;
	$BLPrefs::Allowed = %canIUse;
	
	if(%version != $BLPrefs::ClVersion) {
		MessageBoxOK("Warning", "The pref system running on this server is a different version to yours. Just a heads up.\n\nServer: " @ %version @ "\nYou: " @ $BLPrefs::ClVersion);
	}
}