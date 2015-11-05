function registerBLPrefs() {
	%cat = "BLPrefs";
	%icon = "cog";

	registerBlocklandPref(%cat, "Who can change preferences?", "list", "$Pref::BLPrefs::AllowedRank", $Pref::BLPrefs::AllowedRank, "Host**4|Super Admin**3|Admin**2", "", %icon, 0);
	registerBlocklandPref(%cat, "Announce changes?", "boolean", "$Pref::BLPrefs::Announce", $Pref::BLPrefs::Announce, "", "", %icon, 0);
}
registerBLPrefs();