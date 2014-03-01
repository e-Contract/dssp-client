/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2014 Egelke BVBA
 *  Copyright (C) 2014 e-contract BVBA
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EContract.Dssp.Client
{
    public interface IDsspSessionStore
    {
        /// <summary>
        /// Loads the DsspSession with the provided id.
        /// </summary>
        /// <remarks>
        /// The implementation must always provide a valid DssSession, if not found
        /// it must throw an exception instead or returning null.
        /// </remarks>
        /// <param name="id">The ID of the DsspSession that must be loaded</param>
        /// <returns>The loaded DsspSession</returns>
        DsspSession Load(string id);

        /// <summary>
        /// Stores the session in an implementation specific way.
        /// </summary>
        /// <param name="session">The session to be stored, retreivable via its id</param>
        void Store(DsspSession session);

        void Remove(string id);
    }
}
