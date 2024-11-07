using System;
using System.Text;
using UnityEngine;

namespace Packets
{

    /// <summary>
    /// 
    /// </summary>
    public enum Types : UInt16
    {
        Invalid = 0,
        InitSession,
        PingPong,
        MovePawn,
    }

    public static class Encoding
    {
        public static bool GetHeaderFromBytes(Byte[] bytes, out Header header)
        {
            try
            {
                header.pktId = (Packets.Types)BitConverter.ToUInt16(bytes, Header.PACKET_HEADER_SIZE - 2);
                header.sessionId = BitConverter.ToUInt16(bytes, Header.PACKET_HEADER_SIZE - 4);
                header.pktSize = BitConverter.ToUInt16(bytes, Header.PACKET_HEADER_SIZE - 6);

                return true;
            }
            catch (Exception e)
            {
                header = new Header();

                Debug.LogError($"Failed to parse packet header !! => {e.ToString()}");
                return false;
            }
        }

        public static T GetPacketFromBytes<T>(Byte[] bytes, ref Header header)
        {
            return JsonUtility.FromJson<T>(System.Text.Encoding.UTF8.GetString(bytes, Header.PACKET_HEADER_SIZE, header.pktSize));
        }

        public static int ToBytes(UInt16 sessionId, Types pktId, Body body, Byte[] outBytes)
        {
            try
            {
                var bodyBytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(body));
                bodyBytes.CopyTo(outBytes, Header.PACKET_HEADER_SIZE);
                
                BitConverter.GetBytes((UInt16)pktId).CopyTo(outBytes, Header.PACKET_HEADER_SIZE - 2);
                BitConverter.GetBytes(sessionId).CopyTo(outBytes, Header.PACKET_HEADER_SIZE - 4);
                BitConverter.GetBytes((UInt16)(bodyBytes.Length + Header.PACKET_HEADER_SIZE)).CopyTo(outBytes, Header.PACKET_HEADER_SIZE - 6);

                return bodyBytes.Length + Header.PACKET_HEADER_SIZE;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convent packet header to bytes !! => {e.ToString()}");
                return 0;
            }
        }
    }


    [System.Serializable]
    public struct Header
    {
        public const int MAX_FRAME_SIZE = 512;
        public const int PACKET_HEADER_SIZE = 6;
        public UInt16 pktSize;
        public UInt16 sessionId;
        public Types pktId;

        // Ctor.
        public Header(Packets.Types pktId, UInt16 sessionId, UInt16 pktSize = 0)
        {
            this.pktSize = pktSize;
            this.sessionId = sessionId;
            this.pktId = pktId;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public abstract class Body
    {
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class InitSesssion : Body
    {
        /// <summary>
        /// 
        /// </summary>
        public UInt16 sessionId;
    }


    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class PingPong : Body
    {
        public Int64 timeStamp;
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class MovePawn : Body
    {
        public Vector3 pos;
        public Vector3 velocity;
    }
}