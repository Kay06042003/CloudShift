using System.IO;

namespace CloudShift.Application.Common.Interfaces;

public interface ICloudStorageProvider
{
    Task TransferFileAsync(
        string sourcePath,
        string destPath,
        Stream stream,
        CancellationToken cancellationToken = default);
}
