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
	if(!$BLPrefs::LoadedDefault) {
		echo("\c5[Support_Preferences] Loading Blockland Prefs...");
		exec("config/server/prefs.cs");
		$BLPrefs::LoadedDefault = true;

		fileCopy("config/server/prefs.cs", "config/server/prefs.cs.backup");
	}

	if(isFile($BLPrefs::File)) {

		if(!isFile(%backup = ($BLPrefs::File @ ".backup"))) {
			echo("\c1[Support_Preferences] Backing up add-on preferences file...");

			for(%i = 5; %i > 0 ; %i--) {
				if(isFile(%backup @ "." @ %i)) {
					if(%i == 5) {
						fileDelete(%backup @ ".5");
					} else {
						fileCopy(%backup @ "." @ %i, %backup @ "." @ (%i+1));
					}
				}
			}

			fileCopy($BLPrefs::File, %backup);
		}

		echo("\c5[Support_Preferences] Loading Add-On Preferences...");

		// load all preferences from file and save them so they aren't deleted if their respective addon is disabled
		%fo = new FileObject();
		%fo.openForRead($BLPrefs::File);
		while(!%fo.isEOF()) {
			%line     = %fo.readLine();
			%variable = collapseEscape(getField(%line, 0));
			%val      = collapseEscape(getField(%line, 1));

			%valid = setGlobalByName(%variable, %val);

			if(!%valid)
				continue;

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
	$BLPrefs::serverLoadedPrefs = true;
}


function saveBLPreferences() {
	if(!$BLPrefs::Init || !$BLPrefs::serverLoadedPrefs) {
		return;
	}

	if($BLPrefs::Pref[0] $= "") {
		return;
	}

	echo("\c5[Support_Preferences] Saving BL Preferences...");

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

	if($BLPrefs::LoadedDefault)
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
		loadBLPreferences();

		parent::onServerCreated();
	}

	function onServerDestroyed() {
		saveBLPreferences();

		if(!$Server::Dedicated) {
			$BLPrefs::serverLoadedPrefs = false;
			$BLPrefs::AddedServerSettings = false;
			$BLPrefs::PrefCount = 0;

			PreferenceAddonGroup.deleteAll();
			PreferenceGroup.deleteAll();

			echo("\c1[Support_Preferences] Cleaned Preferences");
		}

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
