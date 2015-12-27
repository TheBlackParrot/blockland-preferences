// ---=== Blockland Preference System ===---
// -- Contributors:
//    -- TheBlackParrot (BL_ID 18701)
//    -- Jincux (BL_ID 9789)
// 	  -- Chrisbot6 (BL_ID 12233)

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

// Chrisbot6's todo list
// - registerBlocklandPref should just be registerPref.
// - values should be ordered differently
// - %addon is ambiguous, rename to %category?
// - %icon should allow the user to use icons from anywhere
// - %type should probably contain all type info, not just a type name - is %perams needed? Not that I dislike it, but it would make legacy easier.
// - %legacy shoudn't be needed
// - List delimiters should be torque standard
// - Do we need shorthand?
// - Server_Prefs value types pls
// - Merge the features of Server_Prefs into this
// - %icon should be per category??

// value types I want to add:
// - RTB's playercount type
// - My wordlist, datablocklist and userlist types
// - RTB's datablock type
// - event system's color/vector type (color/colour/vec can be shorthand?)
// - or maybe we could use the colorset for color like slayer does? That would be interesting.
// - your time type?

function registerBlocklandPref(%addon, %title, %type, %variable, %default, %params, %callback, %icon, %legacy)
{
	// using famfamfam's silk icons. use an icon filename minus the extension for %icon
	// RTB prefs will use the old RTB icon by default

	// the server will not need them, only the soon-to-be client(s) will
	// if there's a way to send icons to clients without clients having it, by all means, please add that
	
	// suggestion from Chris: Server_Prefs has a "wrench" icon for prefs that shows if the client can't find the specified icon.
	// use that same idea?

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
			%pref.minValue = getWord(%params, 0);
			%pref.maxValue = getWord(%params, 1);
			%pref.decimalPoints = getWord(%params, 2);

			if(%pref.defaultValue < %pref.minValue)
			{
				%pref.defaultValue = %pref.minValue;
			}
			else if(%pref.defaultValue > %pref.maxValue)
			{
				%pref.defaultValue = %pref.maxValue;
			}

		case "string" or "password":
			%pref.maxLength = getWord(%params, 0);
			%pref.stripML = getWord(%params, 1);

			if(strlen(%pref.defaultValue) > %pref.maxLength)
			{
				%pref.defaultValue = getSubStr(%pref.defaultValue, 0, %pref.maxLength);
			}

		case "slider":
			%pref.minValue = getWord(%params, 0);
			%pref.maxValue = getWord(%params, 1);
			%pref.snapTo = getWord(%params, 2);
			%pref.stepValue = getWord(%params, 3);

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
				%pref.valueName[%pref.rowValue[%count]] = %pref.rowName[%count];

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
	return getGlobalByName(%this.variable); //eval("return " @ %this.variable @ ";");
}

function BlocklandPrefSO::updateValue(%this, %value, %updater) {
	// we need some way to validate the values on this end of things
	//%updater - client that updated value.
	if(isObject(%updater)) {
		%updaterClean = %updater.getId();
	} else {
		%updaterClean = 0;
	}

	setGlobalByName(%this.variable, %value); //eval(%this.variable @ " = \"" @ expandEscape(%value) @ "\";");

	// the code below bothers me.
	// 1. Why let people put in full expressions for their callbacks? This isn't what a callback is. A callback should have a standardized format and should find what it needs when it runs.
	//    You're giving the user the same pointless burden of responsibility as guiCtrl commands do. Just assume the callback's a function name in a string and nothing more.
	// 2. Why are you using eval here? Just use call(functionName, [args]..), it's a lot easier and you don't have to compile or expand escapes.
	// 3. You don't need to use getID to pass an object to a function. An object reference is essentially an object id, and vice versa. Try putting "hammerItem" in a string and calling .getName()
	//    or .getId();. Torque looks for them.
	
	// I've made a change suggestion here but I'm not sure what you'll think.
	
	// - Chrisbot6
	
	if(%this.callback !$= "") {
		// callback(value, client, pref object);
		// callbacks can now only be function names and always get called with the same value set
		call(%this.callback, %value, %updaterClean, %this);
	}
}

function BlocklandPrefSO::validateValue(%this, %value) {
	// this is where the SO's come in handy
	switch$(%this.type) {
		case "number":
			if(%this.decimalPoints !$= "") {
				%value = mFloatLength(%value, %this.decimalPoints);
			}

			if(%value < %this.minValue) {
				%value = %this.minValue;
			}
			else if(%value > %this.maxValue){
				%value = %this.maxValue;
			}

		case "string" or "password": // why is password a unique type rather than a property of the pref object? What if I have a number or datablock I want to keep secret?
			if(strlen(%value) > %this.maxLength) {
				%value = getSubStr(%value, 0, %this.maxLength);
			}
			if(%this.stripML) {
				%value = stripMLControlChars(%value); // sure we couldn't have a MLString type?
			}

		case "float": // a slider is a float and is called "float" in every other pref system. I've changed it here as proposal.
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
	exec("./prefs/blprefs.cs");
}

$BLPrefs::Init = true;
