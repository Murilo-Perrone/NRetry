namespace NRetry {
    /// <summary>Additional method used by ProcessRetryer to detect operation failures.</summary>
    public enum FailureDetection {
        None,
        ByReturnValue,
        ByFailureDetector,
    }
}