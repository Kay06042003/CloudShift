using CloudShift.Application.Common.Interfaces;

namespace CloudShift.Worker.Storage;

public sealed class NoopCloudStorageProvider : ICloudStorageProvider
{
    public async Task TransferFileAsync(
        string sourcePath,
        string destPath,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        // Placeholder for Google Drive -> Microsoft Graph SDK stream transfer.
        // The real implementation must copy the source SDK stream directly into
        // the destination upload stream/session without buffering the full file.
        await stream.CopyToAsync(Stream.Null, cancellationToken);
    }
}
