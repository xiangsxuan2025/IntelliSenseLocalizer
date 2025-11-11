namespace IntelliSenseLocalizer.App;
internal class EnumHelpers
{

    public static string GetEnumMemberName(KeepEnglishMode status)
    {
        return status switch
        {
            KeepEnglishMode.KeepEnglishAhead => "OriginFirst",
            KeepEnglishMode.KeepEnglishAfter => "LocaleFirst",
            KeepEnglishMode.DontKeepEnglish => "None",
            _ => throw new NotImplementedException(),
        };
    }
}
