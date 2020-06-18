namespace DemoServer.Services {

    /// <summary>
    /// A simple dependency-injected demo service, that is called within a RPC server function.
    /// </summary>
    public class DemoService {

        public string CallService(string message) =>
            "It works: " + message;

    }

}
