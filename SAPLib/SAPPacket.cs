/*
 * Session Announcement Protocol
 * https://tools.ietf.org/html/rfc2974
 * 
 * Authors: Oleksandr Nazaruk <mail@freehand.com.ua>
 * 
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using SAPLib.Utils;

namespace SAPLib
{ 

    public enum Version
    {
        SAPv0 = 0x00,
        SAPv1 = 0x01,
        SAPv2 = 0x02,
    }

    public enum AdressType
    {
        IPv4 = 0x00,
        IPv6 = 0x01,
    }

    public enum Reserved
    {
        UnSet = 0x00,
        Set = 0x01,
    }

    public enum MessageType
    {
        Announcement = 0x00,
        Deletion = 0x01,
    }

    

    public class SapPacket : IEnumerable<byte>
    {
        readonly string mimeType = "application/sdp";

        /// <summary>
        /// Byte 0 : Version number V1  = 001      (3 bits)
        /// Address type IPv4/IPv6      = 0/1      (1 bit)
        /// Reserved                    = 0        (1 bit)
        /// Message Type ann/del        = 0/1      (1 bit)
        /// Encryption on/off           = 0/1      (1 bit)
        /// Compressed on/off           = 0/1      (1 bit) 
        /// </summary>
        public byte Flag
        {
            get
            {
                byte flag = (byte)((((byte)this.Version) << 5) & 0xff);
                BitArray b = new BitArray(new byte[] { flag });
                b.Set(4, this.AdressType == AdressType.IPv6);
                b.Set(3, this.Reserved == Reserved.Set);
                b.Set(2, this.MessageType == MessageType.Deletion);
                b.Set(1, this.Encryption);
                b.Set(0, this.Compression);
                return b.ToByte();
            }
            set
            {
                this.Version = (Version)(((value) >> 5) & 0xff);
                BitArray b = new BitArray(new byte[] { value });
                this.AdressType = b.Get(4) ? AdressType.IPv6 : AdressType.IPv4;
                this.Reserved = b.Get(3) ? Reserved.Set : Reserved.UnSet;
                this.MessageType = b.Get(2) ? MessageType.Deletion : MessageType.Announcement;
                this.Encryption = b.Get(1);
                this.Compression = b.Get(0);
            }
        }

        /// <summary>
        /// Packet raw data size
        /// </summary>
        public int Size
        {
            get
            {
                return this.ToBytes().Length;
            }
        }

        /// <summary>
        /// Version Number. The version number field MUST be set to 1 (SAPv2
        /// announcements which use only SAPv1 features are backwards
        /// compatible, those which use new features can be detected by other
        /// means, so the SAP version number doesn't need to change).
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Address type. If the A bit is 0, the originating source field
        /// contains a 32-bit IPv4 address.If the A bit is 1, the
        /// originating source contains a 128-bit IPv6 address.
        /// </summary>
        public AdressType AdressType { get; set; }

        /// <summary>
        /// Reserved. SAP announcers MUST set this to 0, SAP listeners MUST
        /// ignore the contents of this field.
        /// </summary>
        public Reserved Reserved { get; set; }

        /// <summary>
        ///  Message Type. If the T field is set to 0 this is a session
        ///  announcement packet, if 1 this is a session deletion packet.
        ///  
        /// If the packet is an announcement packet, the payload contains a
        /// session description.
        /// 
        /// If the packet is a session deletion packet, the payload contains a
        /// session deletion message.If the payload format is `application/sdp'
        /// the deletion message is a single SDP line consisting of the origin
        /// field of the announcement to be deleted.
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Encryption Bit. If the encryption bit is set to 1, the payload of
        /// the SAP packet is encrypted.If this bit is 0 the packet is not
        /// encrypted.  See section 7 for details of the encryption process.
        /// 
        /// </summary>
        public bool Encryption { get; set; }

        /// <summary>
        /// Compressed bit. If the compressed bit is set to 1, the payload is
        /// compressed using the zlib compression algorithm[3].  If the
        /// payload is to be compressed and encrypted, the compression MUST be
        /// performed first.
        /// </summary>
        public bool Compression { get; set; }

        /// <summary>
        /// Authentication Length. An 8 bit unsigned quantity giving the number
        /// of 32 bit words following the main SAP header that contain
        /// authentication data.If it is zero, no authentication header is
        /// present.
        /// </summary>
        public byte AuthenticationLength { get; set; }

        /// <summary>
        /// Message Identifier Hash. A 16 bit quantity that, used in combination
        /// with the originating source, provides a globally unique identifier
        /// indicating the precise version of this announcement.The choice
        /// of value for this field is not specified here, except that it MUST
        /// be unique for each session announced by a particular SAP announcer
        /// and it MUST be changed if the session description is modified (and
        /// a session deletion message SHOULD be sent for the old version of
        /// the session).
        /// </summary>
        public ushort MessageIdentifierHash { get; set; }

        /// <summary>
        ///  Originating Source. This gives the IP address of the original source
        ///  of the message.This is an IPv4 address if the A field is set to
        ///  zero, else it is an IPv6 address.The address is stored in
        ///  network byte order.
        ///
        ///  SAPv0 permitted the originating source to be zero if the message
        ///  identifier hash was also zero.This practise is no longer legal,
        ///  and SAP announcers SHOULD NOT set the originating source to zero.
        ///  SAP listeners MAY silently discard packets with the originating
        ///  source set to zero.
        /// </summary>
        public IPAddress OriginatingSource { get; set; }

        /// <summary>
        /// Authentication data containing a digital signature of the packet,
        /// with length as specified by the authentication length header
        ///  field.See section 8 for details of the authentication process.
        /// </summary>
        public Authenticationte AuthenticationData { get; set; }

        /// <summary>
        /// The payload type field is a MIME content type specifier, describing
        /// the format of the payload.This is a variable length ASCII text
        /// string, followed by a single zero byte (ASCII NUL).  The payload type
        /// SHOULD be included in all packets.If the payload type is
        /// `application/sdp' both the payload type and its terminating zero byte
        /// MAY be omitted, although this is intended for backwards compatibility
        /// with SAP v1 listeners only.
        /// </summary>
        public string PayloadType { get; set; }

        public StringBuilder Payload { get; set; }

        public SapPacket()
        {
            this.Version = Version.SAPv1;
            this.Reserved = Reserved.UnSet;
            this.AdressType = AdressType.IPv4;
            this.MessageType = MessageType.Announcement;
            this.Encryption = false;
            this.Compression = false;
            this.AuthenticationLength = 0;

            Random rand = new Random();
            this.MessageIdentifierHash = (ushort)rand.Next(ushort.MaxValue);

            this.OriginatingSource = IPAddress.Parse("127.0.0.1");
            this.PayloadType = this.mimeType;

            this.Payload = new StringBuilder();
        }

        /// <summary>
        /// Attach SDP text
        /// </summary>
        /// <param name="sdp"></param>
        public void AttachSDP(StringBuilder sdp)
        {
            this.PayloadType = this.mimeType;
            this.Payload = sdp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <example>
        /// AttachSDP(new FileInfo(fileName));
        /// </example>
        public void AttachSDP(FileInfo file)
        {
            using (StreamReader reader = file.OpenText())
            {
                string sdpContent = reader.ReadToEnd();
                AttachSDP(new StringBuilder(sdpContent));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void AttachSDP(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                string sdpContent = reader.ReadToEnd();
                AttachSDP(new StringBuilder(sdpContent));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <example>
        /// AttachSDP(new FileStream(fileName, FileMode.Open));
        /// </example>
        public void AttachSDP(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string sdpContent = reader.ReadToEnd();
                AttachSDP(new StringBuilder(sdpContent));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            int place = 0;
             
            int size = Marshal.SizeOf(this.Flag) +
                Marshal.SizeOf(this.AuthenticationLength) +
                Marshal.SizeOf(this.MessageIdentifierHash) +
                ((this.AdressType == AdressType.IPv4) ? 4 : 16) +
                ((this.AuthenticationData != null) ? this.AuthenticationData.ToBytes().Length : 0) +
                (this.PayloadType.Length + 1) + 
                ((this.Payload != null) ? this.Payload.Length : 0);

            byte[] buffer = new byte[size];

            // Flags
            Buffer.SetByte(buffer, place, this.Flag);
            place++;

            // AuthLength (1byte)
            Buffer.SetByte(buffer, place, this.AuthenticationLength);
            place++;

            // MessageIdentifierHash (2byte)
            Buffer.BlockCopy(BitConverter.GetBytes(this.MessageIdentifierHash) , 0, buffer, place, 2);
            place += 2;


            // OriginatingSource (4byte) or (16byte)
            byte[] address = this.OriginatingSource.GetAddressBytes();
            Buffer.BlockCopy(address, 0, buffer, place, address.Length);
            place += address.Length;

            // AuthenticationData
            if (this.AuthenticationData != null)
            {
                byte[] authData = this.AuthenticationData.ToBytes();
                Buffer.BlockCopy(authData, 0, buffer, place, authData.Length);
                place += authData.Length;
            }

            // PayloadType (string) + 0x00;
            if (this.PayloadType.Length > 0)
            {
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(this.PayloadType), 0, buffer, place, this.PayloadType.Length);
                place += this.PayloadType.Length;
                Buffer.SetByte(buffer, place, 0x00);
                place++;
            }

            // Payload (dynamic)
            if (this.Payload != null)
            {
                var payload = Encoding.ASCII.GetBytes(this.Payload.ToString());
                Buffer.BlockCopy(payload, 0, buffer, place, payload.Length);
                place += payload.Length;
            }
                
            return buffer;
        }

        #region Public Methods
        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var b in this.ToBytes())
                yield return b;
        }

        public byte[] ToArray()
        {
            return this.ToBytes();
        }

        public override string ToString()
        {
            return BitConverter.ToString(this.ToBytes());
        }

        #endregion

        #region Explicit Interface Implementations
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
