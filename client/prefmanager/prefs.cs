// clientPrefsList
// --------------------------------------
function clientPrefsList() {
	if(!isObject(clientPrefsList))
		new ScriptGroup(clientPrefsList) {
			name = "ClientPrefsList"; 
		};
	
	return clientPrefsList;
}

clientPrefsList(); // do this right away.

function findPref(%id) {
	// get the pref object for this setting
	%l = clientPrefsList();
	for(%i = 0; %i < %l.getCount(); %i++) {
		%pref = %l.getObject(%i);
		
		//echo(%pref.serverId);
		
		if(%pref.serverId == %id) {
			return %pref;
		}
	}
	
	return -1;
}

//function clientPrefsList::onAdd(%this, %pref) {
//	// add pref to gui
//	echo("got " @ %pref.name);
//}

//function clientPrefsList::onRemove(%this, %pref) {
//	// remove pref from gui
//}
