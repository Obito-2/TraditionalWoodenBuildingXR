using System.Text.RegularExpressions;

public static class MarkdownConverter
{
    public static string Convert(string markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return markdown;

        string result = markdown;

        // 标题（从大到小处理，避免 ## 被 # 误匹配）
        result = Regex.Replace(result, @"^### (.+)$", "<size=110%><b>$1</b></size>", RegexOptions.Multiline);
        result = Regex.Replace(result, @"^## (.+)$",  "<size=120%><b>$1</b></size>", RegexOptions.Multiline);
        result = Regex.Replace(result, @"^# (.+)$",   "<size=130%><b>$1</b></size>", RegexOptions.Multiline);

        // 加粗（** 优先于单 *）
        result = Regex.Replace(result, @"\*\*(.+?)\*\*", "<b>$1</b>");
        // 斜体
        result = Regex.Replace(result, @"\*(.+?)\*", "<i>$1</i>");

        // 无序列表：- / * / + 开头
        result = Regex.Replace(result, @"^[-*+] (.+)$", "• $1", RegexOptions.Multiline);

        // 分隔线（三个或更多 -）
        result = Regex.Replace(result, @"^-{3,}$", "──────────────────", RegexOptions.Multiline);

        // 内联代码：用浅色高亮显示
        result = Regex.Replace(result, @"`(.+?)`", "<color=#AAFFAA>$1</color>");

        return result;
    }
}
