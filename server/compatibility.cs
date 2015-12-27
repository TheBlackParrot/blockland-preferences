$RTB::RTBR_ServerControl_Hook = true; // RTB is totally on guys

// Server_Prefs hooks too
$RTB::Hooks::ServerControl = true; // RTB is totally on guys
$ORBS::Hooks::ServerControl = true; // yup oRBs too

package BLPrefCompatibilityPackage {
	function RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback) {
		%type = getWord(%params, 0);
		// type checks moved to server.cs, considering them shorthand
		// some prefs have "$", some don't
		registerPref(%addon, "General", %name, %type, "$" @ strReplace(%variable, "$", ""), %default, getWords(%params, 1), %callback, 1);

		if(isFunction("RTB_registerPref"))
			parent::RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback);
	}

	// because oRBs is the same thing at this point -_-
	// so much for "innovation", amirite?
	function oRBs_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback) {
		RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback);

		if(isFunction("oRBs_registerPref"))
			parent::oRBs_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback);
	}
};
activatePackage(BLPrefCompatibilityPackage);
