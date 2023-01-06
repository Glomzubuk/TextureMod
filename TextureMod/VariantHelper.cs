using System;
using System.Collections.Generic;
using System.IO;

namespace TextureMod
{
    public static class VariantHelper
    {
        public static List<CharacterVariant> DefaultModelVariants = new List<CharacterVariant>
        {
            CharacterVariant.DEFAULT,
            CharacterVariant.ALT0,
            CharacterVariant.ALT1,
            CharacterVariant.ALT2,
            CharacterVariant.ALT3,
            CharacterVariant.ALT4,
            CharacterVariant.ALT5,
            CharacterVariant.ALT6,
            CharacterVariant.STATIC_ALT,
        };
        public static List<CharacterVariant> AltModelVariants = new List<CharacterVariant>
        {
            CharacterVariant.MODEL_ALT,
            CharacterVariant.MODEL_ALT2,
        };
        public static List<CharacterVariant> DLCModelVariants = new List<CharacterVariant>
        {
            CharacterVariant.MODEL_ALT3,
            CharacterVariant.MODEL_ALT4,
        };

        public static bool VariantMatch(CharacterVariant characterVariant, ModelVariant modelVariant)
        {
            switch (modelVariant)
            {
                case ModelVariant.Default:
                    return characterVariant < CharacterVariant.STATIC_ALT;
                case ModelVariant.Alternative:
                    return characterVariant == CharacterVariant.MODEL_ALT || characterVariant == CharacterVariant.MODEL_ALT2;
                case ModelVariant.DLC:
                    return characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4;
                default:
                    return false;
            }
        }

        public static ModelVariant GetModelVariantFromFilePath(string path)
        {

            string fileName = Path.GetFileNameWithoutExtension(path);

            if (fileName.Contains("_ALT2"))
            {
                return ModelVariant.DLC;
            }
            else if (fileName.Contains("_ALT"))
            {
                return ModelVariant.Alternative;
            }
            else return ModelVariant.Default;
        }

        public static ModelVariant GetModelVariant(CharacterVariant characterVariant)
        {
            if (DefaultModelVariants.Contains(characterVariant)) return ModelVariant.Default;
            else if (AltModelVariants.Contains(characterVariant)) return ModelVariant.Alternative;
            else if (DLCModelVariants.Contains(characterVariant)) return ModelVariant.DLC;
            else return ModelVariant.None;
        }


        public static CharacterVariant GetDefaultVariantForModel(ModelVariant variantType)
        {
            switch (variantType)
            {
                case ModelVariant.Default:
                    return DefaultModelVariants[0];
                case ModelVariant.Alternative:
                    return AltModelVariants[0];
                case ModelVariant.DLC:
                    return DLCModelVariants[0];
                default:
                    return CharacterVariant.DEFAULT;
            }
        }
        public static List<CharacterVariant> GetVariantsForModel(ModelVariant variantType)
        {
            switch (variantType)
            {
                case ModelVariant.Default:
                    return DefaultModelVariants;
                case ModelVariant.Alternative:
                    return AltModelVariants;
                case ModelVariant.DLC:
                    return DLCModelVariants;
                default:
                    return null;
            }
        }
        public static CharacterVariant GetNextVariantForModel(ModelVariant variantType, CharacterVariant characterVariant)
        {
            List<CharacterVariant> availableVariants = GetVariantsForModel(variantType);
            if (VariantMatch(characterVariant, variantType))
            {
                int index = availableVariants.IndexOf(characterVariant);
                return availableVariants[(index + 1) % availableVariants.Count];
            }

            return CharacterVariant.STATIC_ALT;
        }
    }


    /*
    CharacterVariant GetCustomSkinVariant(ModelVariant variantType, CharacterVariant characterVariant)
    {
        switch (variantType)
        {
            case ModelVariant.Alternative:
                if (characterVariant == CharacterVariant.MODEL_ALT || characterVariant == CharacterVariant.MODEL_ALT2)
                {
                    return characterVariant;
                }
                else if (opponentPlayer?.CharacterSelected == localLobbyPlayer.CharacterSelected)
                {
                    return opponentPlayer.CharacterVariant == CharacterVariant.MODEL_ALT ? CharacterVariant.MODEL_ALT2 : CharacterVariant.MODEL_ALT;
                }
                else return CharacterVariant.MODEL_ALT;
            case ModelVariant.DLC:
                if (characterVariant == CharacterVariant.MODEL_ALT3 || characterVariant == CharacterVariant.MODEL_ALT4)
                {
                    return characterVariant;
                }
                else if (opponentPlayer?.CharacterSelected == localLobbyPlayer.CharacterSelected)
                {
                    return opponentPlayer.CharacterVariant == CharacterVariant.MODEL_ALT3 ? CharacterVariant.MODEL_ALT4 : CharacterVariant.MODEL_ALT3;
                }
                else return CharacterVariant.MODEL_ALT3;
            default:
                if (characterVariant < CharacterVariant.STATIC_ALT)
                {
                    return characterVariant;
                }
                else if (opponentPlayer?.CharacterSelected == localLobbyPlayer.CharacterSelected)
                {
                    return opponentPlayer.CharacterVariant == CharacterVariant.DEFAULT ? CharacterVariant.ALT0 : CharacterVariant.DEFAULT;
                }
                else return CharacterVariant.DEFAULT;
        }
    }
    */

    public enum ModelVariant
    {
        None,
        Default,
        Alternative,
        DLC,
    }
}
