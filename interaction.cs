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

	%found = "";
	for(%i=0;%i<%group.getCount();%i++) {
		%row = %group.getObject(%i);
		if(stripos(%found, ":" @ %row.category) == -1) {
			if($Pref::BLPrefs::ServerDebug) {
				echo("\c4Sending" SPC %row.category @ "...");
			}
			%found = ":" @ %row.category @ %found;
			commandToClient(%client, 'addCategory', %row.category, %row.icon);
		}
	}
}

function serverCmdGetBLPrefCategory(%client, %category) {
	%group = PreferenceContainerGroup;
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
		if(%row.category $= %category) {
			if(!%first) {
				%first = true;
				commandToClient(%client, 'receivePref', %row.title, %row.type, %row.variable, eval("return" SPC %row.variable @ ";"), %row.params, %row.legacy);
			} else {
				commandToClient(%client, 'receivePref', %row.title, %row.type, %row.variable, eval("return" SPC %row.variable @ ";"), %row.params);
			}
		}
	}
	commandToClient(%client, 'finishReceivePref');
}
//clientCmdAddPref(%addon, %title, %type, %variable, %value, %params, %icon)
//commandToClient(%client, 'addPref', %row.category, %row.title, %row.type, %row.variable, eval("return" SPC %row.variable @ ";"), %row.params, %row.icon);