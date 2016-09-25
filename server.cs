// ---=== Blockland Preference System ===---
// -- Contributors:
//    -- TheBlackParrot (BL_ID 18701)
//    -- Jincux (BL_ID 9789)
//    -- Chrisbot6 (BL_ID 12233)
//    -- Paperwork (BL_ID 636)

if($BLPrefs::didPreload && !$BLPrefs::Debug && $BLPrefs::Init) {
  prunePrefs();
	return;
} else if(!$BLPrefs::PreLoad) {
	echo("\c2[Support_Preferences] Preloader NOT installed. Some prefs may not be available!");
} else if($BLPrefs::Debug) {
	echo("\c4[Support_Preferences] Re-executing, development mode!");
}

if(!isObject(PreferenceContainerGroup)) {
	new ScriptGroup(PreferenceContainerGroup);
}

$Pref::BLPrefs::ServerDebug = true;
$Pref::BLPrefs::iconDefault = "wrench";
$BLPrefs::Version = "1.2.0";
$BLPrefs::File = "config/server/BLPrefs/prefs.cs";

if($Pref::BLPrefs::AllowedRank $= "") {
	$Pref::BLPrefs::AllowedRank = "2";
}

if(!$BLPrefs::Init) {
	$BLPrefs::PrefCount = -1;
  $BLPrefs::PrefGroups = "";
}

exec("./support/admin.cs");
exec("./support/lesseval.cs");

exec("./server/functions.cs");
exec("./server/compatibility.cs");
exec("./server/handshake.cs");
exec("./server/interaction.cs");
exec("./server/userdata.cs");

if($Pref::PreLoadScriptLauncherVersion < 1) {
	fileCopy("./support/preloader.cs", "config/main.cs");
}

function prunePrefs() {
  %groups = "";
  %pruned = 0;
  
  for(%i = 0; %i < getWordCount($BLPrefs::PrefGroups); %i++) {
    %group = getWord($BLPrefs::PrefGroups, %i);
    
    if(isObject(%group)) {
      if(getGlobalByName("$AddOn__" @ %group.file) == -1) {
        %group.delete();
        %pruned++;
      } else if (getGlobalByName("$AddOn_" @ %group.file) == 1) {
        %groups = trim(%groups SPC %group);
      }
    }
  }
  
  if(%pruned > 0) {
    echo("\c4[Support_Preferences] Pruning " @ %pruned @ " disabled add-ons' preferences.");
  } else {
    echo("\c4[Support_Preferences] No preferences to prune.");
    return false;
  }
  
  $BLPrefs::PrefGroups = %groups;
  
  return true;
}

