//riding on Greek2Me's preloading script
//since we're essentially a necessity, we'll ensure that we load every time

exec("config/server/ADD_ON_LIST.cs");
$AddOn__Support_Preferences = 1; //I don't know what the actual name is, this is just what I (jincux) have been using
export("$AddOn__*", "config/server/ADD_ON_LIST.cs");

//alternatively, we could just put all the server.cs code in here.
//That way, everything is loaded up before the rest of the add-ons are executed
