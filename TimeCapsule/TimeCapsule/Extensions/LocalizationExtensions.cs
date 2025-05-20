using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;

namespace TimeCapsule.Extensions
{
    public static class LocalizationExtensions
    {
        public static IHtmlContent GetPolishYearForm(this IViewLocalizer localizer, int count)
        {
            if (count == 1)
                return localizer["Year_1"];
            else if (count >= 2 && count <= 4)
                return localizer["Year_2_4"];
            else
                return localizer["Year_5_"];
        }

        public static IHtmlContent GetPolishHourForm(this IViewLocalizer localizer, int count)
        {
            if (count == 1)
                return localizer["Hour_1"];
            else if (count >= 2 && count <= 4)
                return localizer["Hour_2_4"];
            else
                return localizer["Hour_5_"];
        }

        public static IHtmlContent GetPolishMinuteForm(this IViewLocalizer localizer, int count)
        {
            if (count == 1)
                return localizer["Minute_1"];
            else if (count >= 2 && count <= 4)
                return localizer["Minute_2_4"];
            else
                return localizer["Minute_5_"];
        }
    }
}