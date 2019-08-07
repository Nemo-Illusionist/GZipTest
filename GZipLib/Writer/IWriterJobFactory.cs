using System.IO.Compression;
using GZipLib.Core;
using GZipLib.Job;

namespace GZipLib.Writer
{
    public interface IWriterJobFactory
    {
        IJob Create(IWriterQueue queue, INextCheck nextCheck, CompressionMode mode);
    }
}