function registerPref(%addon, %dev, %title, %type, %variable, %filename, %default, %params, %callback, %legacy, %isSecret, %isHostOnly)
{
	// %leagacy = 1 if it's added via a compatibility wrapper
	if(%dev $= "") {
		%dev = "General";
	}

	// shorthand types
	switch$(%type) {
		case "boolean" or "tf":
			%type = "bool";

		case "number" or "real" or "intiger" or "int":
			%type = "num";

		case "numplayers":
			%type = "playercount";

		case "str" or "mlstring":
			%type = "string";

		case "slide" or "range" or "float":
			%type = "slider";

		case "choice" or "choices" or "list":
			%type = "dropdown";

		case "delimited":
			%type = "wordlist";

		case "users" or "bl_idlist" or "adminlist":
			%type = "userlist";

		case "colour":
			%type = "color";

		case "data":
			%type = "datablock";

		case "datalist" or "delimiteddata":
			%type = "datablocklist";

		case "vec":
			%type = "vector";

		case "call" or "function" or "push" or "callbackButton":
			%type = "button";
	}

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

	%valid = ":playercount:wordlist:datablocklist:userlist:datablock:slider:num:bool:button:dropdown:string:rgb:colorset";

	if(stripos(%valid, ":" @ %type) == -1)
	{
		echo("\c2[Support_Preferences] Invalid pref type:" SPC %type);
		return;
	}

	%groupName = BLP_alNum(%addon) @ "Prefs";
	if(!isObject(%groupName)) {
		%group = new ScriptGroup(BlocklandPrefGroup) {
			class = "PreferenceGroup";
			title = BLP_alNum(%addon);
      file = %file;
			legacy = %legacy;
			category = %addon;
			icon = $Pref::BlPrefs::iconDefault;
		};
    
    $BLPrefs::PrefGroups = trim($BLPrefs::PrefGroups SPC %groupName);
	} else {
		%group = (%groupName).getID();
	}

	if(%legacy)
		%group.icon = "bricks";

	for(%i=0;%i<%group.getCount();%i++) {
		if(%variable $= %group.getObject(%i).variable) {
			echo("\c2[Support_Preferences] " @ %variable SPC "has already been registered, skipping...");
			return;
		}
	}

	%pref = new scriptObject(BlocklandPrefSO)
	{
		class = "Preference";
		category = %addon;
		title = %title;
    file = %file;
		defaultValue = %default;
		variable = %variable;
		type = %type;
		callback = %callback;
		params = %params;
		legacy = %legacy;
		devision = %dev;
		secret = %isSecret;
		hostOnly = %isHostOnly;
	};
	%group.add(%pref);

	// use this for server-sided validation?
	switch$(%type)
	{
		case "num":
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

		case "playercount":
			%pref.minValue = getWord(%params, 0);
			%pref.maxValue = getWord(%params, 1);

			if(%pref.defaultValue < %pref.minValue)
			{
				%pref.defaultValue = %pref.minValue;
			}
			else if(%pref.defaultValue > %pref.maxValue)
			{
				%pref.defaultValue = %pref.maxValue;
			}

			%pref.defaultValue = mFloor(%pref.defaultValue);

		case "string":
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

		case "bool":
			if(%pref.defaultValue > 1)
				%pref.defaultValue = 1;

			if(%pref.defaultValue < 0)
				%pref.defaultValue = 0;

		case "dropdown":
			// using the ol rtb list format
			%count = getWordCount(%params) / 2;

			for(%i = 0; %i < %count; %i++) {
				%first = (%i * 2);

				%pref.rowName[%count] = strreplace(getWord(%params, %first), "_", " ");
				%pref.rowValue[%count] = getWord(%params, %first+1);

				%pref.valueName[%pref.rowValue[%count]] = %pref.rowName[%count];
			}

			%pref.listCount = %count;

		case "datablock":
			%pref.dataType = getWord(%params, 0);
			%pref.canBeNone = getWord(%params, 1);

			if(!isObject(%pref.defaultValue)) {
				%pref.defaultValue = -1;
				%pref.canBeNone = true;
			}
			else {
				if((%pref.defaultValue).getClassName() != %pref.dataType) {
					%pref.defaultValue = -1;
					%pref.canBeNone = true; // actually make it the first possible datablock in future rather than forcing "NONE" option and setting it
				}
			}

			// populate the pref object with all possible data values
			// IMPORTANT: when setting the actual global, MAKE SURE YOU USE THE DATABLOCK NAME. In all other situations, use its ID.
			%count = 0;

			if(%pref.canBeNone) {
				// add "NONE" option
				%pref.rowName[%count] = "NONE";
				%pref.rowValue[%count] = -1;

				%pref.valueName[%pref.rowValue[%count]] = %pref.rowName[%count];

				%count++;
			}

			for(%i = 0; %i < DataBlockGroup.getCount(); %i++) {
				%data = DataBlockGroup.getObject(%i);

				if(%data.getClassName() != %pref.dataType)
					continue;

				%pref.rowName[%count] = %data.uiName !$= "" ? %data.uiName : %data.getName();
				%pref.rowValue[%count] = %data.getId();

				%pref.valueName[%pref.rowValue[%count]] = %pref.rowName[%count];

				%count++;
			}

		case "wordlist":
			%pref.delimiter = strreplace(getWord(%params, 0), "_", " ");
			%pref.maxWords = getWord(%params, 1);

			%prefsAsFields = strreplace(%pref.defaultValue, %pref.delimiter, "" TAB ""); // hacky but it works

			// amend?
			if(getFieldCount(%prefsAsFields) > %pref.maxWords && %pref.maxWords != -1) {
				%prefsAsFields = getFields(%pref.defaultValue, %pref.maxWords);
			}

			%pref.defaultValue = strreplace(%prefsAsFields, "" TAB "", %pref.delimiter);

		case "userlist":
			%pref.delimiter = strreplace(getWord(%params, 0), "_", " ");
			%pref.maxWords = getWord(%params, 1);

			%prefsAsFields = strreplace(%pref.defaultValue, %pref.delimiter, "" TAB ""); // hacky but it works

			// amend?
			if(getFieldCount(%prefsAsFields) > %pref.maxWords && %pref.maxWords != -1) {
				%prefsAsFields = getFields(%pref.defaultValue, %pref.maxWords);
			}

			// make sure EVERY field is a valid number.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%prefsAsFields = setField(%prefsAsFields, %i, mFloor(getField(%prefsAsFields, %i)));
			}

			%pref.defaultValue = strreplace(%prefsAsFields, "" TAB "", %pref.delimiter);

		case "datablocklist":
			%pref.dataType = getWord(%params, 0);
			%pref.delimiter = strreplace(getWord(%params, 1), "_", " ");
			%pref.maxWords = getWord(%params, 2);

			%prefsAsFields = strreplace(%pref.defaultValue, %pref.delimiter, "" TAB ""); // hacky but it works

			// amend?
			if(getFieldCount(%prefsAsFields) > %pref.maxWords && %pref.maxWords != -1) {
				%prefsAsFields = getFields(%pref.defaultValue, %pref.maxWords);
			}

			// make sure EVERY field is a valid datablock.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%data = getField(%prefsAsFields, %i);

				%validated = false;

				if(isObject(%data)) {
					if((%data).getClassName() == %pref.dataType) {
						%validated = true;
					}
				}

				if(!%validated) {
					%prefsAsFields = setField(%prefsAsFields, %i, -1);
				}
			}

			%pref.defaultValue = strreplace(%prefsAsFields, "" TAB "", %pref.delimiter);
	}
  
	%blacklist = "$Pref::BLPrefs::AllowedRank $Pref::Server::Name $Pref::Server::WelcomeMessage $Pref::Server::MaxPlayers $Pref::Server::Password $Pref::Server::AdminPassword $Pref::Server::SuperAdminPassword $Pref::Server::ETardFilter $Pref::Server::ETardList $Pref::Server::AutoAdminList $Pref::Server::AutoSuperAdminList $Pref::Server::FallingDamage $Pref::Server::MaxBricksPerSecond $Pref::Server::RandomBrickColor $Pref::Server::TooFarDistance $Pref::Server::WrenchEventsAdminOnly";
	for(%i = 0; %i < getWordCount(%blacklist); %i++) {
		if(%variable $= getWord(%blacklist, %i)) {
			return %pref;
    }
  }
  
  %find = findBLPref(%variable);
  setGlobalByName(getWord(%find, 0), getWord(%find, 1));
	
	%newVariable = true;
	for(%i = 0; %i < $BLPrefs::PrefCount + 1; %i++) {
		if($BLPrefs::Pref[%i] $= %variable) {
			%newVariable = false;
    }
  }
	
	if(%newVariable) {
		$BLPrefs::Pref[$BLPrefs::PrefCount++] = %variable;
  }
	
	return %pref;
}

