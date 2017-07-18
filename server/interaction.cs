
// (client) -> requestPrefCategories
// (server) -> receiveCategory
//          -> receiveCategory
//          -> receiveCategory
//          -> ...


function serverCmdRequestPrefCategories(%client) {
	%group = PreferenceAddonGroup;
	if(!isObject(%group)) {
		echo("\c2[Support_Preferences] " @ %client.getPlayerName() SPC "requested preferences, but the container group doesn't exist. This shouldn't be happening.");
		return;
	}

	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	for(%i = 0; %i < %group.getCount(); %i++) {
		%row = %group.getObject(%i);
		if($Pref::BLPrefs::ServerDebug) {
			echo("\c5[Support_Preferences] Sending" SPC %row.title SPC "to" SPC %client.getPlayerName() @ " (BL_ID: " @ %client.getBLID() @ ")...");
		}
		commandToClient(%client, 'ReceiveCategory', %i, %row.title, %row.icon, (%group.getCount()-1 == %i));
	}
}

function serverCmdRequestCategoryPrefs(%client, %catId, %failsafe) {
	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	%group = PreferenceAddonGroup.getObject(%catId);

	if(%failsafe >= 2)
		return; // this should never happen

	if(!isObject(%group)) {
		echo("\c4[Support_Preferences] " @ %client.getPlayerName() SPC "requested preferences, but the container group doesn't exist. This could be happening due to invalid requests or client bugs.");
		serverCmdRequestCategoryPrefs(%client, 0, %failsafe+1);
		return;
	}

	for(%i = 0; %i < %group.getCount(); %i++) {
		%row = %group.getObject(%i);
		commandToClient(%client, 'ReceivePref', %catId, %row.id, %row.title, %row.category, %row.type, %row.params, %row.defaultValue, %row.variable, %row.getValue(), (%group.getCount()-1 == %i), %row.duplicate);
	}
}

//this should be using id's too
function serverCmdUpdatePref(%client, %id, %newValue, %announce) {
	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	//backwards compatibility
	if(%client.legacyPrefs) {
		%pso = Preference::findByVariable(%id);
	} else {
		%pso = PreferenceGroup.getObject(%id);
	}

	if(%pso) {
		if(getSimTime() - %client.lastChangedCat[%pso.addon] >= 100) {
			if(%pso.addon $= -1) {
				messageAll('MsgAdminForce', "\c3" @ %client.name SPC "\c6updated the \c3Server Settings\c6.");
			} else {
				messageAll('MsgAdminForce', "\c3" @ %client.name SPC "\c6updated the \c3" @ %pso.getGroup().title @ "\c6 prefs.");
			}
		}

		%client.lastChangedCat[%pso.addon] = getSimTime();

		if(%pso.hostOnly) {
			if(%client.getBLID() != getNumKeyId() && %client.getBLID() != 999999) {
				return;
			}
		}

		//update value now validates it
		%pso.updateValue(%newValue, %client);
		%newValue = %pso.getValue();

		if($Pref::BLPrefs::ServerDebug) {
			echo("\c4[Support_Preferences] " @ %client.name @ " (BL_ID: " @ %client.getBLID() @ ") set " @ %pso.variable @ " to " @ %newValue);
		}

		if(%announce) {
			if(%pso.type $= "dropdown" || %pso.type $= "datablock") {
				%displayValue = %pso.valueName[%newValue];
			} else if(%pso.type $= "checkbox" || %pso.type $= "bool") {
				%displayValue = (%newValue ? "true" : "false");
			} else {
				%displayValue = expandEscape(%newValue);
			}

			if(!%pso.secret)
				messageAll('', "\c6 + \c3" @ %pso.title SPC "\c6is now\c3" SPC %displayValue);
			else
				messageAll('', "\c6 + \c3" @ %pso.title SPC "\c6was changed.");
		}

		//for(%i = 0; %i < ClientGroup.getCount(); %i++) {
		//	%cl = ClientGroup.getObject(%i);
		//	if(%cl.hasPrefSystem && %cl.BLP_isAllowedUse()) {
		//		//commandToClient(%cl, 'updateBLPref', %varname, %newValue);
		//		commandToClient(%cl, 'updatePref', %pso.id, %newValue);
		//	}
		//}

		saveBLPreferences();
	} else {
		//so they tried to update a variable that doesn't exist...
		warn("Variable \"" @ %varname @ "\" doesn't exist!");
	}
}

function serverCmdOpenPrefs(%cl) {
	if(%cl.BLP_isAllowedUse())
		BLPrefCheckUpdates();
}
