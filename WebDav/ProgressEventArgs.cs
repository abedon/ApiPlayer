// --------------------------------
// <copyright file="ProgressEventArgs.cs" company="Thomas Loehlein">
//     WebDavNet - A WebDAV client
//     Copyright (C) 2009 - Thomas Loehlein
//     This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.
//     This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//     You should have received a copy of the GNU General Public License along with this program; if not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Thomas Loehlein</author>
// <email>thomas.loehlein@gmail.com</email>
// ---------------------------------

using System;

namespace WebDav
{
	/// <summary>
	/// Class that contains all necessary Information about the current progress.
	/// </summary>
    public class ProgressEventArgs : EventArgs
    {
    	#region CONSTRUCTORS
    	/// <summary>
    	/// Initializes a new instance of the <see cref="ProgressEventArgs"/> class.
    	/// </summary>
    	/// <param name="isComplete">A flag that indicates if the action is completed.</param>
    	/// <param name="localFile">The local file.</param>
    	/// <param name="remoteUri">The remote Uri</param>
    	/// <param name="transferredBytes">A <see cref="long"/> of the transferred bytes.</param>
    	/// <param name="totalBytes">A <see cref="long"/> of the total bytes.</param>
        public ProgressEventArgs(bool isComplete, string localFile, Uri remoteUri, long transferredBytes, long totalBytes)
        {
            IsComplete = isComplete;
            LocalFile = localFile;
            RemoteUri = remoteUri;
            TransferredBytes = transferredBytes;
            TotalBytes = totalBytes;
        }
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Gets the transferred bytes.
        /// </summary>
        /// <value>The transferred bytes.</value>
        public long TransferredBytes
        { get; private set; }

        /// <summary>
        /// Gets the amount of total bytes.
        /// </summary>
        /// <value>The total bytes.</value>
        public long TotalBytes
        { get; private set; }

        /// <summary>
        /// Gets the local file.
        /// </summary>
        /// <value>The local file.</value>
        public string LocalFile
        { get; private set; }

        /// <summary>
        /// Gets the remote Uri.
        /// </summary>
        /// <value>The remote URI.</value>
        public Uri RemoteUri
        { get; private set; }

        /// <summary>
        /// Gets the flag that indicates if the action is completed or not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplete
        { get; private set; }
        #endregion
    }
}
