package BLPrefServerPackage {
	function GameConnection::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p) {
		for(%i = 0; %i < getLineCount(%us); %i++) { //being respectful of other mods, not hogging a whole argument
			%line = getLine(%us, %i);
			if(getField(%line, 0) $= "Prefs") {
				%this.hasPrefSystem = 1;
				if($Pref::BLPrefs::ServerDebug) {
					echo("\c4[Support_Preferences] client has the pref system. (version" SPC getField(%line, 1) @ ")");
				}
				break;
			}
		}
		return parent::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p);
	}

	function GameConnection::autoAdminCheck(%client) {
		%aac = Parent::autoAdminCheck(%client);

		commandToClient(%client, 'hasPrefSystem', $BLPrefs::Version, %client.BLP_isAllowedUse());

		// PreLoader was installed, not already here
    if(!$PreLoadScriptsRun && %client.isSuperAdmin) {
      schedule(52, %client, messageClient, %client, '', "\c5Some preferences may be missing as this is your first time using Support_Preferences!");
      schedule(53, %client, messageClient, %client, '', "\c5Restart Blockland for all preferences to load.");
    }

		return %aac;
	}

	function deactivateServerPackages() { //preload fix
		parent::deactivateServerPackages();
		if($Server::Dedicated) {
			activatePackage(BLPrefServerPackage);
			activatePackage(BLPrefCompatibilityPackage);
			activatePackage(BLPrefBL_IDPackage);
			activatePackage(BLPrefSaveLoadPackage);
		}
	}
};
activatePackage(BLPrefServerPackage);
