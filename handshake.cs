package BLPrefServerPackage {
	function GameConnection::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p) {
		if(getField(%us, 0) !$= "") {
			%this.hasPrefSystem = 1;
			if($Pref::BLPrefs::ServerDebug) {
				echo("\c4" @ %this.name SPC "has the pref system. (version" SPC getField(%us, 0) @ ")");
			}
		}
		return parent::onConnectRequest(%this, %a, %b, %c, %d, %e, %f, %g, %us, %i, %j, %k, %l, %m, %n, %o, %p);
	}
};
activatePackage(BLPrefServerPackage);