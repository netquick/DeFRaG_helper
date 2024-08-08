Simple Map browser for Quake 3 Mod "DeFRaG" using list of map from ws.q3df.org. Can find existing DeFRaG installations and installed maps or pull the required files from internet. 
oDFe.x64.exe and the defrag package will be pulled along with some data for this tool like preview Images. 
Launches maps on doubleclick with Random Map feature, storing the last played maps. Favorite function and easy to filter browser to check installed maps. 
Installed maps will be synched on launch. Background tasks update newest maps and add them to the database. 

Avoiding libraries where possible this still uses following NuGet Packages: 
- Microsoft.Data.SQlite
- SevenZipExtractor
  
Custom styles are selfmade as packages like mahApp.metro blew my app to 110MB (this is about 1.1MB actually without libraries, 5.7MB packed ). WPF Application in pure C#. Source is free to use, credits for the parser classes are welcome. 

No commercial interest - Aimed to get more people into DeFRaG and let the ones that already play it have it easier to try out maps. Enjoy.

Startscreen offers access to active servers and the last played Random maps

![image](https://github.com/user-attachments/assets/ab9b443b-eff4-4b5d-8753-242681b5b9ee)



Map Browser with over 18k maps, launch by individual playbuttons or download

![image](https://github.com/user-attachments/assets/713fc7b8-6dcc-474c-814d-f2aa3bf3afc0)


Server Browser included

![image](https://github.com/user-attachments/assets/4211d957-4b02-4df5-9de5-e71621a40d26)



Demo Browser to play demos by doubleclick. Required map will be installed as well. Last played maps are available for quick access.

![image](https://github.com/user-attachments/assets/fef09ef4-d840-4fc1-b2ae-5c7d5d1f4413)


****************************
Features:

- Auto Update
- Map Browser with local sqlite db
- Installs and plays maps with one click
- Updating maps from internet
- Server Browser refreshing state every 30 seconds
- Demo browser to fetch demos for maps and one-click play
- Radom Map feature
- Map History for random and all played maps
- Favorites
- Autoinstallation of DeFRaG if not yet installed
- Sync of local installed maps

  ... more to come

****************************
Release Notes:

v0.9.3  Implemented Edit function. Converts a bsp from the pk3 file to map and opens it in Netradiant_Custom. Editor will be installed if not available. 
****************************
