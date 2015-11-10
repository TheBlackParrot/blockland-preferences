function registerServerSettingPrefs() {
	%cat = "Server";
	%icon = "server_edit";

	registerBlocklandPref(%cat, "Server Name", "string", "$Pref::Server::Name", $Pref::Server::Name, "64 1", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Welcome Message", "string", "$Pref::Server::WelcomeMessage", $Pref::Server::WelcomeMessage, "512 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Maximum Players", "number", "$Pref::Server::MaxPlayers", $Pref::Server::MaxPlayers, "1 99 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Server Password", "password", "$Pref::Server::Password", $Pref::Server::Password, "512 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Admin Password", "password", "$Pref::Server::AdminPassword", $Pref::Server::AdminPassword, "512 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Super Admin Password", "password", "$Pref::Server::SuperAdminPassword", $Pref::Server::SuperAdminPassword, "512 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Port", "number", "$Pref::Server::Port", $Pref::Server::Port, "1 65536 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Enable E-Tard Filter", "boolean", "$Pref::Server::ETardFilter", $Pref::Server::ETardFilter, "", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "E-Tard List", "string", "$Pref::Server::ETardList", $Pref::Server::ETardList, "512 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Enable Falling Damage", "boolean", "$Pref::Server::FallingDamage", $Pref::Server::FallingDamage, "", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Maximum Bricks", "number", "$Pref::Server::BrickLimit", $Pref::Server::BrickLimit, "1 1000000 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Maximum Bricks/second", "number", "$Pref::Server::MaxBricksPerSecond", $Pref::Server::MaxBricksPerSecond, "1 9999 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Randomly color bricks?", "boolean", "$Pref::Server::RandomBrickColor", $Pref::Server::RandomBrickColor, "", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Too Far Distance", "number", "$Pref::Server::TooFarDistance", $Pref::Server::TooFarDistance, "1 999999 0", "updateServerSetting", %icon, 0);
	registerBlocklandPref(%cat, "Allow unranked players to use events?", "boolean", "$Pref::Server::WrenchEventsAdminOnly", $Pref::Server::WrenchEventsAdminOnly, "", "updateServerSetting", %icon, 0);

	// as an example later on with colors, allow all player shapeNameColors to be set

	// TODO: add wrapper to do callbacks, some of these need webcom_postserver()

	$BLPrefs::AddedServerSettings = true;
}
registerServerSettingPrefs();

function updateServerSetting(%value, %client, %prefSO) {
	%title = %prefSO.title;

	if(%title $= "Server Name" || %title $= "Maximum Players" || %title $= "Server Password") {
		webcom_postserver();
	}
}
