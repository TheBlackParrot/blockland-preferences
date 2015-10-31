function serverCmdGetBLPrefCategories(%client) {
	%group = PreferenceContainerGroup;
	if(!isObject(%group)) {
		echo("\c4" @ %client.name SPC "requested preferences, but the container group doesn't exist. This shouldn't be happening.");
		return;
	}

	if(!%client.isSuperAdmin) {
		// for now
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

	if(!%client.isSuperAdmin) {
		// for now
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

	//we need to find the object
	%pso = BlocklandPrefSO::findByVariable(%varname);
	if(%pso) {
		%pso.updateValue(%newvalue, %client);

		if($Pref::BLPrefs::ServerDebug) {
			echo("\c4" @ %client.netname @ " set " @ %varname @ " to " @ %newvalue);
		}
	} else {
		//so they tried to update a variable that doesn't exist...
		warn("Variable \"" @ %varname @ "\" doesn't exist!");
	}
}
