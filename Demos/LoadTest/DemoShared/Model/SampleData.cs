using System.Collections.Generic;

namespace DemoShared.Model {

    /// <summary>
    /// Demo model: Some more data.
    /// </summary>
    public class SampleData {
        public string Text { get; set; } = "";
        public int Number { get; set; } = 0;
        public List<string> List { get; set; } = new List<string>();
        public string? NullableText { get; set; } = null;
    }

}
