using EazyNotes.Common;
using EazyNotes.Models.DTO;
using EazyNotes.Models.POCO;
using EazyNotesDesktop.Library.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EazyNotesDesktop.Library.Transport
{
    /// <summary>
    /// Handles Im-/Export of notes from/to the local file system.
    /// </summary>
    public class FileSystemTransporter : DataTransporter
    {
        const string NOTE_HEADER_EXT = ".ennh";
        const string TOPIC_HEADER_EXT = ".enth";

        private readonly bool ImportSubdirectoriesAsSeparateTopics;

        public FileSystemTransporter(IUserData userData, string exportPath = "", bool importSubdirectoriesAsSeparateTopics = true) 
            : base(userData, exportPath) 
        {
            ImportSubdirectoriesAsSeparateTopics = importSubdirectoriesAsSeparateTopics;
            ExportTarget = Path.Join(ExportTarget, CreateExportFolderName());
        }

        /// <summary>
        /// Exports a single topic along with the given notes to the file system.
        /// </summary>
        public override void ExportTopic(Topic topic, List<AbstractNote> notes)
        {
            // TODO: 
            if (IsValidDirectory(ExportTarget))
            {
                string legalTopicTitle = PathUtil.RemoveInvalidChars(topic.Title);
                string topicPath = Path.Join(ExportTarget, legalTopicTitle);
                // In case a topic name occurs multiple times, append a number to output folder
                string uniqueTopicPath = topicPath;
                int i = 2;
                while (Directory.Exists(uniqueTopicPath))
                {
                    uniqueTopicPath = $"{topicPath}-{i}";
                    i++;
                }
                Directory.CreateDirectory(uniqueTopicPath);

                foreach (AbstractNote note in notes)
                {
                    string legalTitle = PathUtil.RemoveInvalidChars(note.Title);
                    string legalUniqueTitle = legalTitle;
                    string notePath = Path.Join(topicPath, legalTitle);
                    string uniqueNotePath = $"{notePath}.txt";
                    i = 2;
                    while (File.Exists(uniqueNotePath))
                    {
                        legalUniqueTitle = $"{legalTitle}-{i}";
                        uniqueNotePath = $"{Path.Join(topicPath, legalUniqueTitle)}.txt";
                        i++;
                    }
                    // write hidden header file, see https://stackoverflow.com/a/28003937/12213872
                    string noteHeaderFilePath = Path.Join(topicPath, $"{legalUniqueTitle}{NOTE_HEADER_EXT}");
                    AbstractNote headerOnlyNote = note.Clone() as AbstractNote;
                    headerOnlyNote.RemoveKeyAndDataFields();
                    string noteHeaderJson = JsonConvert.SerializeObject(headerOnlyNote);
                    File.WriteAllText(noteHeaderFilePath, noteHeaderJson);
                    File.SetAttributes(noteHeaderFilePath, File.GetAttributes(noteHeaderFilePath) | FileAttributes.Hidden);
                    // write content file
                    File.WriteAllText(uniqueNotePath, note.GetSerializedContent()) ;
                }

                // write hidden topic header file
                string topicHeaderFilePath = Path.Join(uniqueTopicPath, $"{legalTopicTitle}{TOPIC_HEADER_EXT}");
                Topic clone = topic.Clone() as Topic;
                clone.RemoveKeyAndDataFields();
                string topicHeaderJson = JsonConvert.SerializeObject(clone);
                File.WriteAllText(topicHeaderFilePath, topicHeaderJson);
                File.SetAttributes(topicHeaderFilePath, File.GetAttributes(topicHeaderFilePath) | FileAttributes.Hidden);
            }
        }

        public override ImportResult Import(List<string> fileOrFolderPaths, List<Topic> topics, TopicImportBehaviour topicImportBehaviour = TopicImportBehaviour.IfNameExistsUpdateTopic)
        {
            ImportResult importResult = new ImportResult();
            Topic topic;
            foreach (string fileOrFolderPath in fileOrFolderPaths)
            {
                string fileOrFolderName = Path.GetFileNameWithoutExtension(fileOrFolderPath);
                FileAttributes attr = File.GetAttributes(fileOrFolderPath);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    if (topicImportBehaviour != TopicImportBehaviour.ImportIntoSpecificTopic)
                    {
                        topic = topics.SingleOrDefault((topic) => topic.Title.Equals(fileOrFolderName));
                        if (topic == null)
                            topic = ImportTopicFromFolder(fileOrFolderPath, fileOrFolderName);
                        else
                        {
                            if (topicImportBehaviour == TopicImportBehaviour.AlwaysCreateNewTopic)
                            {
                                string baseTopicTitle = topic.Title;
                                int i = 2;
                                string uniqueTopicTitle = baseTopicTitle;
                                while (topic != null)
                                {
                                    uniqueTopicTitle = $"{baseTopicTitle}-{i}";
                                    topic = topics.SingleOrDefault(t => t.Title.Equals(uniqueTopicTitle));
                                    i++;
                                }
                                topic = ImportTopicFromFolder(fileOrFolderPath, uniqueTopicTitle);
                            }
                            else if (topicImportBehaviour == TopicImportBehaviour.IfNameExistsUpdateTopic && topic == null)
                                topic = ImportTopicFromFolder(fileOrFolderPath, fileOrFolderName);
                        }
                    }
                    else
                    {
                        if (topics.Count == 0)
                            topic = ImportTopicFromFolder(fileOrFolderPath, fileOrFolderName);
                        else topic = topics[0];
                    }
                    importResult.AddTopic(topic);
                    ImportResult irFromFolder = ImportNotesFromFolder(fileOrFolderPath);
                    importResult.MergeWith(irFromFolder);
                }
                else // i.e. if its a file, not folder
                {
                    string directoryName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(fileOrFolderPath));
                    topic = topics.SingleOrDefault((topic) => topic.Title.Equals(directoryName));
                    if (topic == null)
                        topic = ImportTopicFromFolder(Path.GetDirectoryName(fileOrFolderPath), directoryName);
                    importResult.AddNote(topic.Title, ImportNoteFromFile(fileOrFolderPath));
                }
            }
            importResult.RemoveEmptyTopics();
            return importResult;
        }

        private string CreateExportFolderName()
        {
            return $"EN_Export_{_userData.User.Username}_{DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(".", "").Replace(":", "")}";
        }

        private Topic ImportTopicFromFolder(string folderPath, string folderName)
        {
            Topic newTopic;
            try
            {
                string path = Path.Combine(folderPath, $"{folderName}{TOPIC_HEADER_EXT}");
                string topicHeaderJson = File.ReadAllText(path, Encoding.UTF8);
                newTopic = JsonConvert.DeserializeObject<Topic>(topicHeaderJson);
                newTopic.Title = folderName;
                newTopic.UserId = _userData.User.Id;
            }
            catch (Exception e)
            {
                newTopic = new Topic(_userData.User.Id, folderName);
            }
            // Replace potentially illegal symbol & color with defaults
            if (!Symbols.GetList.Contains(newTopic.Symbol))
                newTopic.Symbol = Symbols.DefaultTopicSymbol;
            if (!ColorRef.HasColor(newTopic.Color))
                newTopic.Color = ColorRef.DefaultTopicColor;
            return newTopic;
        }

        private ImportResult ImportNotesFromFolder(string basefolderpath)
        {
            // notes will be important from arbitrary depth after folder path; base refers to this root path
            string[] filepaths = Directory.GetFiles(basefolderpath, "*.*", SearchOption.AllDirectories)
                .Where(filepath => Path.GetExtension(filepath).Equals(".txt")).ToArray();
            ImportResult importResult = new ImportResult();
            foreach (string filepath in filepaths)
            {
                if (ImportSubdirectoriesAsSeparateTopics)
                {
                    string topicPath = Path.GetDirectoryName(filepath);
                    string topicParent = Path.GetDirectoryName(topicPath);
                    // If folder name contains '.', Path.GetFileNameWithoutExtension would remove that;
                    // Thus need to manually get the folder name
                    string topicTitle = new List<string>(topicPath
                        .Split(topicPath.Contains(Path.DirectorySeparatorChar)
                        ? Path.DirectorySeparatorChar : Path.AltDirectorySeparatorChar))
                        .Last();
                    while (topicParent != basefolderpath && topicParent.Length >= basefolderpath.Length)
                    {
                        string currDirectoryName = Path.GetFileNameWithoutExtension(topicPath);
                        topicParent = Path.GetDirectoryName(topicParent);
                        // TODO: Here the topic name can become almost indefinitely long => need to cap it to the
                        // maximum valid length for EasyNotes => refer to the modelvalidator; Just take the last N characters then.
                        topicTitle.Insert(0, $"{currDirectoryName}/");
                    }
                    if (!importResult.HasTopic(topicTitle))
                        importResult.AddTopic(ImportTopicFromFolder(topicPath, topicTitle));
                    importResult.AddNote(topicTitle, ImportNoteFromFile(filepath));
                }
                else
                    importResult.AddNote(Path.GetFileNameWithoutExtension(basefolderpath), ImportNoteFromFile(filepath));
            }
            return importResult;
        }

        private AbstractNote ImportNoteFromFile(string filepath)
        {
            AbstractNote note;
            NoteDTO noteHeader = null;
            string title = Path.GetFileNameWithoutExtension(filepath);
            string noteHeaderJson = null;
            try
            {
                noteHeaderJson = File.ReadAllText(Path.Combine(Path.GetDirectoryName(filepath), $"{title}{NOTE_HEADER_EXT}"), Encoding.UTF8);
                noteHeader = JsonConvert.DeserializeObject<NoteDTO>(noteHeaderJson);
            }
            catch { /* noteHeaderJson remains null -> handle below */ }
            string content = File.ReadAllText(filepath);
            string[] contentLines = content.Split(Environment.NewLine);
            if (noteHeader == null)
            {
                if (IsWellFormattedCheckListContent(contentLines) || noteHeader != null && noteHeader.NoteType == (short)NoteType.CheckList)
                {
                    note = new CheckListNote();
                    ((CheckListNote)note).CheckListItems = CheckListItem.FromArray(contentLines);
                }
                else
                {
                    note = new SimpleNote();
                    ((SimpleNote)note).Content = content;
                }
            }
            else // i.e. (noteHeader != null)
            {
                if (noteHeader.NoteType == (short)NoteType.CheckList)
                    note = noteHeader.ToCheckListNote(CheckListItem.FromArray(contentLines));
                else
                {
                    note = noteHeader.ToSimpleNote();
                    ((SimpleNote)note).Content = content;
                }
            }
            note.UserId = _userData.User.Id;
            note.Title = title;
            return note;
        }

        private bool IsWellFormattedCheckListContent(string[] lines)
        {
            // Well formatted Checklist content is content that consists only of lines that
            // follow this format:
            // - <text> // for unchecked checklist items
            // v <text> // for checked checklist items
            foreach (string line in lines)
            {
                string tabTrimmedLine = line.TrimStart('\t');
                if (!(tabTrimmedLine.StartsWith("- ") || tabTrimmedLine.StartsWith("v ")))
                    return false;
            }
            return true;
        }
    }
}
