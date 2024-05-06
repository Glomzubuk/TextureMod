using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLScreen;
using BepInEx.Logging;
using LLBML;
using LLBML.States;
using LLBML.Players;

namespace TextureMod
{
    public class ModelHandler
    {
        private ManualLogSource Logger => TextureMod.Log;

        public readonly Character character;
        public readonly CharacterVariant variant;
        private List<CharacterModel> models = new List<CharacterModel>();
        public List<Renderer> Renderers { get; private set; } = new List<Renderer>();
        public Texture2D texture = null;
        public ScreenType type;


        public ModelHandler(Character character, CharacterVariant variant, ScreenType type = ScreenType.NONE)
        {
            this.character = character;
            this.variant = variant;
            this.type = type;
        }

        public void Add(IEnumerable<Renderer> renderers)
        {
            this.Renderers.AddRange(renderers);
        }

        public void Add(Func<IEnumerable<Renderer>> renderersCallback)
        {
            TextureMod.Instance.StartCoroutine(WaitForRenderers(renderersCallback));
        }

        public void Add(CharacterModel model)
        {
            this.models.Add(model);
            TextureMod.Instance.StartCoroutine(WaitForRenderers(model));
        }

        public IEnumerator WaitForRenderers(CharacterModel model)
        {
            return WaitForRenderers(() => model?.curModel?.transform?.GetComponentsInChildren<Renderer>());
        }

        public IEnumerator WaitForRenderers(Func<IEnumerable<Renderer>> renderersCallback)
        {
            IEnumerable<Renderer> rs = null;
            yield return new WaitUntil(() =>
            {
                if (IsObsolete()) return true;
                rs = renderersCallback.Invoke();
                if (rs != null)
                {
                    foreach (Renderer renderer in rs)
                    {
                        if (renderer.enabled == true && renderer.isVisible == true)
                        {
                            return true;
                        }
                    }
                }
                return false;
            });
            Renderers.AddRange(rs);
        }

        public void Update()
        {

            if (GameStates.IsInLobby())
            {
                models.ForEach((model) => model.SetSilhouette(false));
            }
            if (texture != null)
            {
                /*
                if (GameStates.IsInMatch() && Time.frameCount % 100 != 0)
                {
                    return;
                }*/
                foreach (Renderer r in Renderers)
                {
                    if (r != null && r.isVisible)
                    {
                        RendererHelper.AssignTextureToRenderer(r, texture, character, variant);
                    }
                }
            }
        }

        public bool IsObsolete() {
            if(ScreenApi.CurrentScreens[0]?.screenType != type && ScreenApi.CurrentScreens[1]?.screenType != type)
            {
                return true;
            }

            return false;
        }

        public bool ValidCharacter(Character character, CharacterVariant variant = CharacterVariant.DEFAULT)
        {
            if (character == this.character && VariantHelper.VariantMatch(variant, VariantHelper.GetModelVariant(this.variant)))
            {
                return true;
            }

            return false;
        }

        public static ModelHandler GetCurrentModelHandler(int playerNr = -1)
        {
            ScreenBase screenZero = ScreenApi.CurrentScreens[0];
            ScreenBase screenOne = ScreenApi.CurrentScreens[1];

            if (playerNr >= 0)
            {
                if (screenZero is ScreenPlayers sp)
                {
                    CharacterModel model = sp.playerSelections[playerNr].characterModel;
                    ModelHandler mh = new ModelHandler(model.character, model.characterVariant, sp.screenType);
                    mh.Add(model);
                    return mh;
                }
                else if (screenZero is ScreenGameHud sgh)
                {
                    Player player = Player.GetPlayer(playerNr);
                    GameHudPlayerInfo playerInfo = sgh.playerInfos[player.nr];

                    ModelHandler mh = new ModelHandler(player.Character, player.CharacterVariant, sgh.screenType);
                    mh.Add(() => playerInfo.gameObject.transform.GetComponentsInChildren<Renderer>());
                    mh.Add(() => player.playerEntity.skinRenderers);

                    return mh;
                } else if (screenZero is PostScreen postScreen)
                {
                    Player player = Player.GetPlayer(playerNr);
                    PostSceenPlayerBar playerBar = postScreen.playerBarsByPlayer[player.nr];
                    ModelHandler mh = new ModelHandler(player.Character, player.CharacterVariant, postScreen.screenType);
                    if (postScreen.winner.CJFLMDNNMIE == player.nr) // postScreen.winner.nr
                    {
                        mh.Add(postScreen.winnerModel);
                    }
                    mh.Add(() => playerBar.gameObject.transform.GetComponentsInChildren<Renderer>());
                    return mh;
                }
            }
            else if (playerNr < 0)
            {
                if (screenOne is ScreenUnlocksSkins sus)
                {
                    ModelHandler mh = new ModelHandler(sus.previewModel.character, sus.previewModel.characterVariant, sus.screenType);
                    mh.Add(sus.previewModel);
                    return mh;
                }
                else if (screenOne is ScreenUnlocksCharacters suc)
                {
                    ModelHandler mh = new ModelHandler(suc.previewModel.character, suc.previewModel.characterVariant, suc.screenType);
                    mh.Add(suc.previewModel);
                    return mh;
                }
            }
            return null;
            /*
            throw new NotSupportedException("Got request for a model handler in an unsupported screen.\n" +
                " - Screens[0].type: " + screenZero?.screenType.ToString() + "\n" +
                " - Screens[1].type: " + screenOne?.screenType.ToString());
                */
        }
    }

    public enum ModelHandlerType
    {
        None,
        Lobby,
        Game,
        Results,
        ShowcasePreview,
        ShowcaseSkins,
    }
}
