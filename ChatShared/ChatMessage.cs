namespace Chat {

    /// <summary>
    /// Chat message, consisting of text content and the username of the sender.
    /// </summary>
    public class ChatMessage {
        public string Text { get; set; } = "";
        public string Sender { get; set; } = "";
    }

}
