namespace DemoServer.Services {

    /// <summary>
    /// A simple dependency-injectable demo service,
    /// for testing within a RPC server function.
    /// </summary>
    public class DemoService {

        public string CallService(string message) =>
            "It works: " + message;

    }

}
