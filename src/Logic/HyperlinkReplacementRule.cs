using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulk_Editor
{
    public class HyperlinkReplacementRule
    {
        public string OldTitle { get; set; } = string.Empty;
        public string NewTitle { get; set; } = string.Empty;
        public string NewFullContentID { get; set; } = string.Empty;
    }
}