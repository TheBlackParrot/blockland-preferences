//saving and loading

function findBLPref(%variable) {
	%fo = new FileObject();
	%fo.openForRead($BLPrefs::File);
	while(!%fo.isEOF()){
		%line = %fo.readLine();
		if(getWord(%line, 0) $= %variable) {
			%pref = %line;
			break;
		}
	}
	%fo.close();
	%fo.delete();

	if(%pref $= "") {
		return -1;
	}

	%var = getWord(%pref, 0);
	%val = getWord(%pref, 2);

	%val = getSubStr(%val, 1, strLen(%val) - 3);

	return %var SPC %val;
}

function loadBLPreferences() {
	if(isFile($BLPrefs::File)) {
		// echo("\c5[Support_Preferences] Loading BL Preferences...");

		if(!isFile(%backup = "config/server/BLPrefs/prefs.backup")) {
			echo("\c2[Support_Preferences] Backing up preferences file...");
			fileCopy($BLPrefs::File, %backup);
		}

		// load all preferences from file and save them so they aren't deleted if their respective addon is disabled
		%fo = new FileObject();
		%fo.openForRead($BLPrefs::File);
		while(!%fo.isEOF()) {
			%line     = %fo.readLine();
			%variable = collapseEscape(getField(%line, 0));
			%val      = collapseEscape(getField(%line, 1));

			//call ::onLoad
			for(%i = 0; %i < PreferenceGroup.getCount(); %i++) {
				%p = PreferenceGroup.getObject(%i);
				if(%p.variable $= %variable && !%p._loaded) {
					%p.updateValue = %val;

					setGlobalByName(%variable, %val);

					%p.onLoad(%val);
					//don't break loop, there could be multiple prefs
					// using same variable
					%p._loaded = true;
				}
			}

			%newVariable = true;

			for(%i = 0; %i < $BLPrefs::PrefCount + 1; %i++) {
				if($BLPrefs::Pref[%i] $= %variable) {
					%newVariable = false;
				}
			}

			if(%newVariable) {
				$BLPrefs::Pref[$BLPrefs::PrefCount++] = %variable;
			}
		}
		%fo.close();
		%fo.delete();
	}

	for(%i = 0; %i < PreferenceGroup.getCount(); %i++) {
		%p = PreferenceGroup.getObject(%i);
		if(%p.className $= "ServerSettingPref")
			continue;

		if(!%p._loaded) {
			echo("\c1Loading default value for " @ %p.variable);
			%p.value = %p.defaultValue;
			echo("\c1  " @ %p.value);
			%p._loaded = true;

			if(%p.variable !$= "") {
				echo(%p.variable @ " set to " @ %p.value);
				setGlobalByName(%p.variable, %p.value);
			}

			%p.onDefault(%p.value);
		}
	}

	//update clients with values

	for(%i = 0; %i < PreferenceGroup.getCount(); %i++) {
		%pref = PreferenceGroup.getObject(%i);

		for(%j = 0; %j < ClientGroup.getCount(); %j++) {
			%client = ClientGroup.getObject(%j);

			if(%client.hasPrefSystem && %client.BLP_isAllowedUse()) {
				commandToClient(%client, 'updateBLPref', %pref.variable, getGlobalByName(%pref));
			}
		}

	}
}


function saveBLPreferences() {
	if(!$BLPrefs::Init || !$BLPrefs::serverLoadedPrefs) {
		return;
	}

	if($BLPrefs::Pref[0] $= "") {
		return;
	}

	// echo("\c5[Support_Preferences] Saving BL Preferences...");

	%fo = new FileObject();
	%fo.openForWrite($BLPrefs::File);
	for(%i = 0; %i < $BLPrefs::PrefCount + 1; %i++) {
		%variable = $BLPrefs::Pref[%i];

		if(%variable $= "") {
			continue;
		}

		%fo.writeLine(expandEscape(%variable) TAB expandEscape(getGlobalByName(%variable)));
	}
	%fo.close();
	%fo.delete();

	export("$Pref::Server::*", "config/server/prefs.cs");
}


package BLPrefSaveLoadPackage {
	function onServerCreated() {
		$BLPrefs::serverLoadedPrefs = true;

		parent::onServerCreated();
	}

	function onServerDestroyed() {
		saveBLPreferences();

		$BLPrefs::serverLoadedPrefs = false;
		$BLPrefs::AddedServerSettings = false;
		$BLPrefs::PrefCount = 0;

		PreferenceAddonGroup.deleteAll();
		PreferenceGroup.deleteAll();

		echo("\c2[Support_Preferences] Cleaned Preferences");

		parent::onServerDestroyed();
	}

	function onExit() {
		saveBLPreferences();

		parent::onExit();
	}

  function postServerTCPObj::connect(%this, %addr) {
    parent::connect(%this, %addr);
		loadBLPreferences();
	}

	function deactivateServerPackages() {
		parent::deactivateServerPackages();
		activatePackage(BLPrefSaveLoadPackage);
	}
};
activatePackage(BLPrefSaveLoadPackage);
