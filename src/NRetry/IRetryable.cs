namespace NRetry {
    public interface IRetryable<T> {
        T Attempt();
        void Recover();
    }
}