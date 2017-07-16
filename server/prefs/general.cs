function registerServerSettingPrefs() {

	if($BLPrefs::AddedServerSettings)
		return;

	registerServerSetting("General", "Server Name", "string", "$Pref::Server::Name", "Support_Preferences", $Pref::Server::Name, "64 1", "updateServerSetting", 0);
	registerServerSetting("General", "Welcome Message", "string", "$Pref::Server::WelcomeMessage", "Support_Preferences", $Pref::Server::WelcomeMessage, "512 0", "updateServerSetting", 0);
	registerServerSetting("General", "Maximum Players", "playercount", "$Pref::Server::MaxPlayers", "Support_Preferences", $Pref::Server::MaxPlayers, "1 99", "updateServerSetting", 0);
	registerServerSetting("Security", "Server Password", "string", "$Pref::Server::Password", "Support_Preferences", $Pref::Server::Password, "512 0", "updateServerSetting", 0, 1);
	registerServerSetting("Security", "Admin Password", "string", "$Pref::Server::AdminPassword", "Support_Preferences", $Pref::Server::AdminPassword, "512 0", "updateServerSetting", 0, 1);
	registerServerSetting("Security", "Super Admin Password", "string", "$Pref::Server::SuperAdminPassword", "Support_Preferences", $Pref::Server::SuperAdminPassword, "512 0", "updateServerSetting", 0, 1, 1);
	registerServerSetting("Security", "Enable E-Tard Filter", "boolean", "$Pref::Server::ETardFilter", "Support_Preferences", $Pref::Server::ETardFilter, "", "updateServerSetting", 0);
	registerServerSetting("Security", "E-Tard List", "wordlist", "$Pref::Server::ETardList", "Support_Preferences", $Pref::Server::ETardList, ", 51", "updateServerSetting", 0);
	registerServerSetting("Security", "Who can change preferences?", "list", "$Pref::BLPrefs::AllowedRank", "Support_Preferences", $Pref::BLPrefs::AllowedRank, "Host 3 Super_Admin 2 Admin 1", "updateBLPrefPermission", 0, 0, 1);
	registerServerSetting("Security", "Auto Admin IDs", "userlist", "$Pref::Server::AutoAdminList", "Support_Preferences", $Pref::Server::AutoAdminList, "_ -1", "updateBLPrefPermission", 0, 0, 0);
	registerServerSetting("Security", "Auto Super Admin IDs", "userlist", "$Pref::Server::AutoSuperAdminList", "Support_Preferences", $Pref::Server::AutoSuperAdminList, "_ -1", "updateBLPrefPermission", 0, 0, 1);
	registerServerSetting("Gameplay", "Enable Falling Damage", "boolean", "$Pref::Server::FallingDamage", "Support_Preferences", $Pref::Server::FallingDamage, "", "updateServerSetting", 0);
	registerServerSetting("Gameplay", "Maximum Bricks/second", "number", "$Pref::Server::MaxBricksPerSecond", "Support_Preferences", $Pref::Server::MaxBricksPerSecond, "1 9999 0", "updateServerSetting", 0);
	registerServerSetting("Gameplay", "Randomly color bricks?", "boolean", "$Pref::Server::RandomBrickColor", "Support_Preferences", $Pref::Server::RandomBrickColor, "", "updateServerSetting", 0);
	registerServerSetting("Gameplay", "Too Far Distance", "number", "$Pref::Server::TooFarDistance", $Pref::Server::TooFarDistance, "Support_Preferences", "1 999999 0", "updateServerSetting", 0);
	registerServerSetting("Gameplay", "Wrench events are admin only?", "boolean", "$Pref::Server::WrenchEventsAdminOnly", "Support_Preferences", $Pref::Server::WrenchEventsAdminOnly, "", "updateServerSetting", 0);

	// as an example later on with colors, allow all player shapeNameColors to be set

	$BLPrefs::AddedServerSettings = true;
}
registerServerSettingPrefs();

function pushServerName() {
	%name = $Pref::Player::NetName;
	if(getSubStr(%name, strlen(%name)-2, 1) $= "s")
		%name = %name @ "'";
	else
		%name = %name @ "'s";

  commandToAll('NewPlayerListGui_UpdateWindowTitle', %name SPC $Pref::Server::Name, $Pref::Server::MaxPlayers);
}

function ServerSettingPref::onUpdate(%prefSO, %value, %client) {
	%title = %prefSO.title;

	if(%title $= "Server Name" || %title $= "Maximum Players" || %title $= "Server Password") {
		webcom_postserver();
		pushServerName();

		for(%i = 0; %i< ClientGroup.getCount(); %i++) {
			%cl = ClientGroup.getObject(%i);
			%cl.sendPlayerListUpdate();
		}
	}

	// headachey ones:
	$Server::Name = $Pref::Server::Name;
	$Server::WelcomeMessage = $Pref::Server::WelcomeMessage;
	$Server::MaxBricksPerSecond = $Pref::Server::MaxBricksPerSecond;
	$Server::WrenchEventsAdminOnly = $Pref::Server::WrenchEventsAdminOnly;
}

function updateBLPrefPermission(%level, %client, %pso) { // let client mods know if they're allowed or not
	for(%i = 0; %i < ClientGroup.getCount(); %i++) {
		%cl = ClientGroup.getObject(%i);
		commandToClient(%cl, 'BLPAllowedUse', %cl.BLP_isAllowedUse());
	}
}

function autoAdminsChanged(%value, %client, %prefSO) {
	// update each player's admin status
	for(%i=0;%i<ClientGroup.getCount();%i++)
	{
		%cl = ClientGroup.getObject(%i);

		%status = %cl.checkAdminStatus();

		if(%status $= 2)
		{
			 if(%cl.isSuperAdmin)
				continue;

			 %cl.isAdmin = 1;
			 %cl.isSuperAdmin = 1;
			 %cl.sendPlayerListUpdate();
			 commandtoclient(%cl,'setAdminLevel',2);
			 messageAll('MsgAdminForce','\c2%1 has become Super Admin (Auto)', %cl.getPlayerName());

			 RTBSC_SendPrefList(%client);
		}
		else if(%status == 1)
		{
			 if(%cl.isAdmin)
				continue;

			 %cl.isAdmin = 1;
			 %cl.isSuperAdmin = 0;
			 %cl.sendPlayerListUpdate();
			 commandtoclient(%cl,'setAdminLevel',1);
			 messageAll('MsgAdminForce','\c2%1 has become Admin (Auto)', %cl.getPlayerName());
		}
		else if(%status == 0)
		{
			 if(!%cl.isAdmin)
				continue;

			 %cl.isAdmin = 0;
			 %cl.isSuperAdmin = 0;
			 %cl.sendPlayerListUpdate();
			 commandtoclient(%cl,'setAdminLevel',0);
			 messageAll('MsgAdminForce','\c2%1 is no longer Admin.', %cl.getPlayerName());
		}
	}
}
