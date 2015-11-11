function registerBLPrefs() {
	if(!$BLPrefs::Registered) {
		$BLPrefs::Registered = true;
		%cat = "BLPrefs";
		%icon = "cog";

		if($Pref::BLPrefs::AllowedRank $= "") {
			$Pref::BLPrefs::AllowedRank = 2;
		}
		if($Pref::BLPrefs::AnnounceChanges $= "") {
			$Pref::BLPrefs::AnnounceChanges = 0;
		}

		registerBlocklandPref(%cat, "Who can change preferences?", "list", "$Pref::BLPrefs::AllowedRank", $Pref::BLPrefs::AllowedRank, "Host**3|Super Admin**2|Admin**1", "updateBLPrefPermission", %icon, 0);
		registerBlocklandPref(%cat, "Announce changes?", "boolean", "$Pref::BLPrefs::AnnounceChanges", $Pref::BLPrefs::AnnounceChanges, "", "", %icon, 0);
	}
}
registerBLPrefs();

function updateBLPrefPermission(%level, %client, %pso) { // let client mods know if they're allowed or not
	for(%i = 0; %i < ClientGroup.getCount(); %i++) {
		%cl = ClientGroup.getObject(%i);
		commandToClient(%cl, 'BLPAllowedUse', %cl.BLP_isAllowedUse());
	}
}
