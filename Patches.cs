using Aki.Common.Utils.Patching;
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
            PatcherUtil.Patch<ItemViewPatches.GridItemViewUpdateInfoPatch>();
            PatcherUtil.Patch<ItemViewPatches.NewGridItemViewPatch>();
            PatcherUtil.Patch<ItemViewPatches.ItemViewInitPatch>();
        }
    }

    public static class ItemViewPatches
    {
        public static Dictionary<ItemView, HideoutItemViewPanel> hideoutPanels = new Dictionary<ItemView, HideoutItemViewPanel>();

        public static void SetHideoutItemViewPanel(this ItemView __instance)
        {
            HideoutItemViewPanel hideoutItemViewPanel;
            if (!hideoutPanels.TryGetValue(__instance, out hideoutItemViewPanel))
                return;

            GClass1768 inventoryController = typeof(ItemView).GetField("InventoryController", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as GClass1768;
            ItemUiContext itemUiContext = typeof(ItemView).GetField("ItemUiContext", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as ItemUiContext;

            if (hideoutItemViewPanel != null && inventoryController != null)
            {
                hideoutItemViewPanel.Show(__instance.Item, __instance, (itemUiContext != null) ? itemUiContext.Tooltip : null);
                return;
            }
        }

        public class NewGridItemViewPatch : GenericPatch<NewGridItemViewPatch>
        {
            public NewGridItemViewPatch() : base(null, "PatchPostfix", null, null) { }

            protected override MethodBase GetTargetMethod()
            {
                return typeof(GridItemView).GetMethod("NewGridItemView", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            private static void PatchPostfix(ref GridItemView __instance, Item item, ItemRotation rotation, GClass1768 inventoryController, IItemOwner itemOwner, [CanBeNull] FilterPanel filterPanel, [CanBeNull] ItemUiContext itemUiContext, GClass2088 insurance, bool isSearched = true)
            {
                if (hideoutPanels.ContainsKey(__instance)) return;
                QuestItemViewPanel questIconPanel = typeof(ItemView).GetField("_questsItemViewPanel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as QuestItemViewPanel;
                HideoutItemViewPanel hideoutIconPanel = GameObject.Instantiate(Resources.GetEditOffsetWindowTemplate(questIconPanel), questIconPanel.transform.parent);
                hideoutIconPanel.transform.SetAsFirstSibling();
                hideoutPanels[__instance] = hideoutIconPanel;

                hideoutIconPanel.gameObject.SetActive(true);
            }
        }

        public class ItemViewInitPatch : GenericPatch<ItemViewInitPatch>
        {
            public ItemViewInitPatch() : base(null, "PatchPostfix", null, null) { }

            protected override MethodBase GetTargetMethod()
            {
                return typeof(ItemView).GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            private static void PatchPostfix(ref ItemView __instance)
            {
                __instance.SetHideoutItemViewPanel();
            }
        }

        public class GridItemViewUpdateInfoPatch : GenericPatch<GridItemViewUpdateInfoPatch>
        {
            public GridItemViewUpdateInfoPatch() : base(null, "PatchPostfix", null, null) { }

            protected override MethodBase GetTargetMethod()
            {
                return typeof(GridItemView).GetMethod("UpdateInfo", BindingFlags.Instance | BindingFlags.Public);
            }

            private static void PatchPostfix(ref GridItemView __instance)
            {
                if (!__instance.IsSearched)
                    return;

                HideoutItemViewPanel hideoutItemViewPanel;
                if (!hideoutPanels.TryGetValue(__instance, out hideoutItemViewPanel))
                    return;
                hideoutItemViewPanel.iconImage.gameObject.SetActive(Utils.IsNeededForHideoutUpgrades(__instance.Item));

                __instance.SetHideoutItemViewPanel();
            }
        }
    }
}
