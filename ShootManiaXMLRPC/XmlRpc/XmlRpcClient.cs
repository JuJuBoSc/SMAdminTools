
//---------------------------------------------------//
//
//			Credits : Flo
//
//---------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace ShootManiaXMLRPC.XmlRpc
{
    public class XmlRpcClient
    {
        private bool m_connected;
        private int m_port;
        private string m_address;
        private Socket tcpSocket;
        private int requests;
        private byte[] m_buffer;
        private IAsyncResult asyncResult;
        private AutoResetEvent callRead = new AutoResetEvent(false);
        private Hashtable responses = new Hashtable();
        public event GbxCallbackHandler EventGbxCallback;
        public event OnDisconnectHandler EventOnDisconnectCallback;
        private Hashtable callbackList = new Hashtable();

        private void OnDisconnectCallback()
        {
            if (EventOnDisconnectCallback != null)
                EventOnDisconnectCallback(this);
        }

        private void OnGbxCallback(GbxCallbackEventArgs e)
        {
            if (EventGbxCallback != null)
                EventGbxCallback(this, e);
        }

        /// <summary>
        /// Frees all resources and disconnects from the rpc server.
        /// </summary>
        public void Dispose()
        {
            tcpSocket.Close();
            EventGbxCallback = null;
            EventOnDisconnectCallback = null;
        }

        /// <summary>
        /// Initializes the client with port and ip address.
        /// Use Connect method to set up a connection.
        /// </summary>
        /// <param name="inAddress">IP address to initialize with.</param>
        /// <param name="inPort">Port to initialize the client with.</param>
        public XmlRpcClient(string inAddress, int inPort)
        {
            this.m_address = inAddress;
            this.m_port = inPort;
            this.m_connected = false;
        }

        public bool IsConnected
        {
            get
            {
                return (m_connected && tcpSocket.Connected);
            }
        }

        /// <summary>
        /// Creates a connection to the server.
        /// </summary>
        /// <returns>0:success, 1:wrong socket, 2:wrong protocol version</returns>
        public int Connect()
        {
            // try to connect on the socket ...
            if (!this.SocketConnect(this.m_address, this.m_port))
            {
                this.m_connected = false;
                return 1;
            }

            if (!this.Handshake())
            {
                this.m_connected = false;
                return 2;
            }

            m_buffer = new byte[8];
            asyncResult = tcpSocket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, new AsyncCallback(OnDataArrive), null);
            this.m_connected = true;
            return 0;
        }

        /// <summary>
        /// Establishes a socket connection.
        /// </summary>
        private bool SocketConnect(string inAddress, int inPort)
        {
            // create end point ...
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(inAddress), inPort);

            try
            {
                // create a socket for the remote end point ...
                tcpSocket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // connect onto the remote end point ...
                tcpSocket.Connect(remoteEndPoint);

                // connection established successfully!
                return true;
            }
            catch
            {
                // could not connect!
                this.m_connected = false;
                return false;
            }
        }

        /// <summary>
        /// Reads the header of the protocol and checks if for compability.
        /// </summary>
        /// <returns>Returns whether a connection is possible.</returns>
        private bool Handshake()
        {
            // are we connected already?
            if (tcpSocket.Connected)
            {
                // get size ...
                byte[] Buffer = new byte[4];
                tcpSocket.Receive(Buffer);
                int Size = System.BitConverter.ToInt32(Buffer, 0);

                // get handshake ...
                byte[] HandshakeBuffer = new byte[Size];
                tcpSocket.Receive(HandshakeBuffer);
                string Handshake = Encoding.UTF8.GetString(HandshakeBuffer);

                // check if compatible ...
                if (Handshake != "GBXRemote 2")
                    return false;
                else
                    return true;
            }
            else
            {
                throw new NotConnectedException();
            }
        }

        /// <summary>
        /// Closes the socket connection.
        /// </summary>
        public void Disconnect()
        {
            this.tcpSocket.Close();
        }

        private void OnDataArrive(IAsyncResult iar)
        {

            // end receiving and check if connection's still alive ...
            try
            {
                tcpSocket.EndReceive(iar);

                // receive the message from the server ...
                GbxCall call = XmlRpc.ReceiveCall(this.tcpSocket, m_buffer);

                // watch out for the next calls ...
                m_buffer = new byte[8];
                asyncResult = tcpSocket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, new AsyncCallback(OnDataArrive), null);

                if (call.Type == MessageTypes.Callback)
                {

                    // throw new event ...
                    GbxCallbackEventArgs eArgs = new GbxCallbackEventArgs(call);
                    OnGbxCallback(eArgs);
                }
                else
                {

                    // add the response to the queue ...
                    lock (this)
                    {
                        responses.Add(call.Handle, call);
                    }

                    // callback if any method was set ...
                    if (callbackList[call.Handle] != null)
                    {
                        ((GbxCallCallbackHandler)callbackList[call.Handle]).BeginInvoke(call, null, null);
                        callbackList.Remove(call.Handle);
                    }
                }
            }
            catch
            {

                this.m_connected = false;

                // something went wrong :S
                tcpSocket.Close();

                // release a disconnect event ...
                OnDisconnectCallback();
            }
            finally
            {
                // we received something :)
                callRead.Set();
            }
        }

        /// <summary>
        /// (Dis)activates callbacks from the server.
        /// </summary>
        /// <param name="inState">Whether to receive callbacks from the server.</param>
        /// <returns>Returns new callback state</returns>
        public bool EnableCallbacks(bool inState)
        {
            GbxCall EnableCall = new GbxCall("EnableCallbacks", new object[] { inState });
            EnableCall.Handle = --this.requests;
            return (XmlRpc.SendCall(this.tcpSocket, EnableCall) != 0);
        }

        /// <summary>
        /// Sends a request to the server and blocks until a response has been received.
        /// </summary>
        /// <param name="inMethodName">The method to call.</param>
        /// <param name="inParams">Parameters describing your request.</param>
        /// <returns>Returns a response object from the server.</returns>
        public GbxCall Request(string inMethodName, object[] inParams)
        {
            // reset event ...
            callRead.Reset();

            // send the call and remember the handle we are waiting on ...
            GbxCall Request = new GbxCall(inMethodName, inParams);
            Request.Handle = --this.requests;
            int handle = XmlRpc.SendCall(this.tcpSocket, Request);

            // wait until we received the call ...
            do
            {
                callRead.WaitOne();
            } while (responses[handle] == null && tcpSocket.Connected);

            // did we get disconnected ?
            if (!tcpSocket.Connected)
                throw new NotConnectedException();

            // get the call and return it ...
            return GetResponse(handle);
        }

        /// <summary>
        /// Sends a Request and does not wait for a response of the server.
        /// The response will be written into a buffer or you can set a callback method
        /// that will be executed.
        /// </summary>
        /// <param name="inMethodName">The method to call.</param>
        /// <param name="inParams">Parameters describing your request.</param>
        /// <param name="callbackHandler">An optional delegate which is callen when the response is available otherwise set it to null.</param>
        /// <returns>Returns a handle to your request.</returns>
        public int AsyncRequest(string inMethodName, object[] inParams, GbxCallCallbackHandler callbackHandler)
        {
            // send the call and remember the handle ...
            GbxCall Request = new GbxCall(inMethodName, inParams);
            Request.Handle = --this.requests;
            int handle = XmlRpc.SendCall(this.tcpSocket, Request);

            lock (this)
            {
                if (handle != 0)
                {
                    // register a callback on this request ...
                    if (callbackHandler != null)
                    {

                        callbackList.Add(handle, callbackHandler);
                    }

                    // return handle id ...
                    return handle;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets an asynchron response from the list.
        /// </summary>
        /// <param name="inHandle">The handle which was returned from AsyncRequest.</param>
        /// <returns>Returns the cached response.</returns>
        public GbxCall GetResponse(int inHandle)
        {
            return (GbxCall)responses[inHandle];
        }

        public string IP
        {
            get
            {
                return this.m_address;
            }
            set
            {
                this.m_address = value;
            }
        }

        public int Port
        {
            get
            {
                return this.m_port;
            }
            set
            {
                this.m_port = value;
            }
        }
    }

    public delegate void GbxCallbackHandler(object o, GbxCallbackEventArgs e);
    public delegate void OnDisconnectHandler(object o);
    public delegate void GbxCallCallbackHandler(GbxCall res);

    public class NotConnectedException : Exception
    {
        public NotConnectedException()
        {
            // exception is enough info ...
        }
    }

    public class GbxCallbackEventArgs : EventArgs
    {
        public readonly GbxCall Response;

        public GbxCallbackEventArgs(GbxCall response)
        {
            Response = response;
        }
    }
}