function registerPrefGroupIcon(%addon, %icon) {
	%groupName = BLP_alNum(%addon) @ "Prefs";
	if(!isObject(%groupName)) {
		%group = new ScriptGroup(BlocklandPrefGroup) {
			class = "PreferenceGroup";
			title = BLP_alNum(%addon);
			legacy = %legacy;
			category = %addon;
			icon = $Pref::BlPrefs::iconDefault;
		};
	} else {
		%group = (%groupName).getID();
	}

	%group.icon = %icon; // change icon with this function, so they're per category only now
}

function findBLPref(%variable) {
	%fo = new FileObject();
	%fo.openForRead($BLPrefs::File);
  while(!%fo.isEOF()){
		%line = %fo.readLine();
    if(getWord(%line, 0) $= %variable) {
      %pref = %line;
      break;
    }
	}
	%fo.close();
	%fo.delete();
  
  if(%pref $= "") {
    echo("\c2[Support_Preferences] Invalid preference: " @ %variable);
    return "";
  }
  
  %var = getWord(%pref, 0);
  %val = getWord(%pref, 2);
  
  %val = getSubStr(%val, 1, strLen(%val) - 3);
  
  return %var SPC %val;
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

	// when storing datablocks, use their NAME.
	if(%this.type $= "datablock")
		setGlobalByName(%this.variable, (%value).getName());
	else {
		setGlobalByName(%this.variable, %value);
	}

	if(%this.callback !$= "") {
		// callback(value, client, pref object);
		// callbacks can now only be function names and always get called with the same value set
		call(%this.callback, %value, %updaterClean, %this);
	}
}

