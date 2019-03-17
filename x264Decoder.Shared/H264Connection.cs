using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace x264Decoder {

    public class H264Connection {

        private TcpClient client;
        private string host;
        private int port;
        private Task connectionTask;
        private IH264Parser parser;
        private bool isRunning;
        private const int MAX_ERROR = 10;
        private int errorCount = 0;

        /// <summary>
        /// Time in s before a pending connection dies.
        /// </summary>
        public int Timeout = 10;

        public bool IsRunning { get { return isRunning; } }

        public delegate void H264Error(Exception ex);

        /// <summary>
        /// Called (fromb a background thread) whenever a fatal error has occured.
        /// </summary>
        public H264Error OnError;

        public H264Connection(string host, int port, IH264Parser parser) {

            this.host = host;
            this.port = port;
            this.parser = parser;

        }

        public void Start() {

            client = new TcpClient();
            client.ReceiveTimeout = Timeout * 1000;

            isRunning = true;
            connectionTask = Task.Factory.StartNew(() => {

                try {

                    client.Connect(host, port);
                    Byte[] data = new Byte[256];

                    while (IsRunning) {

                        Int32 length = client.GetStream().Read(data, 0, data.Length);

                        if (length > 0) {

                            if (!parser.Parse(data, length) && errorCount++ % MAX_ERROR == 0) {

                                Console.WriteLine($"** AVC ERROR ** Got to many errors: {errorCount}. Closing connection.");
                                OnError(new H264Exception($"** AVC ERROR ** Got to many errors: {errorCount}. Closing connection."));
                                Stop();
                                return;
                   
                            }

                        }

                    }

                } catch (Exception ex)  {

                    Console.WriteLine($"** AVC ERROR ** Exception thrown during parsing/decodinf: {ex.Message}.");
                    OnError(new H264Exception(ex));

                }

                Stop();

            });

        }

        public void Stop() {

            isRunning = false;

            parser.Reset();

            if (client?.Connected == true) { client.Close(); }

            client = null;

        }

    }

}
