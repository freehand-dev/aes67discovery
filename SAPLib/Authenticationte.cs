using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SAPLib
{
    public enum AuthType
    {
        PGP = 0x00,
        CMS = 0x1,
    }

    public enum AuthVersion
    {
        v1 = 0x01, 
    }

    public class Authenticationte : IEnumerable<byte>
    {
        public byte Flag
        {
            get
            {
                byte flag = (byte)((((byte)this.Version) << 5) & 0xff);
                BitArray b = new BitArray(new byte[] { flag });
                b.Set(4, this.Padding);
                return flag;
            }
            set
            {
                this.Version = (AuthVersion)(((value) >> 5) & 0xff);
                BitArray b = new BitArray(new byte[] { value });
                this.Padding = b.Get(4);
                this.Type = b.Get(3) ? AuthType.CMS : AuthType.PGP;
            }
        }

        /// <summary>
        /// Version Number, V:  The version number of the authentication format
        /// specified by this memo is 1.
        /// </summary>
        public AuthVersion Version { get; set; }

        /// <summary>
        /// Padding Bit, P:  If necessary the authentication data is padded to be
        /// a multiple of 32 bits and the padding bit is set.In this case
        /// the last byte of the authentication data contains the number of
        /// padding bytes (including the last byte) that must be discarded.
        /// </summary>
        public bool Padding { get; set; }

        /// <summary>
        /// Authentication Type, Auth: The authentication type is a  4 bit
        /// encoded field that denotes the authentication infrastructure the
        /// sender expects the recipients to use to check the authenticity and
        /// integrity of the information.This defines the format of the
        /// authentication subheader and can take the values:  0 = PGP format,
        /// 1 = CMS format.All other values are undefined and SHOULD be
        /// ignored.
        /// </summary>
        public AuthType Type { get; set; }

        public byte[] SpecificSubheader { get; set; }

        public Authenticationte()
        {
            this.Version = AuthVersion.v1;
            this.Padding = false;
            this.Type = AuthType.PGP;
        }

        public byte[] ToBytes()
        {
            int place = 0;
            int size = Marshal.SizeOf(this.Flag) +
                ((this.SpecificSubheader != null) ? SpecificSubheader.Length : 0);

            byte[] buffer = new byte[size];

            // Flags
            Buffer.SetByte(buffer, place, this.Flag);
            place++;

            if (this.SpecificSubheader != null)
            {
                // MessageIdentifierHash (2byte)
                Buffer.BlockCopy(this.SpecificSubheader, 0, buffer, place, this.SpecificSubheader.Length);
                place += this.SpecificSubheader.Length;
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
