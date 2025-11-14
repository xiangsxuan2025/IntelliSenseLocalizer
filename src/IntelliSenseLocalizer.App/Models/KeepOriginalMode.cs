namespace IntelliSenseLocalizer.App.Models;

public class KeepOriginalMode(KeepEnglishMode mode, string display, string displayDescription)
{
    public static readonly KeepOriginalMode KeepEnglishAfter =
        new KeepOriginalMode(KeepEnglishMode.KeepEnglishAfter, "英文在后", "中文翻译显示在前，英文原文在后，便于快速阅读");

    public static readonly KeepOriginalMode KeepEnglishAhead =
        new KeepOriginalMode(KeepEnglishMode.KeepEnglishAhead, "英文在前", "英文内容显示在前，中文翻译在后，便于对照学习");

    public static readonly KeepOriginalMode DontKeepEnglish =
        new KeepOriginalMode(KeepEnglishMode.DontKeepEnglish, "不保留英文", "仅显示中文翻译内容，界面最简洁");

    public static readonly KeepOriginalMode[] Supported =
    [
        KeepEnglishAfter,
        KeepEnglishAhead,
        DontKeepEnglish
    ];

    public KeepEnglishMode Mode { get; set; } = mode;
    public string Display{ get; set; } = display;
    public string DisplayDescription { get; set; } = displayDescription;
}
