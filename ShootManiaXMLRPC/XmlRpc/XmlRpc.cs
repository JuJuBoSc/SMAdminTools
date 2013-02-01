
//---------------------------------------------------//
//
//			Credits : Flo
//
//---------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace ShootManiaXMLRPC.XmlRpc
{
    public class XmlRpc
    {
        private static bool SendRpc(Socket in_socket, byte[] in_data)
        {
            int offset = 0;
            int len = in_data.Length;
            int bytesSent;
            try
            {
                while (len > 0)
                {
                    bytesSent = in_socket.Send(in_data, offset, len, SocketFlags.None);
                    len -= bytesSent;
                    offset += bytesSent;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] ReceiveRpc(Socket in_socket, int in_length)
        {
            byte[] data = new byte[in_length];
            int offset = 0;
            byte[] buffer;
            while (in_length > 0)
            {
                int read = Math.Min(in_length, 1024);
                buffer = new byte[read];
                int bytesRead = in_socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                Array.Copy(buffer, 0, data, offset, buffer.Length);
                in_length -= bytesRead;
                offset += bytesRead;
            }
            return data;
        }

        public static int SendCall(Socket in_socket, GbxCall in_call)
        {
            if (in_socket.Connected)
            {
                lock (in_socket)
                {
                    try
                    {
                        // create request body ...
                        byte[] body = Encoding.UTF8.GetBytes(in_call.Xml);

                        // create response header ...
                        byte[] bSize = BitConverter.GetBytes(body.Length);
                        byte[] bHandle = BitConverter.GetBytes(in_call.Handle);

                        // create call data ...
                        byte[] call = new byte[bSize.Length + bHandle.Length + body.Length];
                        Array.Copy(bSize, 0, call, 0, bSize.Length);
                        Array.Copy(bHandle, 0, call, 4, bHandle.Length);
                        Array.Copy(body, 0, call, 8, body.Length);

                        // send call ...
                        in_socket.Send(call);

                        return in_call.Handle;
                    }
                    catch
                    {
                        return 0;
                    }
                }
            }
            else
            {
                throw new NotConnectedException();
            }
        }

        public static GbxCall ReceiveCall(Socket in_socket, byte[] inHeader)
        {
            if (in_socket.Connected)
            {
                lock (in_socket)
                {
                    // read response size and handle ...
                    byte[] bSize = new byte[4];
                    byte[] bHandle = new byte[4];
                    if (inHeader == null)
                    {
                        in_socket.Receive(bSize);
                        in_socket.Receive(bHandle);
                    }
                    else
                    {
                        Array.Copy(inHeader, 0, bSize, 0, 4);
                        Array.Copy(inHeader, 4, bHandle, 0, 4);
                    }
                    int size = BitConverter.ToInt32(bSize, 0);
                    int handle = BitConverter.ToInt32(bHandle, 0);

                    // receive response body ...
                    byte[] data = ReceiveRpc(in_socket, size);

                    // parse the response ...
                    GbxCall call = new GbxCall(handle, data);

                    return call;
                }
            }
            else
            {
                throw new NotConnectedException();
            }
        }
    }
}
