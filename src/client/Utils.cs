using Comfort.Common;
using EFT.Hideout;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace HideoutArchitect
{
    public static class Utils
    {
        public static List<AreaData> GetApplicableUpgrades(Item item)
        {
            List<AreaData> areas = Singleton<GClass1282>.Instance.AreaDatas.Where(area =>
            {
                return area.Status != EAreaStatus.NotSet 
                    && area.Template.Enabled == true // "Place of fame" for example
                    && (area.NextStage.Requirements.Value as List<GClass1309>).Any(genericRequirement =>
                {
                    ItemRequirement itemRequirement = genericRequirement as ItemRequirement;
                    if (itemRequirement == null) return false;
                    return itemRequirement.Type == ERequirementType.Item && /*!itemRequirement.Fulfilled &&*/ itemRequirement.TemplateId == item.TemplateId;
                });
            }).ToList();

            return areas;
        }

        public static bool IsNeededForHideoutUpgrades(Item item)
        {
            List<AreaData> data = GetApplicableUpgrades(item);
            return data != null && data.Count > 0;
        }

        public static string ToSentenceCase(this string text)
        {
            string result = text;
            try
            {
                // start by converting entire string to lower case
                var lowerCase = text.ToLower();
                // matches the first sentence of a string, as well as subsequent sentences
                var r = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
                // MatchEvaluator delegate defines replacement of setence starts to uppercase
                result = r.Replace(lowerCase, s => s.Value.ToUpper());
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error converting string case for '{text}': {ex}");
            }

            return result;
        }

        public static bool IsValidHexColor(this string inputColor)
        {
            //Taken from https://stackoverflow.com/a/13035186
            if (Regex.Match(inputColor, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success)
                return true;

            var result = System.Drawing.Color.FromName(inputColor);
            return result.IsKnownColor;
        }
    }
}
