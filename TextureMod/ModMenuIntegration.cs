// Script used to connect to ModMenu
using System.Collections.Generic;
using UnityEngine;
using LLModMenu;
using System.IO;
using System;

namespace TextureMod
{
    public class ModMenuIntegration : MonoBehaviour
    {
        private Config config;
        private ModMenu mm;
        private bool mmAdded = false;

        public List<Entry> writeQueue = new List<Entry>();

        private void Start()
        {
            InitConfig();
        }

        private void Update()
        {
            if (mmAdded == false)
            {
                mm = FindObjectOfType<ModMenu>();
                if (mm != null)
                {
                    mm.mods.Add(base.gameObject.name);
                    mmAdded = true;
                }
            }
        }


        private void InitConfig()
        {
            /*
             * Mod menu now uses a single function to add options etc. (AddToWriteQueue)
             * your specified options should be added to this function in the same format as stated under
             * 
            Keybindings:
            AddToWriteQueue("(key)keyName", "LeftShift");                                       value can be: Any KeyCode as a string e.g. "LeftShift"

            Options:
            AddToWriteQueue("(bool)boolName", "true");                                          value can be: ["true" | "false"]
            AddToWriteQueue("(int)intName", "27313");                                           value can be: any number as a string. For instance "123334"
            AddToWriteQueue("(slider)sliderName", "50|0|100");                                  value must be: "Default value|Min Value|MaxValue"
            AddToWriteQueue("(header)headerName", "Header Text");                               value can be: Any string
            AddToWriteQueue("(gap)gapName", "identifier");                                      value does not matter, just make name and value unique from other gaps

            ModInformation:
            AddToWriteQueue("(text)text1", "Descriptive text");                                  value can be: Any string
            */


            // Insert your options here \/
            AddToWriteQueue("(key)enableSkinChanger", "LeftShift");
            AddToWriteQueue("(key)nextSkin", "Mouse0");
            AddToWriteQueue("(key)previousSkin", "Mouse3");
            AddToWriteQueue("(key)cancelOpponentCustomSkin", "A");
            AddToWriteQueue("(key)reloadEntireSkinLibrary", "F9");
            AddToWriteQueue("(key)reloadCustomSkin", "F5");

            AddToWriteQueue("(key)enterShowcaseStudio", "Tab");
            AddToWriteQueue("(key)showcaseStudioHideHud", "F3");
            AddToWriteQueue("(key)showcaseStudioRotateCharacter", "Mouse0");
            AddToWriteQueue("(key)showcaseStudioMoveCamera", "Mouse2");
            AddToWriteQueue("(key)showcaseStudioMoveLight", "Mouse3");


            AddToWriteQueue("(header)h1", "Lobby Settings:");
            AddToWriteQueue("(bool)noHoldMode", "false");
            AddToWriteQueue("(bool)neverApplyOpponentsSkin", "false");
            AddToWriteQueue("(bool)lockButtonsOnRandom", "true");
            AddToWriteQueue("(bool)assignFirstSkinOnCharacterSelection", "false");
            AddToWriteQueue("(gap)1", "1");
            AddToWriteQueue("(header)h2", "Real-time Skin editing:");
            AddToWriteQueue("(bool)reloadCustomSkinOnInterval", "true");
            AddToWriteQueue("(slider)skinReloadIntervalInFrames", "60|10|600");
            AddToWriteQueue("(gap)2", "2");
            AddToWriteQueue("(header)h3", "General:");
            AddToWriteQueue("(bool)showDebugInfo", "false");
            AddToWriteQueue("(gap)3", "2");
            AddToWriteQueue("(header)h4", "Parry Color:");
            AddToWriteQueue("(bool)enableCustomParryAndClash", "true");
            AddToWriteQueue("(slider)parryFirstColorR", "2|0|255");
            AddToWriteQueue("(slider)parryFirstColorG", "205|0|255");
            AddToWriteQueue("(slider)parryFirstColorB", "239|0|255");
            AddToWriteQueue("(gap)4", "2");
            AddToWriteQueue("(slider)parrySecondColorR", "0|0|255");
            AddToWriteQueue("(slider)parrySecondColorG", "152|0|255");
            AddToWriteQueue("(slider)parrySecondColorB", "194|0|255");
            AddToWriteQueue("(gap)5", "2");
            AddToWriteQueue("(slider)parryThirdColorR", "255|0|255");
            AddToWriteQueue("(slider)parryThirdColorG", "255|0|255");
            AddToWriteQueue("(slider)parryThirdColorB", "255|0|255");

            AddToWriteQueue("(text)text3", "Wondering how to assign skins and in what part of the game you can do so?");
            AddToWriteQueue("(text)text4", "Simply hold the 'Enable Skin Changer' button and press either the `Next skin` or the `Previous Skin` buttons to cycle skins");
            AddToWriteQueue("(text)text5", "Skins can be assigned in Ranked Lobbies, 1v1 Lobbies, FFA Lobbies(Only for player 1 and 2) and in the skin unlock screen for a character or in ShowcaseStudio");
            AddToWriteQueue("(text)text6", "If you select random in the lobby and try to assign a custom skin you will be given a random character and random skin. In online lobbies you will be set to ready, and your buttons will become unavailable unless you've deactivated 'Lock Buttons On Random'");
            AddToWriteQueue("(text)text7", " ");
            AddToWriteQueue("(text)text8", "If you wish to real time edit your skins, use the F5 button to reload your skin whenever you're in training mode or in the character skin unlock screen");
            AddToWriteQueue("(text)text9", "You can also enable the interval mode and have it automatically reload the current custom skin every so often. Great for dual screen, or windowed mode setups (Does not work in training mode)");
            AddToWriteQueue("(text)text1", "This mod was written by MrGentle");

            this.config = ModMenu.Instance.configManager.InitConfig(gameObject.name, writeQueue);
            writeQueue.Clear();
        }

        public void AddToWriteQueue(string key, string value)
        {

            string[] splits = key.Remove(0, 1).Split(')');
            if (splits[0] == "bool")
            {
                if (value == "true") value = "True";
                if (value == "false") value = "False";
            }
            writeQueue.Add(new Entry(splits[1], value, splits[0]));
        }

        public void AddEntryToWriteQueue(string key, string value, string type)
        {
            if (type == "bool")
            {
                if (value == "true") value = "True";
                if (value == "false") value = "False";
            }
            writeQueue.Add(new Entry(key, value, type));
        }

        public KeyCode GetKeyCode(string keyName)
        {
            return this.config.GetKeyCode(keyName.Split(')')[1]);
        }
        public KeyCode NewGetKeyCode(string keyName)
        {
            return this.config.GetKeyCode(keyName);
        }

        public bool GetTrueFalse(string boolName)
        {
            return this.config.GetBool(boolName.Split(')')[1]);
        }

        public bool NewGetTrueFalse(string boolName)
        {
            return this.config.GetBool(boolName);
        }

        public int GetSliderValue(string sliderName)
        {
            return this.config.GetSliderValue(sliderName.Split(')')[1]);
        }

        public int NewGetSliderValue(string sliderName)
        {
            return this.config.GetSliderValue(sliderName);
        }

        public int GetInt(string intName)
        {
            return this.config.GetInt(intName.Split(')')[1]);
        }

        public int NewGetInt(string intName)
        {
            return this.config.GetInt(intName);
        }
    }
}