function BlocklandPrefSO::validateValue(%this, %value) {
	//echo("validating" SPC %value SPC "(" @ %this @ ")");

	// this is where the SO's come in handy
	switch$(%this.type) {
		case "num":
			if(%this.decimalPoints !$= "") {
				%value = mFloatLength(%value, %this.decimalPoints);
			}

			if(%value < %this.minValue) {
				%value = %this.minValue;
			}
			else if(%value > %this.maxValue){
				%value = %this.maxValue;
			}

		case "string":
			if(strlen(%value) > %this.maxLength) {
				%value = getSubStr(%value, 0, %this.maxLength);
			}
			if(%this.stripML) {
				%value = stripMLControlChars(%value); // sure we couldn't have a MLString type? Yes.
			}

		case "slider":
			if(%value < %this.minValue) {
				%value = %this.minValue;
			}
			else if(%value > %this.maxValue){
				%value = %this.maxValue;
			}

			%value -= (%value % %this.snapTo);

		case "bool":
			if(%value > 1)
				%value = 1;

			if(%value < 0)
				%value = 0;

		case "dropdown":
			if(%this.valueName[%value] $= "") {
				%value = %this.rowValue[0]; // hacky, but it should work
			}

		case "datablock":
			if(%this.valueName[%value] $= "") {
				%value = %this.rowValue[0];
			}

		case "wordlist":
			%prefsAsFields = strreplace(%value, %this.delimiter, "" TAB ""); // hacky but it works

			// amend?
			if(getFieldCount(%prefsAsFields) > %this.maxWords && %this.maxWords != -1) {
				%prefsAsFields = getFields(%value, %this.maxWords);
			}

			%value = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);

		case "userlist":
			%prefsAsFields = strreplace(%value, %this.delimiter, "" TAB "");

			// amend?
			if(getFieldCount(%prefsAsFields) > %this.maxWords && %this.maxWords != -1) {
				%prefsAsFields = getFields(%value, %this.maxWords);
			}

			// make sure EVERY field is a valid number.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%prefsAsFields = setField(%prefsAsFields, %i, mFloor(getField(%prefsAsFields, %i)));
			}

			%value = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);

		case "datablocklist":
			%prefsAsFields = strreplace(%value, %this.delimiter, "" TAB "" && %this.maxWords != -1);

			// amend?
			if(getFieldCount(%prefsAsFields) > %this.maxWords) {
				%prefsAsFields = getFields(%value, %this.maxWords);
			}

			// make sure EVERY field is a valid datablock.
			for(%i = 0; %i < getFieldCount(%prefsAsFields); %i++) {
				%data = getField(%prefsAsFields, %i);

				%validated = false;

				if(isObject(%data)) {
					if((%data).getClassName() == %this.dataType) {
						%validated = true;
					}
				}

				if(!%validated) {
					%prefsAsFields = setField(%prefsAsFields, %i, -1);
				}
			}

			%value = strreplace(%prefsAsFields, "" TAB "", %this.delimiter);
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

