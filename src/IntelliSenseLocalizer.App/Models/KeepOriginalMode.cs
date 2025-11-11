namespace IntelliSenseLocalizer.App.Models;
public class KeepOriginalMode(KeepEnglishMode mode, string description)
{
    public static readonly KeepOriginalMode KeepEnglishAfter =
            new KeepOriginalMode(KeepEnglishMode.KeepEnglishAfter, "在后");
    public static readonly KeepOriginalMode KeepEnglishAhead =
            new KeepOriginalMode(KeepEnglishMode.KeepEnglishAhead, "在前");
    public static readonly KeepOriginalMode DontKeepEnglish =
            new KeepOriginalMode(KeepEnglishMode.DontKeepEnglish, "不保留");

    public static readonly KeepOriginalMode[] Supported =
    [
        KeepEnglishAfter,
        KeepEnglishAhead,
        DontKeepEnglish
        ];

    public KeepEnglishMode Mode { get; set; } = mode;
    public string Desc { get; set; } = description;
}
