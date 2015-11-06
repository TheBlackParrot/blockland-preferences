function registerBLPrefs() {
	%cat = "BLPrefs";
	%icon = "cog";

	if($Pref::BLPrefs::AllowedRank $= "") {
		$Pref::BLPrefs::AllowedRank = 2;
	}
	if($Pref::BLPrefs::AnnounceChanges $= "") {
		$Pref::BLPrefs::AnnounceChanges = 0;
	}
	
	registerBlocklandPref(%cat, "Who can change preferences?", "list", "$Pref::BLPrefs::AllowedRank", $Pref::BLPrefs::AllowedRank, "Host**3|Super Admin**2|Admin**1", "", %icon, 0);
	registerBlocklandPref(%cat, "Announce changes?", "boolean", "$Pref::BLPrefs::AnnounceChanges", $Pref::BLPrefs::AnnounceChanges, "", "", %icon, 0);
}
registerBLPrefs();