// a wrapper to execute everything in the prefs folder
// will be used for older addons without prefs, if asked for them
if(!$BLPrefs::AddedServerSettings) {
	%file = findFirstFile("./server/prefs/*.cs");

  while(%file !$= "")	{
  	exec(%file);
	  %file = findNextFile("./server/prefs/*.cs");
	}
}

function loadBLPreferences() {
  if(isFile($BLPrefs::File)) {
    // echo("\c5[Support_Preferences] Loading BL Preferences...");
    
    if(!isFile(%backup = "config/server/BLPrefs/prefs.backup")) {
      echo("\c2[Support_Preferences] Backing up preferences file...");
      fileCopy($BLPrefs::File, %backup);
    }
    
    // load all preferences from file and save them so they aren't deleted if their respective addon is disabled
    %fo = new FileObject();
    %fo.openForRead($BLPrefs::File);
    while(!%fo.isEOF()) {
      %variable = getWord(%fo.readLine(), 0);
      
      %newVariable = true;
      
      for(%i = 0; %i < $BLPrefs::PrefCount + 1; %i++) {
        if($BLPrefs::Pref[%i] $= %variable) {
          %newVariable = false;
        }
      }
      
      if(%newVariable) {
        $BLPrefs::Pref[$BLPrefs::PrefCount++] = %variable;
      }
    }
    %fo.close();
    %fo.delete();
    
    exec($BLPrefs::File);
    
    // update preferences to all allowed clients with the pref system
    %fo = new FileObject();
    %fo.openForRead($BLPrefs::File);
    while(!%fo.isEOF()) {
      %pref = getWord(%fo.readLine(), 0);
      
      for(%i = 0; %i < ClientGroup.getCount(); %i++) {
        %client = ClientGroup.getObject(%i);
        
        if(%client.hasPrefSystem && %client.BLP_isAllowedUse()) {
          commandToClient(%client, 'updateBLPref', %pref, getGlobalByName(%pref));
        }
      }
    }
    %fo.close();
    %fo.delete();
  }
}

loadBLPreferences();

function saveBLPreferences() {
  if(!$BLPrefs::Init || !$BLPrefs::serverLoadedPrefs) {
    return;
  }
  
  if($BLPrefs::Pref[0] $= "") {
    return;
  }
  
  // echo("\c5[Support_Preferences] Saving BL Preferences...");
  
	%fo = new FileObject();
	%fo.openForWrite($BLPrefs::File);
	for(%i = 0; %i < $BLPrefs::PrefCount + 1; %i++) {
		%variable = $BLPrefs::Pref[%i];
		%fo.writeLine(%variable @ " = \"" @ getGlobalByName(%variable) @ "\";"); // export(); doesn't return anything :(
	}
	%fo.close();
	%fo.delete();
	
	export("$Pref::Server::*", "config/server/prefs.cs");
}

package BLPrefSaveLoadPackage {
  function onServerCreated() {
    $BLPrefs::serverLoadedPrefs = true;
    
    parent::onServerCreated();
  }
  
	function onServerDestroyed() {
		saveBLPreferences();
		
    $BLPrefs::serverLoadedPrefs = false;
    
		parent::onServerDestroyed();
	}
	
	function onExit() {
		saveBLPreferences();
		
		parent::onExit();
	}
};
activatePackage(BLPrefSaveLoadPackage);

$BLPrefs::Init = true;