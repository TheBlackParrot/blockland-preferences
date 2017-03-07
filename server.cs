//	---=== Blockland Preference System ===---
//	-- Contributors:
//		-- TheBlackParrot (BL_ID 18701)
//		-- Jincux (BL_ID 9789)
//		-- Chrisbot6 (BL_ID 12233)
//		-- Paperwork (BL_ID 636)

// valid pref types:
// - playercount [min] [max] (RTB's convenience list type) #
// - wordlist [delim] [max]
// - datablocklist [type] [delim] [max]
// - userlist [delim] [max]
// - datablock [type] [hasNoneOption] #
// - slider [min] [max] [snapTo] [stepValue] #
// - num [min] [max] [decimalpoints] #
// - bool #
// - button #
// - dropdown [item1Name] [item1Var] [item2Name] [item2Var] etc #
// - string [charLimit] [stripML] #
// - colorset #
// - rgb [form] # form = hex, integer

if($BLPrefs::didLoad && !$BLPrefs::Debug && $BLPrefs::Init) {
	prunePrefs();
	return;
} else if(!$BLPrefs::PreLoad) {
	echo("\c2[Support_Preferences] Preloader NOT installed. Some prefs may not be available!");
} else if($BLPrefs::Debug) {
	echo("\c4[Support_Preferences] Re-executing, development mode!");
}

if($Pref::PreLoadScriptLauncherVersion < 1) {
	fileCopy("./support/preloader.cs", "config/main.cs");
}

if(!isObject(PreferenceGroup)) {
	new SimSet(PreferenceGroup);
}

if(!isObject(PreferenceGroups)) {
	new SimSet(PreferenceGroups);
}

$Pref::BLPrefs::ServerDebug = true;
$Pref::BLPrefs::iconDefault = "wrench";
$BLPrefs::Version = "2.0.0-alpha.0+indev";
$BLPrefs::File = "config/server/BLPrefs/prefs.cs";

if($Pref::BLPrefs::AllowedRank $= "") {
	$Pref::BLPrefs::AllowedRank = "2";
}

if(!$BLPrefs::Init) {
	$BLPrefs::PrefCount = -1;
	$BLPrefs::PrefGroups = "";
}

exec("./class/preference.cs");
exec("./class/preferenceGroup.cs");

exec("./support/admin.cs");
exec("./support/lesseval.cs");

exec("./server/functions.cs");
exec("./server/compatibility.cs");
exec("./server/handshake.cs");
exec("./server/interaction.cs");
exec("./persistence.cs");
exec("./server/userdata.cs");

function prunePrefs() {
	%groups = "";
	%pruned = 0;

	for(%i = 0; %i < getWordCount($BLPrefs::PrefGroups); %i++) {
		%group = getWord($BLPrefs::PrefGroups, %i);

		if(isObject(%group)) {
			if(getGlobalByName("$AddOn__" @ %group.file) == -1) {
				%group.delete();
				%pruned++;
			} else if (getGlobalByName("$AddOn__" @ %group.file) == 1) {
				%groups = trim(%groups SPC %group);
			}
		}
	}

	if(%pruned > 0) {
		echo("\c4[Support_Preferences] Pruned " @ %pruned @ " disabled add-ons' preferences.");
	} else {
		echo("\c4[Support_Preferences] No preferences to prune.");
	}

	$BLPrefs::PrefGroups = %groups;
}

function registerPref(%addon, %category, %title, %type, %variable, %filename, %default, %params, %callback, %legacy, %isSecret, %isHostOnly)
{
	warn("Depreciated Preference registration");
	// %leagacy = 1 if it's added via a compatibility wrapper
	if(%category $= "") {
		%category = "General";
	}

	if(%legacy) {
		%group.icon = "bricks";
	}

	//for(%i=0;%i<%group.getCount();%i++) {
	//	if(%variable $= %group.getObject(%i).variable) {
	//		// echo("\c2[Support_Preferences] " @ %variable @ " has already been registered, skipping...");
	//		return;
	//	}
	//}

	%pref = new ScriptObject(Preference) {

		addon = %addon;
		category = %category;
		title = %title;

		type = %type;
		params = %params;

		variable = %variable;
		defaultValue = %default;

		updateCallback = %callback;
		loadCallback = %callback;
		defaultCallback = %callback;

		hostOnly = %isHostOnly;
		secret = %isSecret;

		legacy = %legacy;
	};

	return %pref;
}

function BlocklandPrefSO::findByVariable(%var) { // there's gotta be a better way to do this
	for(%i = 0; %i < PreferenceContainerGroup.getCount(); %i++) {
		%group = PreferenceContainerGroup.getObject(%i);

		for(%j = 0; %j < %group.getCount(); %j++) {
			%pso = %group.getObject(%j);
			if(%pso.variable $= %var) {
				return %pso;
			}
		}
	}

	return false;
}

// a wrapper to execute everything in the prefs folder
// will be used for older addons without prefs, if asked for them
if(!$BLPrefs::AddedServerSettings) {
	%file = findFirstFile("./server/prefs/*.cs");

	while(%file !$= "")	{
		exec(%file);
		%file = findNextFile("./server/prefs/*.cs");
	}
}


loadBLPreferences();

$BLPrefs::Init = true;
$BLPrefs::didLoad = true;
