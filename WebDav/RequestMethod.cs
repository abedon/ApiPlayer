// --------------------------------
// <copyright file="RequestMethod.cs" company="Thomas Loehlein">
//     WebDavNet - A WebDAV client
//     Copyright (C) 2009 - Thomas Loehlein
//     This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.
//     This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//     You should have received a copy of the GNU General Public License along with this program; if not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Thomas Loehlein</author>
// <email>thomas.loehlein@gmail.com</email>
// ---------------------------------

namespace WebDav
{
    /// <summary>
    /// All supported request methods.
    /// </summary>
    public enum RequestMethod
    {
        /// <summary>
        /// GET Method.
        /// </summary>
        Get,
        /// <summary>
        /// PUT Method.
        /// </summary>
        Put,
        /// <summary>
        /// MOVE Method.
        /// </summary>
        Move,
        /// <summary>
        /// PROPFIND Method.
        /// </summary>
        PropFind,
        /// <summary>
        /// MKCOL Method.
        /// </summary>
        MkCol,
        /// <summary>
        /// DELETE Method.
        /// </summary>
        Delete,
        /// <summary>
        /// COPY Method.
        /// </summary>
        Copy,
        /// <summary>
        /// HEAD Method.
        /// </summary>
        Head
    }
}
