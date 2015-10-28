function RTB_registerPref(%name, %addon, %variable, %params, %file, %default, %requiresRestart, %hostOnly, %callback) {
	registerBlocklandPref(%addon, %name, getWord(%params, 0), "$" @ %variable, %default, getWords(%params, 1), %callback);
}