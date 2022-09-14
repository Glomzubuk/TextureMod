using System;
using System.IO;
using TextureMod.CustomSkins;

namespace TextureMod
{
    public static class VariantHelper
    {


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


        public static CharacterVariant GetDefaultVariantForModel(ModelVariant variantType)
        {
            switch (variantType)
            {
                case ModelVariant.Alternative:
                    return CharacterVariant.MODEL_ALT;
                case ModelVariant.DLC:
                    return CharacterVariant.MODEL_ALT3;
                case ModelVariant.Default:
                    return CharacterVariant.DEFAULT;
                default:
                    return CharacterVariant.DEFAULT;
            }
        }
    }
}
