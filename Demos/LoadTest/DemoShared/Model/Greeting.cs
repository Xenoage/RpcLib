namespace DemoShared.Model {

    /// <summary>
    /// Demo model: Data for a greeting, consisting of a name
    /// and optionally some more data.
    /// </summary>
    public class Greeting {
        public string Name { get; set; } = "Mathilda";
        public SampleData? MoreData { get; set; } = null;
    }

}
