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
			echo("\c1[Support_Preferences] Backing up preferences file...");
			fileCopy($BLPrefs::File, %backup);
		}


		echo("\c1[Support_Preferences] Loading preferences...");

		// load all preferences from file and save them so they aren't deleted if their respective addon is disabled
		%fo = new FileObject();
		%fo.openForRead($BLPrefs::File);
		while(!%fo.isEOF()) {
			%line     = %fo.readLine();
			%variable = collapseEscape(getField(%line, 0));
			%val      = collapseEscape(getField(%line, 1));

			setGlobalByName(%variable, %val);

			%newVariable = true;

			for(%i = 0; %i < $BLPrefs::PrefCount; %i++) {
				if($BLPrefs::Pref[%i] $= %variable) {
					%newVariable = false;
				}
			}

			if(%newVariable) {
				%id = $BLPrefs::PrefCount;
				$BLPrefs::PrefValue[%id] = %val;
				$BLPrefs::Pref[%id]      = %variable;
				$BLPrefs::PrefCount++;
			}
		}
		%fo.close();
		%fo.delete();
	} else {
		echo("\c1[Support_Preferences] Defaulting Preferences");
	}
	BLPrefUpdateTick();
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
	for(%i = 0; %i < $BLPrefs::PrefCount; %i++) {
		%variable = $BLPrefs::Pref[%i];

		if(%variable $= "") {
			continue;
		}

		%fo.writeLine(expandEscape(%variable) TAB expandEscape(getGlobalByName(%variable)));
	}
	%fo.close();
	%fo.delete();

	export("$Pref::Server::*", "config/server/prefs.cs");

	if(!isFile($BLPrefs::File)) {
		echo("\c2[Support_Preferences] Failed to save preferences!");
	}
}

function BLPrefUpdateTick() {
	cancel($BLPrefs::UpdateTick);

	BLPrefCheckUpdates();

	$BLPrefs::UpdateTick = schedule(1000, PreferenceGroup, BLPrefUpdateTick);
}

function BLPrefCheckUpdates() {
	for(%i = 0; %i < PreferenceGroup.getCount(); %i++) {
		%p = PreferenceGroup.getObject(%i);
		if(%p.variable !$= "" && %p._loaded) {
			%val = getGlobalByName(%p.variable);
			if(%val !$= %p.value) {
				echo("\c1[Support_Preferences] Detected change in " @ %p.variable);

				%p.updateValue(%val);

				for(%j = 0; %j < ClientGroup.getCount(); %j++) {
					%client = ClientGroup.getObject(%j);

					if(%client.hasPrefSystem && %client.BLP_isAllowedUse()) {
						commandToClient(%client, 'updateBLPref', %p.variable, %p.value);
					}
				}
			}
		}
	}
}

package BLPrefSaveLoadPackage {
	function onServerCreated() {
		$BLPrefs::serverLoadedPrefs = true;
		loadBLPreferences();

		parent::onServerCreated();
	}

	function onServerDestroyed() {
		saveBLPreferences();

		$BLPrefs::serverLoadedPrefs = false;
		$BLPrefs::AddedServerSettings = false;
		$BLPrefs::PrefCount = 0;

		PreferenceAddonGroup.deleteAll();
		PreferenceGroup.deleteAll();

		echo("\c1[Support_Preferences] Cleaned Preferences");

		parent::onServerDestroyed();
	}

	function onExit() {
		saveBLPreferences();

		parent::onExit();
	}

  function postServerTCPObj::connect(%this, %addr) {
    parent::connect(%this, %addr);
	}

	function deactivateServerPackages() {
		parent::deactivateServerPackages();
		activatePackage(BLPrefSaveLoadPackage);
	}
};
activatePackage(BLPrefSaveLoadPackage);
