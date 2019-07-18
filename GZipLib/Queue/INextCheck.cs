namespace GZipLib.Queue
{
    public interface INextCheck
    {
        bool IsNext(long position);
    }
}
