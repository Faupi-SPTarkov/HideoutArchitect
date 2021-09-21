using Aki.Common;
using Aki.Reflection.Patching;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HideoutArchitect.Patches
{
    class Patcher
    {
        public static void PatchAll()
        {
            new PatchManager().RunPatches();
        }
    }

    public class PatchManager
    {
        public PatchManager()
        {
            this._patches = new PatchList
            {
                new ItemViewPatches.GridItemViewUpdateInfoPatch(),
                new ItemViewPatches.ItemViewInitPatch(),
                new ItemViewPatches.NewGridItemViewPatch()
            };
        }

        public void RunPatches()
        {
            this._patches.EnableAll();
        }

        private readonly PatchList _patches;
    }

    public static class ItemViewPatches
    {
        public static Dictionary<ItemView, HideoutItemViewPanel> hideoutPanels = new Dictionary<ItemView, HideoutItemViewPanel>();

        public static void SetHideoutItemViewPanel(this ItemView __instance)
        {
            if (!hideoutPanels.TryGetValue(__instance, out HideoutItemViewPanel hideoutItemViewPanel))
                return;

            ItemUiContext itemUiContext = typeof(ItemView).GetField("ItemUiContext", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as ItemUiContext;

            if (hideoutItemViewPanel != null)
            {
                hideoutItemViewPanel.Show(__instance.Item, __instance, itemUiContext?.Tooltip);
                return;
            }
        }

        public class NewGridItemViewPatch : Patch
        {
            public NewGridItemViewPatch() : base(typeof(NewGridItemViewPatch), null, null, "PatchPostfix", null, null) { }

            protected override MethodBase GetTargetMethod()
            {
                return typeof(GridItemView).GetMethod("NewGridItemView", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            private static void PatchPostfix(ref GridItemView __instance, Item item)
            {
                if (hideoutPanels.ContainsKey(__instance)) return;
                try
                {
                    QuestItemViewPanel questIconPanel = typeof(ItemView).GetField("_questsItemViewPanel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as QuestItemViewPanel;
                    HideoutItemViewPanel hideoutIconPanel = GameObject.Instantiate(Resources.GetEditOffsetWindowTemplate(questIconPanel), questIconPanel.transform.parent);
                    hideoutIconPanel.transform.SetAsFirstSibling();
                    hideoutPanels[__instance] = hideoutIconPanel;

                    hideoutIconPanel.gameObject.SetActive(true);
                }
                catch { 
                    // Item doesn't have a "quest item" icon panel, so it's probably static
                }
            }
        }

        public class ItemViewInitPatch : Patch
        {
            public ItemViewInitPatch() : base(typeof(ItemViewInitPatch), null, null, "PatchPostfix", null, null) { }

            protected override MethodBase GetTargetMethod()
            {
                return typeof(ItemView).GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            private static void PatchPostfix(ref ItemView __instance)
            {
                __instance.SetHideoutItemViewPanel();
            }
        }

        public class GridItemViewUpdateInfoPatch : Patch
        {
            public GridItemViewUpdateInfoPatch() : base(typeof(GridItemViewUpdateInfoPatch), null, null, "PatchPostfix", null, null) { }

            protected override MethodBase GetTargetMethod()
            {
                return typeof(GridItemView).GetMethod("UpdateInfo", BindingFlags.Instance | BindingFlags.Public);
            }

            private static void PatchPostfix(ref GridItemView __instance)
            {
                if (!__instance.IsSearched)
                    return;

                if (!hideoutPanels.TryGetValue(__instance, out HideoutItemViewPanel hideoutItemViewPanel))
                    return;
                hideoutItemViewPanel.iconImage.gameObject.SetActive(HideoutArchitect.IsNeededForHideoutUpgrades(__instance.Item));

                __instance.SetHideoutItemViewPanel();
            }
        }
    }
}
