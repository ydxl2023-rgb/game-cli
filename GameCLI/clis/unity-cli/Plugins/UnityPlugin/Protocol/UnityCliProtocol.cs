using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oathx.UnityCLI.Protocol
{
    /// <summary>
    /// Describes the active Unity Editor bridge endpoint written to Library/UnityCLI/endpoint.json.
    /// </summary>
    [Serializable]
    public sealed class CLIEndpoint
    {
        /// <summary>
        /// Transport name used when the bridge communicates through a local named pipe.
        /// </summary>
        public const string TransportNamedPipe = "named-pipe";

        /// <summary>
        /// Protocol version understood by the bridge.
        /// </summary>
        public string protocolVersion = string.Empty;
        /// <summary>
        /// Transport type used by this endpoint.
        /// </summary>
        public string transport = string.Empty;
        /// <summary>
        /// Named pipe identifier the CLI connects to.
        /// </summary>
        public string pipeName = string.Empty;
        /// <summary>
        /// Per-session token required by every request.
        /// </summary>
        public string token = string.Empty;
        /// <summary>
        /// Unity Editor process id that owns the endpoint.
        /// </summary>
        public int pid;
        /// <summary>
        /// Unity project root that created the endpoint.
        /// </summary>
        public string projectPath = string.Empty;
        /// <summary>
        /// UTC timestamp for when the endpoint was published.
        /// </summary>
        public string startedAtUtc = string.Empty;
    }

    /// <summary>
    /// Reads and writes length-prefixed UTF-8 JSON frames on a stream.
    /// </summary>
    public static class CLIPipeProtocol
    {
        /// <summary>
        /// Number of bytes used to encode the little-endian payload length.
        /// </summary>
        const int HeaderSize = 4;
        /// <summary>
        /// Maximum accepted JSON payload size.
        /// </summary>
        const int MaxFrameSize = 16 * 1024 * 1024;

        /// <summary>
        /// Reads a framed JSON payload asynchronously.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="cancellationToken">The token used to cancel the read.</param>
        /// <returns>A task that completes with the decoded JSON payload.</returns>
        public static async Task<string> ReadFrameAsync(Stream stream, CancellationToken cancellationToken)
        {
            var header = await ReadExactAsync(stream, HeaderSize, cancellationToken);
            var length = BitConverter.ToInt32(header, 0);
            if (length < 0 || length > MaxFrameSize)
            {
                throw new InvalidDataException("Invalid frame length.");
            }

            var payload = await ReadExactAsync(stream, length, cancellationToken);
            return Encoding.UTF8.GetString(payload);
        }

        /// <summary>
        /// Writes a framed JSON payload asynchronously.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="json">The JSON payload to encode.</param>
        /// <param name="cancellationToken">The token used to cancel the write.</param>
        /// <returns>A task that completes when the frame has been flushed.</returns>
        public static async Task WriteFrameAsync(Stream stream, string json, CancellationToken cancellationToken)
        {
            var payload = Encoding.UTF8.GetBytes(json ?? string.Empty);
            if (payload.Length > MaxFrameSize)
            {
                throw new InvalidDataException("Frame is too large.");
            }

            var header = BitConverter.GetBytes(payload.Length);
            await stream.WriteAsync(header, 0, header.Length, cancellationToken);
            await stream.WriteAsync(payload, 0, payload.Length, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Reads the requested number of bytes asynchronously.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The exact byte count to read.</param>
        /// <param name="cancellationToken">The token used to cancel the read.</param>
        /// <returns>A task that completes with the requested byte buffer.</returns>
        static async Task<byte[]> ReadExactAsync(Stream stream, int length, CancellationToken cancellationToken)
        {
            var buffer = new byte[length];
            var offset = 0;
            while (offset < length)
            {
                var read = await stream.ReadAsync(buffer, offset, length - offset, cancellationToken);
                if (read == 0)
                {
                    throw new EndOfStreamException("The pipe closed before a complete frame was read.");
                }

                offset += read;
            }

            return buffer;
        }
    }
}
