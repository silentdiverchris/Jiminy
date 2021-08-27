using System.Text;

namespace Jiminy.Classes
{
    internal class HtmlTableBase
    {
        internal string? Classes { get; set; }
        internal string? Styles { get; set; }

        internal static string? BuildClass(string? classes)
        {
            return string.IsNullOrEmpty(classes)
                ? null
                : $" class='{classes}'";
        }

        internal static string? BuildStyle(string? styles)
        {
            return string.IsNullOrEmpty(styles)
                ? null
                : $" style='{styles}'";
        }
    }

    internal class HtmlTableCell : HtmlTableBase
    {
        public HtmlTableCell(string text)
        {
            Text = text;
        }

        public HtmlTableCell(string? text = null, string? classes = null, string? styles = null, string? linkUrl = null, string? title = null, bool isHeader = false)
        {
            Text = text;
            Classes = classes;
            Styles = styles;
            LinkUrl = linkUrl;
            Title = title;
            IsHeader = isHeader;
        }

        internal string? Text { get; set; }
        internal string? Title { get; set; }
        internal string? LinkUrl { get; set; }
        internal bool IsHeader { get; set; }

        internal string GeneratedHtml
        {
            get
            {
                string? contentHtml = string.IsNullOrEmpty(LinkUrl)
                    ? Text
                    : $"<a href='{LinkUrl}'>{Text}</a>";

                string? titleHtml = string.IsNullOrEmpty(Title)
                    ? null
                    : $" title='{Title}'";

                string el = IsHeader ? "th" : "td";

                return $"<{el}{BuildClass(Classes)}{BuildStyle(Styles)}{titleHtml}>{contentHtml}</{el}>";
            }
        }
    }

    internal class HtmlTableRow : HtmlTableBase
    {
        internal bool IsHeader { get; set; }
        internal List<HtmlTableCell> Cells { get; set; } = new();

        internal string GeneratedHtml
        {
            get
            {
                StringBuilder sb = new(1000);

                foreach (var cell in Cells)
                {
                    sb.Append(cell.GeneratedHtml);
                }

                return $"<tr{BuildClass(Classes)}{BuildStyle(Styles)}>{sb}</tr>";
            }
        }
    }

    internal class HtmlTable : HtmlTableBase
    {
        internal string? Title { get; set; }
        internal string? TitleElement { get; set; } = "h3";
        internal string? SubTitle { get; set; }
        internal string? SubTitleElement { get; set; } = "p";
        internal int DisplayOrder { get; set; } = 0;

        internal List<HtmlTableRow> Rows { get; set; } = new();

        internal string GeneratedHtml
        {
            get
            {
                StringBuilder sb = new(2000);

                var headerRows = Rows.Where(_ => _.IsHeader == true);
                var bodyRows = Rows.Where(_ => _.IsHeader == false);

                if (Title is not null)
                {
                    sb.Append($"<{TitleElement}>{Title}</{TitleElement}>");
                }

                if (SubTitle is not null)
                {
                    sb.Append($"<{SubTitleElement}>{SubTitle}</{SubTitleElement}>");
                }

                sb.Append($"<table{BuildClass(Classes)}{BuildStyle(Styles)}>");

                if (headerRows.Any())
                {
                    sb.Append($"<thead>");

                    foreach (var row in headerRows)
                    {
                        sb.Append(row.GeneratedHtml);
                    }

                    sb.Append($"</thead>");
                }

                if (bodyRows.Any())
                {
                    sb.Append($"<tbody>");

                    foreach (var row in Rows.Where(_ => _.IsHeader == false))
                    {
                        sb.Append(row.GeneratedHtml);
                    }

                    sb.Append($"</tbody>");
                }

                sb.Append($"</table>");

                return sb.ToString();
            }
        }
    }

}
