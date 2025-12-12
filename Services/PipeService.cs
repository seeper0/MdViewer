using System.IO;
using System.IO.Pipes;

namespace MdViewer.Services
{
    public class PipeService : IDisposable
    {
        private const string PipeName = "MdViewer_Pipe";
        private NamedPipeServerStream? _server;
        private CancellationTokenSource? _cts;
        private bool _disposed;

        public event Action<string>? FileRequested;

        public void StartServer()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => ListenAsync(_cts.Token));
        }

        private async Task ListenAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    await _server.WaitForConnectionAsync(token);

                    using var reader = new StreamReader(_server);
                    var filePath = await reader.ReadLineAsync(token);

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        FileRequested?.Invoke(filePath);
                    }

                    _server.Disconnect();
                    _server.Dispose();
                    _server = null;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // 연결 오류 시 재시도
                    await Task.Delay(100, token);
                }
            }
        }

        public static bool TrySendToExistingInstance(string filePath)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(500); // 500ms 타임아웃

                using var writer = new StreamWriter(client) { AutoFlush = true };
                writer.WriteLine(filePath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cts?.Cancel();
            _cts?.Dispose();
            _server?.Dispose();
        }
    }
}
