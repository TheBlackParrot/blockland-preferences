//PreferenceGroup class file

function PreferenceGroup::onAdd(%this) {
  if(%this.initialized)
    return;

  %this.setName(%this.name);
  PreferenceGroups.add(%this);
  %this.initialized = true;
}
