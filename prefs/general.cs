function registerServerSettingPrefs() {
	%cat = "Server";
	%icon = "server_edit";

	registerBlocklandPref(%cat, "Title", "string", "$Pref::Server::Title", $Pref::Server::Title, "64 1", "eval(\"$Server::Title = $Pref::Server::Title;\");", %icon, 0);
	registerBlocklandPref(%cat, "Welcome Message", "string", "$Pref::Server::WelcomeMessage", $Pref::Server::WelcomeMessage, "512 0", "eval(\"$Server::WelcomeMessage = $Pref::Server::WelcomeMessage;\");", %icon, 0);
	registerBlocklandPref(%cat, "Maximum Players", "number", "$Pref::Server::MaxPlayers", $Pref::Server::MaxPlayers, "1 99 0", "eval(\"$Server::MaxPlayers = $Pref::Server::MaxPlayers;\");", %icon, 0);
	registerBlocklandPref(%cat, "Server Password", "password", "$Pref::Server::Password", $Pref::Server::Password, "512 0", "eval(\"$Server::Password = $Pref::Server::Password;\");", %icon, 0);
	registerBlocklandPref(%cat, "Admin Password", "password", "$Pref::Server::AdminPassword", $Pref::Server::AdminPassword, "512 0", "eval(\"$Server::AdminPassword = $Pref::Server::AdminPassword;\");", %icon, 0);
	registerBlocklandPref(%cat, "Super Admin Password", "password", "$Pref::Server::SuperAdminPassword", $Pref::Server::SuperAdminPassword, "512 0", "eval(\"$Server::SuperAdminPassword = $Pref::Server::SuperAdminPassword;\");", %icon, 0);
	registerBlocklandPref(%cat, "Port", "number", "$Pref::Server::Port", $Pref::Server::Port, "1 65536 0", "eval(\"$Server::Port = $Pref::Server::Port;\");", %icon, 0);
	registerBlocklandPref(%cat, "E-Tard Filter", "boolean", "$Pref::Server::ETardFilter", $Pref::Server::ETardFilter, "", "eval(\"$Server::ETardFilter = $Pref::Server::ETardFilter;\");", %icon, 0);
	registerBlocklandPref(%cat, "E-Tard List", "string", "$Pref::Server::ETardList", $Pref::Server::ETardList, "512 0", "eval(\"$Server::ETardList = $Pref::Server::ETardList;\");", %icon, 0);
	registerBlocklandPref(%cat, "Falling Damage", "boolean", "$Pref::Server::FallingDamage", $Pref::Server::FallingDamage, "", "eval(\"$Server::FallingDamage = $Pref::Server::FallingDamage;\");", %icon, 0);
	registerBlocklandPref(%cat, "Maximum Bricks", "number", "$Pref::Server::BrickLimit", $Pref::Server::BrickLimit, "1 1000000 0", "eval(\"$Server::BrickLimit = $Pref::Server::BrickLimit;\");", %icon, 0);
	registerBlocklandPref(%cat, "Maximum Bricks/second", "number", "$Pref::Server::MaxBricksPerSecond", $Pref::Server::MaxBricksPerSecond, "1 9999 0", "eval(\"$Server::MaxBricksPerSecond = $Pref::Server::MaxBricksPerSecond;\");", %icon, 0);
	registerBlocklandPref(%cat, "Random Brick Color", "boolean", "$Pref::Server::RandomBrickColor", $Pref::Server::RandomBrickColor, "", "eval(\"$Server::RandomBrickColor = $Pref::Server::RandomBrickColor;\");", %icon, 0);
	registerBlocklandPref(%cat, "Too Far Distance", "number", "$Pref::Server::TooFarDistance", $Pref::Server::TooFarDistance, "1 999999 0", "eval(\"$Server::TooFarDistance = $Pref::Server::TooFarDistance;\");", %icon, 0);
	registerBlocklandPref(%cat, "Events Are Admin Only", "boolean", "$Pref::Server::WrenchEventsAdminOnly", $Pref::Server::WrenchEventsAdminOnly, "", "eval(\"$Server::WrenchEventsAdminOnly = $Pref::Server::WrenchEventsAdminOnly;\");", %icon, 0);

	// as an example later on with colors, allow all player shapeNameColors to be set

	// TODO: add wrapper to do callbacks, some of these need webcom_postserver()
}
registerServerSettingPrefs();