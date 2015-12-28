// --------------------------------------------------------------------------------------------
// Client-Side code.
// --------------------------------------------------------------------------------------------

// preload package.
if(isPackage(prefsLoadHack))
	deactivatePackage(prefsLoadHack);

$BLPrefs::ClVersion = "0.0-dev";

exec("support_preload.cs");

// SUPPORT
exec("./support/variables.cs");
exec("./support/math.cs");

// CLIENT STUFF
exec("./client/prefmanager/main.cs");