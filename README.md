# TextureMod

Version: 2.0.0  
Main Author: MrGentle  
Previous Maintainers: Daioutzu  
Current Maintainer: Glomzubuk  

Welcome to the world of simple skins imports, and easy skin creation!
Here you can get the modified TextureMod base skins for the additionnal effects support: [Skin.Editing.Textures.zip](https://github.com/Daioutzu/LLBMM-TextureMod/releases/download/1.4.9/Skin.Editing.Textures.zip)
Check out the community skin pack [over here](https://drive.google.com/drive/folders/1y1F2hbE-I4IXfeLJZ73_-tl9AJATSlTY?usp=sharing)!


**First of all, let's talk about files and folders.**  
DDS SKIN FILES WILL NOT WORK!!
Skins for use with TextureMod should be PNG files, so if you want to use a DDS skin i've included a fantastic DDS to PNG converter with the mod.

Any skins you want installed should be put into this location:
`<ModdingFolder>\TextureMod\Characters\<Character>\<Author>\<Skin>`

So if Hang made a Jet skin the folder structure would be:
`Characters\JET\01#Hang\01#Skin.png`

But why did Hang add numbers to the Author folder, and the skin name?
That's because of TextureMod's sorting system, which loads files based on folder structure.

![Folders picture](https://glomzubuk.fr/hosting/texmod/Folders.png)

In the above case, the skins within the Gentle folder will be loaded first, followed by ShyShannon folder, followed By aTastyT0ast folder. The skins inside the folder will also be loaded in their respective order:

![Skins picture](https://glomzubuk.fr/hosting/texmod/Skins.png)

When you've placed all your skins in the correct folders, you can install your mod, or hit refresh mod if you just wanna add some skins to an existing installation.

**How do i specify what model should use my skin?**  
if you want to use your characters alternative skin, you add _ALT to the end of the filename. (For doombox or any other character with 3+ models you do _ALT and _ALT2 respectively)
so here are 3 examples:  
dbSkin.png – Assigns the original model  
dbSkin_ALT.png – Assigns Omega model  
dbSkin_ALT2.png – Assigns Visualizer model

![ALT skins naming format](https://glomzubuk.fr/hosting/texmod/ALTNaming.png)

In the above example, the Green Robo skin will be assigned to mecha Latch.

**I have my skins installed, how do i get started?**  
Open your game and go to Options > Mod Settings > TextureMod and set your keybindings. You can also bind controller buttons here.

Now enter a lobby, or showcase! In the lobby, select your character and press the button combination you assigned to set a custom skin. Press the combination multiple times to cycle through the skins.
Your opponents custom skin will automatically be assigned if they choose one. You can also press your cancel button if you don’t want to see your opponents skin this game.

**I want to test out skin creation, what's a good workflow?**  
Personally, i copy one of the included original character textures (which you can find in the mod folder) and copy it over to
`<ModdingFolder>\TextureMod\Characters\<Character>\<Author>`
then name it something like `01#WIP.png`

Open up the game and browse to the specified characters showcase and open Showcase Studio.

Select your custom skin with the assigned button combination and press your assigned "Reload Custom Skin" button.
Your skin should now be reloading from file on an interval.

Now open the 01#WIP.png file in your favorite image editor and start working.
When you want to see how the skin looks, save over the original `01#WIP.png` file, and look ingame. Once the skin refreshes, you should see the changes.

When you're done editing, copy the finished skin from
`<ModdingFolder>\TextureMod\Characters\<Character>\<Author>`
and put it somewhere it wont get lost.
