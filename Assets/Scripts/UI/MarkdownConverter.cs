using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Markdown 内容块：文本或图片，按序渲染实现图文混排
/// </summary>
public class MarkdownBlock
{
    public enum BlockType { Text, Image }

    public BlockType Type;
    public string Content;   // Text 块：Markdown 文本；Image 块：alt 文本（标题）
    public string ImageUrl;  // Image 块专用
}

public static class MarkdownConverter
{
    /// <summary>
    /// 将 Markdown 字符串转为 TMP Rich Text（不含图片）
    /// </summary>
    public static string Convert(string markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return markdown;

        string result = markdown;

        // 表格内的图片语法 → 文本占位（在表格格式化之前处理）
        result = Regex.Replace(result, @"!\[([^\]]*)\]\(([^)]+)\)", "[图片: $1]");

        // Markdown 表格 → 对齐文本
        result = FormatTables(result);

        // 标题：以 DefaultFontSize(5) 为基准按比例放大
        result = Regex.Replace(result, @"^### (.+)$", "<size=6><b>$1</b></size>",   RegexOptions.Multiline);
        result = Regex.Replace(result, @"^## (.+)$",  "<size=6.5><b>$1</b></size>", RegexOptions.Multiline);
        result = Regex.Replace(result, @"^# (.+)$",   "<size=7><b>$1</b></size>",   RegexOptions.Multiline);

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

    /// <summary>
    /// 将 Markdown 表格转为对齐的纯文本
    /// </summary>
    private static string FormatTables(string text)
    {
        // 匹配表格块：表头行 + 分隔行 + 若干数据行
        var tableRegex = new Regex(
            @"^(\|.+\|)[ \t]*\r?\n" +           // 表头
            @"(\|[ \t]*:?[-]+:?[ \t]*\|[ \t]*(?:\r?\n|$))" + // 分隔行（含对齐标记 :---）
            @"((?:^(\|.+\|)[ \t]*(?:\r?\n|$))*)",           // 数据行
            RegexOptions.Multiline);

        return tableRegex.Replace(text, match =>
        {
            // 收集所有行：表头 + 跳过对齐分隔行 + 数据行
            var rows = new List<string[]>
            {
                SplitTableRow(match.Groups[1].Value)
            };

            // 数据行
            string dataBlock = match.Groups[3].Value.Trim();
            if (!string.IsNullOrEmpty(dataBlock))
            {
                foreach (string dataLine in dataBlock.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = dataLine.Trim();
                    if (trimmed.StartsWith("|"))
                        rows.Add(SplitTableRow(trimmed));
                }
            }

            if (rows.Count == 0) return match.Value;

            // 计算每列最大宽度
            int colCount = rows.Max(r => r.Length);
            int[] colWidths = new int[colCount];
            foreach (var row in rows)
            {
                for (int i = 0; i < row.Length && i < colCount; i++)
                    colWidths[i] = Math.Max(colWidths[i], row[i].Length);
            }

            // 构建输出
            var sb = new System.Text.StringBuilder();
            for (int r = 0; r < rows.Count; r++)
            {
                sb.Append("  ");  // 缩进
                for (int i = 0; i < rows[r].Length && i < colCount; i++)
                {
                    if (i > 0) sb.Append("  ");
                    sb.Append(rows[r][i].PadRight(colWidths[i]));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        });
    }

    /// <summary>
    /// 拆分表格行，去掉首尾 | 并按 | 分割，trim 每个单元格
    /// </summary>
    private static string[] SplitTableRow(string row)
    {
        string trimmed = row.Trim();
        if (trimmed.StartsWith("|")) trimmed = trimmed.Substring(1);
        if (trimmed.EndsWith("|")) trimmed = trimmed.Substring(0, trimmed.Length - 1);
        return trimmed.Split('|').Select(c => c.Trim()).ToArray();
    }

    /// <summary>
    /// 将 Markdown 解析为内容块列表，按 ![title](url) 语法拆分出图文块
    /// 表格行内的图片语法保留在文本中，不提取为独立 ImageBlock
    /// </summary>
    public static List<MarkdownBlock> ParseBlocks(string markdown)
    {
        var blocks = new List<MarkdownBlock>();
        if (string.IsNullOrEmpty(markdown)) return blocks;

        var imageRegex = new Regex(@"!\[([^\]]*)\]\(([^)]+)\)");
        int lastIndex = 0;

        foreach (Match match in imageRegex.Matches(markdown))
        {
            // 判断该图片是否在表格行内（该行包含 | 分隔符）
            bool isInTableRow = IsInTableRow(markdown, match.Index);

            if (isInTableRow)
            {
                // 表格内图片不提取，跳过（保留在后续文本中）
                continue;
            }

            // 图片之前的文本
            if (match.Index > lastIndex)
            {
                string textBefore = markdown.Substring(lastIndex, match.Index - lastIndex).TrimEnd();
                if (!string.IsNullOrEmpty(textBefore))
                    blocks.Add(new MarkdownBlock { Type = MarkdownBlock.BlockType.Text, Content = textBefore });
            }

            // 图片块
            blocks.Add(new MarkdownBlock
            {
                Type = MarkdownBlock.BlockType.Image,
                Content = match.Groups[1].Value,
                ImageUrl = match.Groups[2].Value
            });

            lastIndex = match.Index + match.Length;
        }

        // 最后一段文本（余下的 ![alt](url) 语法均为表格内图片，保留由 Convert() 处理）
        if (lastIndex < markdown.Length)
        {
            string textAfter = markdown.Substring(lastIndex).TrimStart();
            if (!string.IsNullOrEmpty(textAfter))
                blocks.Add(new MarkdownBlock { Type = MarkdownBlock.BlockType.Text, Content = textAfter });
        }

        return blocks;
    }

    /// <summary>
    /// 判断指定位置是否在表格行内（该行以 | 开头或中间含 | 分隔符）
    /// </summary>
    private static bool IsInTableRow(string text, int index)
    {
        // 找到 index 所在行的行首
        int lineStart = text.LastIndexOf('\n', index);
        if (lineStart < 0) lineStart = 0; else lineStart++;
        // 找到行尾
        int lineEnd = text.IndexOf('\n', index);
        if (lineEnd < 0) lineEnd = text.Length;

        string line = text.Substring(lineStart, lineEnd - lineStart).Trim();
        // 表格行以 | 开头且在非首位置还有至少一个 |
        return line.StartsWith("|") && line.IndexOf('|', 1) > 0;
    }
}
