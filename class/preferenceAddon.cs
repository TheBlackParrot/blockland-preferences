//PreferenceAddon class file

function PreferenceAddon::onAdd(%this) {
  if(%this.initialized)
    return;

  %this.setName(%this.name);
  PreferenceAddonGroup.add(%this);
  %this.initialized = true;
}

function registerPreferenceAddon(%addon, %title, %icon) {
  if(!isFile("Add-Ons/" @ %addon @ "/server.cs")) {
    error("[Support_Preferences] Failed to register add-on \"" @ %addon @ "\" - does not exist");
  }
  %groupName = BLP_alNum(%addon) @ "Prefs";
	if(!isObject(%groupName)) {
		%group = new ScriptGroup(PreferenceAddon) {
			name = %groupName;

			title = %title;

			file = %addon;
			icon = %icon;

			legacy = false;
		};
	} else {
		%group = (%groupName).getID();

    %group.title = %title;
    %group.icon = %icon;
	}
}
