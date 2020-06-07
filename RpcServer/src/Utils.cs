using System;

namespace RpcServer {

    public class Utils {

        public static long TimeNow() =>
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    }
}
