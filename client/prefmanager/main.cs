// LOAD SCRIPTS M8
// --------------------------------------
exec("./prefs.cs");
exec("./gui.cs");
exec("./commands.cs");

// PACKAGE
// --------------------------------------
$BLPrefs::Connected = false;
$BLPrefs::Allowed = false;

if(isPackage(prefsClient))
	deactivatePackage(prefsClient);

package prefsClient {
	function adminGui::clickServerSettings(%this) {
		//Canvas.popDialog(adminGui);
		//Canvas.popDialog(escapeMenu);
		
		if($BLPrefs::Connected) {
			if($BLPrefs::Allowed)
				Canvas.pushDialog(NewSettingsGui);
			else
				MessageBoxOK("Oops!", "You don't have permission to open the prefs menu. Sorry!");
		}
		else
			Parent::clickServerSettings(%this);
	}
	
	function disconnectedCleanup(%a)
	{
		$BLPrefs::Connected = false;
		$BLPrefs::Allowed = false;
		
		Parent::disconnectedCleanup(%a);
	}
};

activatePackage(prefsClient);