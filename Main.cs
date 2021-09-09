using Comfort.Common;
using EFT.Hideout;
using EFT.InventoryLogic;
using HideoutArchitect.Patches;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace HideoutArchitect
{
    public class HideoutArchitect
    {
        private static Transform _gameObjectStorage;
        public static Transform GameObjectStorage
        {
            get
            {
                if (_gameObjectStorage == null)
                {
                    GameObject storage = new GameObject("HideoutArchitect Storage");
                    UnityEngine.Object.DontDestroyOnLoad(storage);
                    storage.SetActive(false);
                    _gameObjectStorage = storage.transform;
                }

                return _gameObjectStorage;
            }
        }


        private static ModInformation _modInfo;
        public static ModInformation ModInfo
        {
            private set
            {
                _modInfo = value;
            }
            get
            {
                if (_modInfo == null)
                    LoadModInfo();
                return _modInfo;
            }
        }

        private static void Main()
        {
            _ = Resources.LoadTexture("neededforhideout", Path.Combine(ModInfo.path, "icon_neededforhideout_small.png"));
            Patcher.PatchAll();
        }


        private static void LoadModInfo()
        {
            JObject response = JObject.Parse(Aki.SinglePlayer.Utils.RequestHandler.GetJson($"/HideoutArchitect/GetInfo"));
            try
            {
                Assert.IsTrue(response.Value<int>("status") == 0);
                ModInfo = response["data"].ToObject<ModInformation>();
            }
            catch (Exception getModInfoException)
            {
                string errMsg = $"[{typeof(HideoutArchitect)}] Package.json couldn't be found! Make sure you've installed the mod on the server as well!";
                Debug.LogError(errMsg);
                throw getModInfoException;
            }

        }
    }
}
