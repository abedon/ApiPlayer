// --------------------------------
// <copyright file="AsyncHandler.cs" company="Thomas Loehlein">
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
	/// Abstract base class for all Asynchronous handler.
	/// </summary>
    public abstract class AsyncHandler
    {
        internal WebDavManager _webDavManager;

        internal AsyncHandler(WebDavManager webDavManager, Uri remoteUri, string localFile)
        {
            _webDavManager = webDavManager;

            RemoteUri = remoteUri;
            LocalFile = localFile;
        }

        /// <summary>
        /// Gets or sets the remote Uri.
        /// </summary>
        public Uri RemoteUri
        { get; private set; }

        /// <summary>
        /// Gets or sets the local file.
        /// </summary>
        public string LocalFile
        { get; private set; }

        /// <summary>
        /// Aborts the related asynchronous operation.
        /// </summary>
        public abstract void Abort();
    }

    /// <summary>
    /// Asynchronous handler for downloads.
    /// </summary>
    public class DownloadAsyncHandler : AsyncHandler
    {
        internal DownloadAsyncHandler(WebDavManager webDavManager, Uri remoteUri, string localFile)
            : base(webDavManager, remoteUri, localFile)
        {}

        /// <summary>
        /// Aborts the related asynchronous download.
        /// </summary>
        public override void Abort()
        {
            _webDavManager.AbortDownload(RemoteUri, LocalFile);
        }
    }

    /// <summary>
    /// Asynchronous handler for uploads.
    /// </summary>
    public class UploadAsyncHandler : AsyncHandler
    {
        internal UploadAsyncHandler(WebDavManager webDavManager, Uri remoteUri, string localFile)
            : base(webDavManager, remoteUri, localFile)
        {}

        /// <summary>
        /// Aborts the related asynchronous upload.
        /// </summary>
        public override void Abort()
        {
            _webDavManager.AbortUpload(RemoteUri, LocalFile);
        }
    }
}
