
// (client) -> requestPrefCategories
// (server) -> receiveCategory
//          -> receiveCategory
//          -> receiveCategory
//          -> ...


function serverCmdRequestPrefCategories(%client) {
	%group = PreferenceGroups;
	if(!isObject(%group)) {
		echo("\c2[Support_Preferences] " @ %client.getPlayerName() SPC "requested preferences, but the container group doesn't exist. This shouldn't be happening.");
		return;
	}

	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	// the groups made things SO MUCH SIMPLER
	for(%i=0;%i<%group.getCount();%i++) {
		%row = %group.getObject(%i);
		if($Pref::BLPrefs::ServerDebug) {
			echo("\c5[Support_Preferences] Sending" SPC %row.title SPC "to" SPC %client.getPlayerName() @ " (BL_ID: " @ %client.getBLID() @ ")...");
		}
		commandToClient(%client, 'ReceiveCategory', %i, %row.title, %row.icon, (%group.getCount()-1 == %i));
	}
}

function serverCmdRequestCategoryPrefs(%client, %anID, %failsafe) {
	%group = PreferenceGroups.getObject(%anID);

	if(%failsafe >= 2)
		return; // this should never happen

	if(!isObject(%group)) {
		echo("\c4[Support_Preferences] " @ %client.getPlayerName() SPC "requested preferences, but the container group doesn't exist. This could be happening due to invalid requests or client bugs.");
		serverCmdRequestCategoryPrefs(%client, 0, %failsafe+1);
		return;
	}

	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	// the groups made things SO MUCH SIMPLER
	for(%i=0;%i<%group.getCount();%i++) {
		%row = %group.getObject(%i);
		commandToClient(%client, 'ReceivePref', %anID, %i, %row.title, %row.category, %row.type, %row.params, %row.defaultValue, %row.variable, %row.getValue(), (%group.getCount()-1 == %i), %row.duplicate);
	}
}

function serverCmdUpdatePref(%client, %varname, %newvalue) {
	//validate!
	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	//we need to find the object
	%pso = Preference::findByVariable(%varname);
	if(%pso) {
		if(getSimTime() - %client.lastChangedCat[%pso.category] >= 100) {
			messageAll('MsgAdminForce', "\c3" @ %client.name SPC "\c6updated the \c3" @ %pso.category @ "\c6 prefs.");
		}

		%client.lastChangedCat[%pso.category] = getSimTime();

		if(%pso.hostOnly) {
			if(%client.getBLID() != getNumKeyId() && %client.getBLID() != 999999) {
				return;
			}
		}

		%newvalue = %pso.validateValue(%newvalue);
		%pso.updateValue(%newvalue, %client);

		if($Pref::BLPrefs::ServerDebug) {
			echo("\c4[Support_Preferences] " @ %client.name @ " (BL_ID: " @ %client.getBLID() @ ") set " @ %varname @ " to " @ %newvalue);
		}

		if(%announce) {
			if(%pso.type $= "dropdown" || %pso.type $= "datablock") {
				%displayValue = %pso.valueName[%newvalue];
			} else {
				%displayValue = expandEscape(%newvalue);
			}

			if(!%pso.secret)
				messageAll('', "\c6 + \c3" @ %pso.title SPC "\c6is now\c3" SPC %displayValue);
			else
				messageAll('', "\c6 + \c3" @ %pso.title SPC "\c6was changed.");
		}

		for(%i = 0; %i < ClientGroup.getCount(); %i++) {
			%cl = ClientGroup.getObject(%i);
			if(%cl.hasPrefSystem && %cl.BLP_isAllowedUse()) {
				commandToClient(%cl, 'updateBLPref', %varname, %newvalue);
			}
		}

		saveBLPreferences();
	} else {
		//so they tried to update a variable that doesn't exist...
		warn("Variable \"" @ %varname @ "\" doesn't exist!");
	}
}
