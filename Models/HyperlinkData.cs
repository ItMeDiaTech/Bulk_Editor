using System;

namespace Bulk_Editor.Models
{
    /// <summary>
    /// Represents hyperlink data extracted from a Word document
    /// </summary>
    public class HyperlinkData
    {
        public string Address { get; set; } = string.Empty;
        public string SubAddress { get; set; } = string.Empty;
        public string TextToDisplay { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int LineNumber { get; set; }
        public string OriginalText { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ContentID { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Creates a deep copy of the hyperlink data
        /// </summary>
        public HyperlinkData Clone()
        {
            return new HyperlinkData
            {
                Address = Address,
                SubAddress = SubAddress,
                TextToDisplay = TextToDisplay,
                PageNumber = PageNumber,
                LineNumber = LineNumber,
                OriginalText = OriginalText,
                Title = Title,
                ContentID = ContentID,
                Status = Status
            };
        }
    }
}