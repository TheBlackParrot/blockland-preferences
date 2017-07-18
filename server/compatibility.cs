$RTB::RTBR_ServerControl_Hook = true; // RTB is totally on guys

if(isFile("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs"))
	exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");
$RTB::Hooks::ServerControl = true; // RTB is totally on guys

if(isFile("Add-Ons/System_oRBs/hooks/serverControl.cs"))
	exec("Add-Ons/System_oRBs/hooks/serverControl.cs");
$ORBS::Hooks::ServerControl = true; // yup oRBs too

if(!isFile("Add-Ons/System_ReturnToBlockland/server.cs")) { // the addon does not need to be *valid*, the server.cs just needs to exist
	%fo = new FileObject();
	%fo.openForWrite("Add-Ons/System_ReturnToBlockland/server.cs");
	%fo.writeLine("// This is an empty, fake RTB folder so that RTB prefs will load."); // if you're going to change this, ensure file CRC is changed on line 63
	%fo.close();
	%fo.delete();
}

function rtbLegacyPref::onUpdate(%this, %val, %cl) {
	if(%this.rtbCallback !$= "") {
		call(%this.rtbCallback, %this.lastVal, %val);
	}

	%this.lastVal = %val;
}

function rtbLegacyPref::onLoad(%this, %val) {
	%this.lastVal = %val;
}

function rtbLegacyPref::onDefault(%this, %val) {
	if(%this.rtbCallback !$= "") {
		call(%this.rtbCallback, %this.lastVal, %val);
	}

	%this.lastVal = %val;
}

//polyfill
if(!isFunction(oRBs_registerPref)) eval("function oRBs_registerPref() {}");
if(!isFunction(RTB_registerPref))  eval("function RTB_registerPref()  {}");

package BLPrefCompatibilityPackage {
	function RTB_registerPref(%name, %addon, %variable, %params, %filename, %default, %requiresRestart, %hostOnly, %callback) {
		//if(isFunction("RTB_registerPref")) {
		//	parent::RTB_registerPref(%name, %addon, %variable, %params, %filename, %default, %requiresRestart, %hostOnly, %callback);
		//}

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

		%pref = new ScriptObject(Preference) {
			className       = "rtbLegacyPref";

			addon           = %cat;
			category        = %sub;
			title           = %name;

			type            = %type;
			params          = getWords(%params, 1);

			variable        = %variable;
			defaultValue    = %default;

			rtbCallback     = %callback;

			hostOnly        = %hostOnly;
			requiresRestart = %requiresRestart;

			loadNow         = false;
		};

		%group = %pref.getGroup();
		%group.legacy = true;
		%group.icon   = "bricks";
		%group.name   = %cat;
	}

	function oRBs_registerPref(%name, %addon, %variable, %params, %filename, %default, %requiresRestart, %hostOnly, %callback) {
		warn("oRBs is bad and you should not be using it.");
		RTB_registerPref(%name, %addon, %variable, %params, %filename, %default, %requiresRestart, %hostOnly, %callback);

		if(isFunction("oRBs_registerPref")) {
			parent::oRBs_registerPref(%name, %addon, %variable, %params, %filename, %default, %requiresRestart, %hostOnly, %callback);
		}
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
