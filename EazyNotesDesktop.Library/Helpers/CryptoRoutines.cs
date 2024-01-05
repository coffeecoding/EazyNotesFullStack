using EazyNotes.Models.POCO;
using System.Collections.Generic;
using System;
using EazyNotes.CryptoServices;
using EazyNotes.Models.DTO;

namespace EazyNotesDesktop.Library.Helpers
{
    public interface ICryptoRoutines
    {
        TopicDTO EncryptTopic(Topic topic);
        Topic DecryptTopic(TopicDTO topic);
        NoteDTO EncryptNote(AbstractNote note, string rsaPubKeyXml = null);
        AbstractNote DecryptNote(NoteDTO note);
    }

    public class CryptoRoutines : ICryptoRoutines
    {
        private readonly IUserData _userSecrets;

        public CryptoRoutines(IUserData userSecrets)
        {
            _userSecrets = userSecrets;
        }

        private StatefulAES EnsureIVKeyIsSetAndCreateAES(Entity entity, String rsaPubKeyXml)
        {
            if (string.IsNullOrWhiteSpace(entity.IVKey))
            {
                StatefulAES aes = new StatefulAES();
                entity.IVKey = EncodeIVKey(aes, rsaPubKeyXml);
                return aes;
            }
            else
            {
                string privKeyXml = _userSecrets.GetRSAPrivateKeyXml();
                return DecodeIVKeyAndCreateAES(entity.IVKey, privKeyXml);
            }
        }

        private string EncodeIVKey(StatefulAES aes, string pubKeyXml)
        {
            byte[] IVKey = new byte[272];
            Array.Copy(aes.IV, 0, IVKey, 0, 16);
            byte[] encryptedAESKey = RSAHelper.Encrypt(aes.Key, pubKeyXml);
            Array.Copy(encryptedAESKey, 0, IVKey, 16, 256);
            return Convert.ToBase64String(IVKey);
        }

        private StatefulAES DecodeIVKeyAndCreateAES(string ivkey64, string privKeyXml)
        {
            byte[] IVKey = Convert.FromBase64String(ivkey64);
            byte[] IV = new byte[16];
            Array.Copy(IVKey, 0, IV, 0, 16);
            byte[] encryptedKey = new byte[256];
            Array.Copy(IVKey, 16, encryptedKey, 0, 256);
            byte[] decryptedKey = RSAHelper.Decrypt(encryptedKey, privKeyXml);
            StatefulAES aes = new StatefulAES(decryptedKey, IV);
            return aes;
        }

        public TopicDTO EncryptTopic(Topic topic)
        {
            if (topic == null) 
                return null;
            StatefulAES aes = EnsureIVKeyIsSetAndCreateAES(topic, 
                _userSecrets.GetRSAPublicKeyXml());
            TopicDTO topicDTO = topic.ToTopicDTO();
            topicDTO.Title = aes.EncryptToBase64(topic.Title);
            return topicDTO;
        }

        public Topic DecryptTopic(TopicDTO topic)
        {
            if (topic == null)
                return null;
            StatefulAES aes = DecodeIVKeyAndCreateAES(topic.IVKey, 
                _userSecrets.GetRSAPrivateKeyXml());
            Topic decryptedTopic = topic.ToTopic();
            decryptedTopic.Title = aes.DecryptFromBase64(topic.Title);
            return decryptedTopic;
        }

        public NoteDTO EncryptNote(AbstractNote note, string rsaPubKeyXml = null)
        {
            if (note == null)
                return null;
            return note is SimpleNote
                ? EncryptSimpleNote((SimpleNote)note, rsaPubKeyXml)
                : EncryptCheckListNote((CheckListNote)note, rsaPubKeyXml);
        }

        public AbstractNote DecryptNote(NoteDTO note)
        {
            if (note == null)
                return null;
            // In C# 8.0 this is the best one can do
            // C# 9.0 allows for a nice conditional expression here
            if (note.NoteType == (short)NoteType.SimpleNote)
                return DecryptSimpleNote(note);
            return DecryptAndDecodeCheckListNote(note);
        }

        private NoteDTO EncryptSimpleNote(SimpleNote note, string rsaPubKeyXml = null)
        {
            if (note == null)
                return null;
            bool isPubKey = rsaPubKeyXml != null;
            string rsaKey = isPubKey ? rsaPubKeyXml : _userSecrets.GetRSAPublicKeyXml();
            StatefulAES aes = EnsureIVKeyIsSetAndCreateAES(note, rsaKey);
            NoteDTO noteDTO = note.ToNoteDTO();
            noteDTO.Title = aes.EncryptToBase64(note.Title);
            noteDTO.Content = aes.EncryptToBase64(note.Content);
            return noteDTO;
        }

        private SimpleNote DecryptSimpleNote(NoteDTO note)
        {
            if (note == null)
                return null;
            SimpleNote decryptedItem = note.ToSimpleNote();
            StatefulAES aes = DecodeIVKeyAndCreateAES(note.IVKey, 
                _userSecrets.GetRSAPrivateKeyXml());
            decryptedItem.Title = aes.DecryptFromBase64(note.Title);
            decryptedItem.Content = aes.DecryptFromBase64(note.Content);
            return decryptedItem;
        }

        private NoteDTO EncryptCheckListNote(CheckListNote note, string rsaPubKeyXml = null)
        {
            if (note == null)
                return null;
            bool isPubKey = rsaPubKeyXml != null;
            string rsaKey = isPubKey ? rsaPubKeyXml : _userSecrets.GetRSAPrivateKeyXml();
            StatefulAES aes = EnsureIVKeyIsSetAndCreateAES(note, rsaKey);
            NoteDTO noteDTO = note.ToNoteDTO();
            noteDTO.Title = aes.EncryptToBase64(note.Title);
            List<CheckListItem> clones = note.CheckListItems.ConvertAll(i => i.Clone() as CheckListItem);
            clones.ForEach(i => i.Text = aes.EncryptToBase64(i.Text));
            noteDTO.Content = CheckListNote.CustomSerializeItems(clones);
            return noteDTO;
        }

        private CheckListNote DecryptAndDecodeCheckListNote(NoteDTO note)
        {
            if (note == null)
                return null;
            StatefulAES aes = DecodeIVKeyAndCreateAES(note.IVKey, _userSecrets.GetRSAPrivateKeyXml());
            List<CheckListItem> items = CheckListNote.CustomDeserializeItems(note.Content);
            items.ForEach(i => i.Text = aes.DecryptFromBase64(i.Text));
            CheckListNote decryptedNote = note.ToCheckListNote(items);
            decryptedNote.Title = aes.DecryptFromBase64(decryptedNote.Title);
            return decryptedNote;
        }
    }
}
