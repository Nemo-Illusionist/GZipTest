using GZipLib.Core;
using GZipLib.Job;

namespace GZipLib.Reader
{
    public interface IReaderJob : IJob, INextCheck
    {
        void Start();
    }
}