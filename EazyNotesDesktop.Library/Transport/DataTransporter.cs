using EazyNotes.Models.POCO;
using EazyNotesDesktop.Library.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace EazyNotesDesktop.Library.Transport
{
    public enum TopicImportBehaviour
    {
        IfNameExistsUpdateTopic,
        AlwaysCreateNewTopic, 
        ImportIntoSpecificTopic
    }

    public abstract class DataTransporter
    {
        protected readonly IUserData _userData;

        public string ExportTarget { get; set; }

        public DataTransporter(IUserData userData, string exportTarget) {
            _userData = userData;
            ExportTarget = exportTarget;
        }

        public abstract ImportResult Import(List<string> fileOrFolderPaths, List<Topic> topic, TopicImportBehaviour topicImportBehaviour);

        public abstract void ExportTopic(Topic topics, List<AbstractNote> notes);

        // TODO: Defer the exact message to upper layers, as they are subject
        // to localization. Only throw the appropriate type of exception without comments.
        // Define new Exceptions if needed, in Library.Common namespace.
        public static bool IsValidDirectory(string dir)
        {
            try
            {
                Path.GetFullPath(dir);
                if (!Path.IsPathFullyQualified(dir))
                    throw new NotFullyQualifiedPathException();
                if (Directory.Exists(dir))
                    return true;
                else Directory.CreateDirectory(dir);
                return true;
            } catch (Exception e)
            {
                if (e is PathTooLongException || e is UnauthorizedAccessException
                    || e is NotFullyQualifiedPathException)
                    throw;
                throw new InvalidPathException(e.Message);
            }
        }
    }
}
