﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CompoundFileStorage;
using MsgWriter.Exceptions;

namespace MsgWriter
{
    /// <summary>
    /// Contains a list of <see cref="Attachment"/> objects that are added to a message
    /// </summary>
    public sealed class Attachments : List<Attachment>
    {
        #region AddAttachment
        /// <summary>
        /// Checks if the <paramref name="fileName"/> already exists in this object
        /// </summary>
        /// <param name="fileName"></param>
        private void CheckAttachmentFileName(string fileName)
        {
            var file = Path.GetFileName(fileName);

            if (this.Any(
                attachment => attachment.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase)))
                throw new MWAttachmentExists("The attachment with the name '" + file + "' already exists");
        }

        /// <summary>
        /// Add's an attachment to the E-mail
        /// </summary>
        /// <param name="fileName">The file to add with full path</param>
        /// <param name="isInline">Set to true to add the attachment inline</param>
        /// <param name="contentId">The id for the inline attachment when <paramref name="isInline"/> is set to true</param>
        /// <exception cref="FileNotFoundException">Raised when the <paramref name="fileName"/> could not be found</exception>
        /// <exception cref="MWAttachmentExists">Raised when an attachment with the same name already exists</exception>
        /// <exception cref="ArgumentNullException">Raised when <paramref name="isInline"/> is set to true and
        /// <paramref name="contentId"/> is null, white space or empty</exception>
        public void AddAttachment(string fileName, bool isInline = false, string contentId = "")
        {
            CheckAttachmentFileName(fileName);
            var file = new FileInfo(fileName);
            Add(new Attachment(file.OpenRead(),
                               fileName,
                               file.CreationTime,
                               file.LastAccessTime,
                               isInline,
                               contentId));
        }

        /// <summary>
        /// Add's an attachment to the E-mail
        /// </summary>
        /// <param name="stream">The stream to the attachment</param>
        /// <param name="fileName">The name for the attachment</param>
        /// <param name="isInline">Set to true to add the attachment inline</param>
        /// <param name="contentId">The id for the inline attachment when <paramref name="isInline"/> is set to true</param>
        /// <exception cref="ArgumentNullException">Raised when the stream is null</exception>
        /// <exception cref="MWAttachmentExists">Raised when an attachment with the same name already exists</exception>
        /// <exception cref="ArgumentNullException">Raised when <paramref name="isInline"/> is set to true and
        /// <paramref name="contentId"/> is null, white space or empty</exception>
        public void AddAttachment(Stream stream, string fileName, bool isInline = false, string contentId = "")
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            CheckAttachmentFileName(fileName);
            var dateTime = DateTime.Now;
            Add(new Attachment(stream,
                               fileName,
                               dateTime,
                               dateTime,
                               isInline,
                               contentId));
        }
        #endregion
    }

    /// <summary>
    /// This class represents an Outlook attachment
    /// </summary>
    public sealed class Attachment
    {
        #region Properties
        /// <summary>
        /// The stream to the attachment
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// The filename of the attachment
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// The content id for an inline attachment
        /// </summary>
        public string ContentId { get; private set; }

        /// <summary>
        /// True when the attachment is inline
        /// </summary>
        public bool IsInline { get; private set; }

        /// <summary>
        /// Tthe date and time when the attachment was created
        /// </summary>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// The date and time when the attachment was last modified
        /// </summary>
        public DateTime LastModificationTime { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new attachment object and sets all its properties
        /// </summary>
        /// <param name="stream">The stream to the attachment</param>
        /// <param name="fileName">The attachment filename</param>
        /// <param name="creationTime">The date and time when the attachment was created</param>
        /// <param name="lastModificationTime">The date and time when the attachment was last modified</param>
        /// <param name="isInline">True when the attachment is inline</param>
        /// <param name="contentId">The id for the attachment when <paramref name="isInline"/> is set to true</param>
        /// <exception cref="ArgumentNullException">Raised when <paramref name="isInline"/> is set to true and
        /// <paramref name="contentId"/> is null, white space or empty</exception>
        internal Attachment(Stream stream, 
                            string fileName,
                            DateTime creationTime,
                            DateTime lastModificationTime,
                            bool isInline = false, 
                            string contentId = "")
        {
            Stream = stream;
            FileName = Path.GetFileName(fileName);
            CreationTime = creationTime;
            LastModificationTime = lastModificationTime;
            IsInline = isInline;
            ContentId = contentId;

            if (isInline && string.IsNullOrWhiteSpace(contentId))
                throw new ArgumentNullException("contentId", "The content id cannot be empty when isInline is set to true");
        }
        #endregion

        /// <summary>
        /// This method add's the attachment object to the given <paramref name="rootStorage"/>
        /// and it will set all the needed properties
        /// </summary>
        /// <param name="rootStorage"></param>
        internal void AddToStorage(CFStorage rootStorage)
        {
            var storage = rootStorage.AddStorage("__attach_version1.0_#" + i.ToString().PadLeft(8, '0'));
            var stream = storage.AddStream("__substg1.0_3001001F");
            stream.SetData(Encoding.UTF8.GetBytes(attachment.FileName));
            stream = storage.AddStream("__substg1.0_37010102");
            stream.SetData(attachment.Stream.ToByteArray());
        }
    }
}
