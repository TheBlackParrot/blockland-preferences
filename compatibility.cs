$RTB::RTBR_ServerControl_Hook = true;

package BLPrefCompatibilityPackage {
	function RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback) {
		%type = getWord(%params, 0);
		switch$(%type) {
			case "int" or "num" or "float":
				%type = "number";

			case "bool":
				%type = "boolean";
		}
		// some prefs have "$", some don't
		registerBlocklandPref(%addon, %name, %type, "$" @ strReplace(%variable, "$", ""), %default, getWords(%params, 1), %callback, "bricks", 1);

		if(isFunction("RTB_registerPref"))
			parent::RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback);
	}
};
activatePackage(BLPrefCompatibilityPackage);
