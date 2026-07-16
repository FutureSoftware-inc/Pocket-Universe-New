namespace Crystal.Common
{
    public interface IRefProvider<out TRefType> where TRefType : class
    {
        UnityEngine.Object Host { get; }
        long ID { get; }
        bool IsValid { get; }
        TRefType GetRef();
    }
}