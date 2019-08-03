using GZipLib.Core;
using GZipLib.Job;

namespace GZipLib.Writer
{
    public interface IWriterJob : IJob
    {
        void Start(INextCheck nextCheck);
    }
}