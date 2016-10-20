$RTB::RTBR_ServerControl_Hook = true; // RTB is totally on guys

if(isFile("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs"))
	exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");
$RTB::Hooks::ServerControl = true; // RTB is totally on guys

if(!isFile("Add-Ons/System_ReturnToBlockland/server.cs")) { // the addon does not need to be *valid*, the server.cs just needs to exist
	%fo = new FileObject();
	%fo.openForWrite("Add-Ons/System_ReturnToBlockland/server.cs");
	%fo.writeLine("// This is an empty, fake RTB folder so that RTB prefs will load."); // if you're going to change this, ensure file CRC is changed on line 49
	%fo.close();
	%fo.delete();
}

package BLPrefCompatibilityPackage {
	function RTB_registerPref(%name, %addon, %variable, %params, %filename, %default, %requiresRestart, %hostOnly, %callback) {
		if(isFunction("RTB_registerPref")) {
			parent::RTB_registerPref(%name, %addon, %variable, %params, %filename, %default, %requiresRestart, %hostOnly, %callback);
		}

		%type = getWord(%params, 0);

		// do some wizardry to sort certain legacy addons better
		%addon = strReplace(%addon, "|", "" TAB "");
		%addon = strReplace(%addon, "-", "" TAB "");
		%addon = strReplace(%addon, ":", "" TAB "");
		%addon = strReplace(%addon, ";", "" TAB "");

		%cat = getField(%addon, 0);
		%sub = getFields(%addon, 1);

		// remove space from the start of %sub
		if(strpos(%sub, " ") == 0) {
			%sub = getSubStr(%sub, 1, strlen(%sub) - 1);
		}

		if(%sub $= "") {
			%sub = "General";
		}

		// type checks moved to server.cs, considering them shorthand
		// some prefs have "$", some don't
		registerPref(%cat, %sub, %name, %type, "$" @ strReplace(%variable, "$", ""), %filename, %default, getWords(%params, 1), %callback, 1);
	}
	
	function ServerSettingsGui::onWake(%this) {
		parent::onWake(%this);
    
		if(getFileCRC("Add-Ons/System_ReturnToBlockland/server.cs") == -1587284330) {
			$ServerSettingsGui::UseRTB = false;
			ServerSettingsGui_RTBLabel.setVisible(false);
			ServerSettingsGui_UseRTB.setVisible(false);
		}
	}
};
activatePackage(BLPrefCompatibilityPackage);
