# DRLeagueParser
Today : A parser for Dirt Rally league results, forked from [aarondemarre/DRLeagueParser](https://github.com/aarondemarre/DRLeagueParser)
Tomorrow : A manager for Dirt Rally league results

## Why this project ? 
DiRT Rally is a fantastic rally game with leagues system. A web application display some details for current event. This application consume a public API (designed by himself).
The purpose of this project is to create a new application based on this API, to : 
* expolit full data of the API 
* Store and retrieve data after event finished (because the API doesn't retrieved Driver Times once event finished)
* Display some comparative charts 
* ...

## Roadmap
* Document this unofficial RaceNet API
* Enhance WPF application ==> DR League Viewer 
	* Exploit all the information of RaceNetAPI in the WPF
	* Add caching system to limit calls
	* Persist data 
	* Export features
	* Some UX improvements
* Create a Web application ==> DR League Manager
	* Add some charts 
	* Auto updater system (when you set event ID first time)
	* Custom leaderboard scoring system
	* ...

Thanks to CanardPC DR racers (cooly08, zexav, ...),  [aarondemarre](https://github.com/aarondemarre) and futurs contributors.