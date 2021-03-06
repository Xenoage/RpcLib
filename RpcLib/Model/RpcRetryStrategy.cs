﻿namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// If and how to retry RPC calls which could not be finished because of an
    /// <see cref="RpcFailure.IsRetryable"/> failure.
    /// </summary>
    public enum RpcRetryStrategy {

        /// <summary>
        /// When the timeout is hit, the RPC call returns a timeout failure
        /// and is not tried automatically again. This is the default behavior.
        /// </summary>
        None,

        /// <summary>
        /// Retries the RPC call as soon as the other peer is online again.
        /// When N methods with this flag are called, all N calls will be
        /// executed later in exactly this order.
        /// 
        /// Example use case: A method to add or remove credit (when adding 5, 10
        /// and 30 ct, at the very end the other peer should have received all 45 ct).
        /// 
        /// Notice, that if methods with this flag have a return value, this return value
        /// will only be received by the caller when the call works at the first attempt.
        /// When retried later, though the remote peer will also return the result,
        /// there is no calling context any more which could receive it.
        /// </summary>
        Retry,

        /// <summary>
        /// Retries the RPC call as soon as the other peer is online again,
        /// but only the newest call with the same method name is retried.
        /// 
        /// Example use case: A method to set a configuration file (when first setting
        /// the name to "MyFirstName", then to "MySecondName" and finally to "MyThirdName",
        /// only "MyThirdName" will be sent to the other peer as soon it is online again).
        /// 
        /// Notice, that if methods with this flag have a return value, the same is
        /// true for this return value as documented in <see cref="Retry"/>.
        /// 
        /// Also notice, that the previous calls of the same method will only
        /// be removed in the backlog (permanent storage), not in the real queue (in memory).
        /// Otherwise, it could happen that we never send this method, when it is
        /// frequently and repeatedly called, but is always set back to the end
        /// of the queue instead of being executed.
        /// </summary>
        RetryLatest
    }

}
