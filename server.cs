// ---=== Blockland Preference System ===---
// -- Contributors:
//    -- TheBlackParrot (BL_ID 18701)
//    -- Jincux (BL_ID 9789)

if($BLPrefs::didPreload && !$BLPrefs::Debug) {
	echo("Preferences already preloaded, nothing to do here.");
	return;
} else if(!$BLPrefs::PreLoad) {
	warn("Preloader wasn't installed. Some prefs may not be available.");
} else if($BLPrefs::Debug) {
	warn("Re-executing, development mode");
}

if(!isObject(PreferenceContainerGroup)) {
	new ScriptGroup(PreferenceContainerGroup);
}

$Pref::BLPrefs::ServerDebug = true;
$BLPrefs::Version = "0.0-dev";

exec("./functions.cs");
exec("./compatibility.cs");
exec("./handshake.cs");
exec("./interaction.cs");

if($Pref::PreLoadScriptLauncherVersion < 1) {
	fileCopy("./preloader.cs", "config/main.cs");
}

function registerBlocklandPref(%addon, %title, %type, %variable, %default, %params, %callback, %icon, %legacy)
{
	// using famfamfam's silk icons. use an icon filename minus the extension for %icon
	// RTB prefs will use the old RTB icon by default

	// the server will not need them, only the soon-to-be client(s) will
	// if there's a way to send icons to clients without clients having it, by all means, please add that

	// %leagacy = 1 if it's added via a compatibility wrapper

	// shorthand types
	switch$(%type) {
		case "bool" or "tf":
			%type = "boolean";

		case "num" or "int" or "float" or "real":
			%type = "number";

		case "str":
			%type = "string";

		case "slide" or "range":
			%type = "slider";

		case "choice" or "choices":
			%type = "list";
	}

	%valid = ":number:string:slider:boolean:list:password";
	// possible future entries?: color (hex, rgb, set (via params)), time
	if(stripos(%valid, ":" @ %type) == -1)
	{
		warn("Invalid pref type:" SPC %type);
		return;
	}

	%groupName = BLP_alNum(%addon) @ "Prefs";
	if(!isObject(%groupName)) {
		%group = new ScriptGroup(BlocklandPrefGroup) {
			class = "PreferenceGroup";
			title = BLP_alNum(%addon);
			legacy = %legacy;
			category = %addon;
			icon = %icon;
		};
	} else {
		%group = (%groupName).getID();
	}

	for(%i=0;%i<%group.getCount();%i++) {
		if(%variable $= %group.getObject(%i).variable) {
			echo("\c4" @ %variable SPC "has already been registered, skipping...");
			return;
		}
	}

	%pref = new scriptObject(BlocklandPrefSO)
	{
		class = "Preference";
		category = %addon;
		title = %title;
		defaultValue = %default;
		variable = %variable;
		type = %type;
		callback = %callback;
		params = %params;
		icon = %icon;
		legacy = %legacy;
		announce = true;
	};
	%group.add(%pref);

	// use this for server-sided validation?
	switch$(%type)
	{
		case "number":
			%pref.minValue = getField(%params, 0);
			%pref.maxValue = getField(%params, 1);
			%pref.decimalPoints = getField(%params, 2);

			if(%pref.defaultValue < %pref.minValue)
			{
				%pref.defaultValue = %pref.minValue;
			}
			else if(%pref.defaultValue > %pref.maxValue)
			{
				%pref.defaultValue = %pref.maxValue;
			}

		case "string" or "password":
			%pref.maxLength = getField(%params, 0);
			%pref.stripML = getField(%params, 1);

			if(strlen(%pref.defaultValue) > %pref.maxLength)
			{
				%pref.defaultValue = getSubStr(%pref.defaultValue, 0, %pref.maxLength);
			}

		case "slider":
			%pref.minValue = getField(%params, 0);
			%pref.maxValue = getField(%params, 1);
			%pref.snapTo = getField(%params, 2);
			%pref.stepValue = getField(%params, 3);

			if(%pref.defaultValue < %pref.minValue)
			{
				%pref.defaultValue = %pref.minValue;
			}
			else if(%pref.defaultValue > %pref.maxValue)
			{
				%pref.defaultValue = %pref.maxValue;
			}

		case "boolean":
			if(%pref.defaultValue > 1)
				%pref.defaultValue = 1;

			if(%pref.defaultValue < 0)
				%pref.defaultValue = 0;

		case "list":
			// TS needs an explode function ffs
			%count = 0;
			while(strLen(%params)) {
				%pos = stripos(%params, "|");
				%row = getSubStr(%params, 0, %pos);

				%pref.rowName[%count] = getSubStr(%row, 0, stripos(%row, "**"));
				%pref.rowValue[%count] = getSubStr(%row, stripos(%row, "**")+2, strLen(%row));

				%count++;

				if(stripos(%params, "|") != -1) {
					%params = getSubStr(%params, stripos(%params, "|")+1, strLen(%params));
				} else {
					%params = "";
				}
			}
			%pref.listCount = %count;
			// "Host**4|Super Admin**3|Admin**2|All**1"

			// "**" denotes the separation of the visible part and the value of the variable
			// "|" denotes a new choice
	}

	return %pref;
}

function BlocklandPrefSO::onAdd(%obj)
{
	%obj.setName("");
}

function BlocklandPrefSO::getValue(%this) {
	return eval("return " @ %this.variable @ ";");
}

function BlocklandPrefSO::updateValue(%this, %value, %updater) {
	// we need some way to validate the values on this end of things
	//%updater - client that updated value.
	if(isObject(%updater)) {
		%updaterClean = %updater.getId();
	} else {
		%updaterClean = 0;
	}

	eval(%this.variable @ " = \"" @ expandEscape(%value) @ "\";");

	if(strpos(%this.callback, ";") != -1) {//callback is a full expression
		eval(%this.callback);
	} else { // callback(value, client, pref object);
		eval(%this.callback @ "(\"" @ expandEscape(%value) @ "\", " @ %updaterClean @ ", " @ %this.getId() @ ");");
	}
}

function BlocklandPrefSO::validateValue(%this, %value) {
	// this is where the SO's come in handy
	switch$(%this.type) {
		case "number":
			%value = mFloatLength(%value, %this.decimalPoints);
			if(%value < %this.minValue) {
				%value = %this.minValue;
			}
			else if(%value > %this.maxValue){
				%value = %this.maxValue;
			}

		case "string" or "password":
			if(strlen(%value) > %this.maxLength) {
				%value = getSubStr(%value, 0, %this.maxLength);
			}
			if(%this.stripML) {
				%value = stripMLControlChars(%value);
			}

		case "slider":
			if(%value < %this.minValue) {
				%value = %this.minValue;
			}
			else if(%value > %this.maxValue){
				%value = %this.maxValue;
			}

			%value -= (%value % %this.snapTo);

		case "boolean":
			if(%value > 1)
				%value = 1;

			if(%value < 0)
				%value = 0;

	}
	return %value;
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

function BlocklandPrefGroup::onAdd(%this) {
	%this.setName(%this.title @ "Prefs");
	PreferenceContainerGroup.add(%this);
}

// add a wrapper to execute everything in the prefs folder
// will be used for older addons without prefs, if asked for them
if(!$BLPrefs::AddedServerSettings) {
	exec("./prefs/general.cs");
}

$BLPrefs::Init = true;
