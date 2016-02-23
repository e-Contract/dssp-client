/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2014 Egelke BVBA
 *  Copyright (C) 2014 e-Contract.be BVBA
 *
 *  DSS-P client is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  DSS-P client is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with DSS-P client.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using System.Security.Cryptography;

namespace EContract.Dssp.Client
{
    internal sealed class Psha1DerivedKeyGenerator
    {
        public byte[] Key;

        public Psha1DerivedKeyGenerator(byte[] key)
        {
            this.Key = key;
        }

        public byte[] GenerateDerivedKey(byte[] nonce, int derivedKeySize)
        {
            return new Psha1DerivedKeyGenerator.ManagedPsha1(this.Key, nonce).GetDerivedKey(derivedKeySize);
        }

        private sealed class ManagedPsha1
        {
            private KeyedHashAlgorithm hmac;
            private byte[] secret;
            private byte[] nonce;

            public ManagedPsha1(byte[] secret, byte[] nonce)
            {
                this.secret = secret;
                this.nonce = nonce;
                this.hmac = new HMACSHA1(secret);
            }

            public byte[] GetDerivedKey(int derivedKeySize)
            {
                int offset = 0;
                int required = derivedKeySize / 8;

                byte[] a = nonce;
                byte[] buffer = new byte[required];

                while (required > 0) {
                    hmac.Initialize();
                    hmac.TransformFinalBlock(a, 0, a.Length);
                    a = hmac.Hash;
                    hmac.Initialize();
                    hmac.TransformBlock(a, 0, a.Length, a, 0);
                    hmac.TransformFinalBlock(nonce, 0, nonce.Length);
                    int tocpy = required < hmac.Hash.Length ? required : hmac.Hash.Length;
                    Array.Copy(hmac.Hash, 0, buffer, offset, tocpy);
                    offset += tocpy;
                    required -= tocpy;
                }

                return buffer;
            }
        }
    }
}
