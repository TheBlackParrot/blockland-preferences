//Object-based preference initialization
// This is both an example and unit test
// Goal for Prefs v2.0

//registerPreferenceAddon(%addon, %title, %icon);
registerPreferenceAddon("Support_Preferences", "Prefs", "gear");

new ScriptObject(MyCoolPref) {
  class          = "Preference"; //don't change this one

  addon          = "Support_Preferences"; //add-on filename
  category       = "General";
  title          = "Can use";

  type           = "dropdown";
  params         = "Host 3 Super_Admin 2 Admin 1"; //list based parameters

  variable       = "$Pref::BLPrefs::AllowedRank"; //global variable (optional)

  defaultValue   = "Super_Admin 2"; //two words due to dropdown

  updateCallback = "myRealUpdateCallback"; //to call after ::onUpdate (optional)
  loadCallback   = "myRealUpdateCallback"; //to call after ::onLoad (optional)

  hostOnly       = true; //default false (optional)
  secret         = false; //whether to tell clients the value was updated (optional)

  loadNow        = false; // load value on creation instead of with pool (optional)
  noSave         = false; // do not save (optional)
  requireRestart = false; // denotes a restart is required (optional)
};

function MyCoolPref::onUpdate(%this, %val) {
  //realUpdateCallback should have already been called by now,
  // handled by the Preference generic

  if($Test::Pref $= %val) {
    echo("Passed update callback test");
  } else {
    echo("Failed update callback test");
  }
}

function MyCoolPref::onLoad(%this, %val) {
  echo("Loaded \"" @ %this.name @ "\"");
}

function myRealUpdateCallback(%this, %val) {
  $Test::Pref = %val;
}
