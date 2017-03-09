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
				if(%p.variable $= %variable) {
					%p.updateValue = %val;
					%p.onLoad(%val);
					//don't break loop, there could be multiple prefs
					// using same variable
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
		$BLPrefs::PrefCount = 0;
		PreferenceAddonGroup.deleteAll();
		PreferenceGroup.deleteAll();

		parent::onServerDestroyed();
	}

	function onExit() {
		saveBLPreferences();

		parent::onExit();
	}
};
activatePackage(BLPrefSaveLoadPackage);
