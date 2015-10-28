function serverCmdGetBLPrefs(%client) {
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
		commandToClient(%client, 'addPref', %row.category, %row.title, %row.type, %row.variable, eval("return" SPC %row.variable @ ";"), %row.params, %row.icon);
	}
}
//clientCmdAddPref(%addon, %title, %type, %variable, %value, %params, %icon)