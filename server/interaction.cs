function serverCmdGetBLPrefCategories(%client) {
	%group = PreferenceContainerGroup;
	if(!isObject(%group)) {
		echo("\c4" @ %client.name SPC "requested preferences, but the container group doesn't exist. This shouldn't be happening.");
		return;
	}

	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	// the groups made things SO MUCH SIMPLER
	for(%i=0;%i<%group.getCount();%i++) {
		%row = %group.getObject(%i);
		if($Pref::BLPrefs::ServerDebug) {
			echo("\c4Sending" SPC %row.category @ "...");
		}
		commandToClient(%client, 'addCategory', %row.category, %row.icon);
	}
}

function serverCmdGetBLPrefCategory(%client, %category) {
	//%group = PreferenceContainerGroup;
	%group = (BLP_alNum(%category) @ "Prefs");
	if(!isObject(%group)) {
		echo("\c4" @ %client.name SPC "requested preferences, but the container group doesn't exist. This shouldn't be happening.");
		return;
	}

	if(!%client.BLP_isAllowedUse()) {
		return;
	}

	for(%i=0;%i<%group.getCount();%i++) {
		%row = %group.getObject(%i);
		if(!%first) {
			%first = true;
			commandToClient(%client, 'receivePref', %row.title, %row.type, %row.variable, eval("return" SPC %row.variable @ ";"), %row.params, %row.legacy);
		} else {
			commandToClient(%client, 'receivePref', %row.title, %row.type, %row.variable, eval("return" SPC %row.variable @ ";"), %row.params);
		}
	}
	commandToClient(%client, 'finishReceivePref');
}
//clientCmdAddPref(%addon, %title, %type, %variable, %value, %params, %icon)
//commandToClient(%client, 'addPref', %row.category, %row.title, %row.type, %row.variable, eval("return" SPC %row.variable @ ";"), %row.params, %row.icon);

function serverCmdupdateBLPref(%client, %varname, %newvalue) {
	//validate!
	if(!%client.BLP_isAllowedUse())
		return;

	//we need to find the object
	%pso = BlocklandPrefSO::findByVariable(%varname);
	if(%pso) {
		if(%pso.hostOnly) {
			if(%client.bl_id != getNumKeyId())
				return;
		}
		
		%newvalue = %pso.validateValue(%newvalue);
		%pso.updateValue(%newvalue, %client);

		if($Pref::BLPrefs::ServerDebug) {
			echo("\c4" @ %client.netname @ " set " @ %varname @ " to " @ %newvalue);
		}

		if($Pref::BLPrefs::AnnounceChanges) {
			if(%pso.type $= "list" || %pso.type $= "datablock") {
				%displayValue = %pso.valueName[%newvalue];
			} else {
				%displayValue = expandEscape(%newvalue);
			}
			
			if(!%pso.isSecret)
				messageAll('MsgAdminForce', "\c6 + \c3" @ %client.netname SPC "\c6set\c3" SPC %pso.title SPC "\c6to\c3" SPC %displayValue);
			else
				messageAll('MsgAdminForce', "\c6 + \c3" @ %client.netname SPC "\c6set\c3" SPC %pso.title);
		}

		for(%i = 0; %i < ClientGroup.getCount(); %i++) {
			%cl = ClientGroup.getObject(%i);
			if(%cl.hasPrefSystem && %cl.isAdmin) {
				commandToClient(%cl, 'updateBLPref', %varname, %newvalue);
			}
		}
	} else {
		//so they tried to update a variable that doesn't exist...
		warn("Variable \"" @ %varname @ "\" doesn't exist!");
	}
}
