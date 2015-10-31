package BLPrefServerPackage {
	function GameConnection::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p) {
		for(%i = 0; %i < getLineCount(%us); %i++) { //being respectful of other mods, not hogging a whole argument
			%line = getLine(%us, %i);
			if(getField(%line, 0) $= "Prefs") {
				%this.hasPrefSystem = 1;
				if($Pref::BLPrefs::ServerDebug) {
					echo("\c4" @ %this.name SPC "has the pref system. (version" SPC getField(%line, 1) @ ")");
				}
				break;
			}
		}
		return parent::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p);
	}

	function GameConnection::autoAdminCheck(%client) {
		%admin = parent::autoAdminCheck(%client);
		commandToClient(%client, 'hasPrefSystem', $BLPrefs::Version, %admin); //let them know if they have permission to use it; assume that admin is permission for now?
		return %admin;
	}
};
activatePackage(BLPrefServerPackage);
