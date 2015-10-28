$RTB::RTBR_ServerControl_Hook = true;
function RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback) {
	%type = getWord(%params, 0);
	switch$(%type) {
		case "int" or "num" or "float":
			%type = "integer";

		case "bool":
			%type = "boolean";
	}
	registerBlocklandPref(%addon, %name, %type, "$" @ %variable, %default, getWords(%params, 1), %callback, "bricks");
}