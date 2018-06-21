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

$BLPrefs::Location = "Add-Ons/System_BlocklandGlass/resources/Support_Preferences";

if($BLPrefs::didLoad && !$BLPrefs::Debug && $BLPrefs::Init) {
	if(!$Server::Dedicated) {
		echo("[Support_Preferences] Preferences Reloading");
		activatePackage(BLPrefCompatibilityPackage);
		prunePrefs();
	}
	return;
} else if(!$BLPrefs::PreLoad) {
	echo("\c2[Support_Preferences] Preloader NOT installed. Some prefs may not be available!");
} else {
	//echo("\c4[Support_Preferences] Loading...");
}

//filecopy doesnt like zips
function filecopy_hack(%source, %destination) {
  %fo_source = new FileObject();
  %fo_dest = new FileObject();
  %fo_source.openForRead(%source);
  %fo_dest.openForWrite(%destination);
  while(!%fo_source.isEOF()) {
    %fo_dest.writeLine(%fo_source.readLine());
  }
  %fo_source.close();
  %fo_dest.close();
  %fo_source.delete();
  %fo_dest.delete();
}

if($Pref::PreLoadScriptLauncherVersion < 1) {
	fileDelete("config/main.cs");
	fileCopy_hack($BLPrefs::Location @ "/support/preloader.cs", "config/main.cs");
	$PreLoaderInstalled = true;
}

if(!isObject(PreferenceGroup)) {
	new SimSet(PreferenceGroup);
}

if(!isObject(PreferenceAddonGroup)) {
	new SimSet(PreferenceAddonGroup);
}

$Pref::BLPrefs::ServerDebug = false;
$Pref::BLPrefs::iconDefault = "wrench";
$BLPrefs::Version = "2.0.4";
$BLPrefs::File = "config/server/BLPrefs/prefs.cs";

if($Pref::BLPrefs::AllowedRank $= "") {
	$Pref::BLPrefs::AllowedRank = "2";
}

if(!$BLPrefs::Init) {
	$BLPrefs::PrefCount = -1;
	$BLPrefs::PrefGroups = "";
}

exec("./class/preference.cs");
exec("./class/preferenceAddon.cs");

exec("./support/admin.cs");
exec("./support/lesseval.cs");

exec("./server/functions.cs");
exec("./server/compatibility.cs");
exec("./server/handshake.cs");
exec("./server/interaction.cs");
exec("./server/persistence.cs");
exec("./server/userdata.cs");

exec("./rtb/puppet.cs");

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
	warn("Depreciated Preference Registration");
	// %leagacy = 1 if it's added via a compatibility wrapper
	if(%category $= "") {
		%category = "General";
	}

	if(%legacy) {
		%group.icon = "bricks";
	}

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

// a wrapper to execute everything in the prefs folder
// will be used for older addons without prefs, if asked for them
if(!$BLPrefs::AddedServerSettings) {
	%file = findFirstFile("./server/prefs/*.cs");

	while(%file !$= "")	{
		exec(%file);
		%file = findNextFile("./server/prefs/*.cs");
	}
	//exec("./server/prefs/general.cs");
}

loadBLPreferences();
registerServerSettingPrefs();

$BLPrefs::Init = true;
$BLPrefs::didLoad = true;
