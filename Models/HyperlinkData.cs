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
        public string ElementId { get; set; } = string.Empty;  // Reference to the hyperlink element in the document

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
                Status = Status,
                ElementId = ElementId
            };
        }

        /// <summary>
        /// Determines whether this instance equals another HyperlinkData instance
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is not HyperlinkData other)
                return false;

            return Address == other.Address &&
                   SubAddress == other.SubAddress &&
                   TextToDisplay == other.TextToDisplay &&
                   PageNumber == other.PageNumber &&
                   LineNumber == other.LineNumber &&
                   OriginalText == other.OriginalText &&
                   Title == other.Title &&
                   ContentID == other.ContentID &&
                   Status == other.Status &&
                   ElementId == other.ElementId;
        }

        /// <summary>
        /// Returns the hash code for this instance
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                HashCode.Combine(Address, SubAddress, TextToDisplay, PageNumber),
                HashCode.Combine(LineNumber, OriginalText, Title, ContentID),
                HashCode.Combine(Status, ElementId)
            );
        }
    }
}