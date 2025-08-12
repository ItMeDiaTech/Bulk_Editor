using Bulk_Editor.Services;

namespace Bulk_Editor.Models
{
    public class Theme
    {
        public string Name { get; set; } = string.Empty;
        public ThemeConfiguration Colors { get; set; } = new();
    }
}