package BLPrefServerPackage {
	function GameConnection::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p) {
		for(%i = 0; %i < getLineCount(%us); %i++) { //being respectful of other mods, not hogging a whole argument
			%line = getLine(%us, %i);
			if(getField(%line, 0) $= "Prefs") {
				%this.hasPrefSystem = 1;
				if($Pref::BLPrefs::ServerDebug) {
					echo("\c4" @ %this.name SPC "has the pref system. (version" SPC getField(%line, 1) @ ")");
				}
				commandToClient(%this, 'hasPrefSystem', $BLPrefs::Version);
			}
		}
		return parent::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p);
	}
};
activatePackage(BLPrefServerPackage);
