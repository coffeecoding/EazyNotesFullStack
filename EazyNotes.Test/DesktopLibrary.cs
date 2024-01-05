using NUnit.Framework;
using EazyNotesDesktop.Library.Common;

namespace EazyNotes.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void NoteOptions_AddFlagWhenOptionsNull()
        {
            string options = null;

            NoteOptions.Add(ref options, NoteOptions.WRAP_TEXT);

            Assert.AreEqual(options, $"{NoteOptions.WRAP_TEXT}{NoteOptions.SEPARATOR}");
        }

        [Test]
        public void NoteOptions_AddFlag()
        {
            string options = "";

            NoteOptions.Add(ref options, NoteOptions.WRAP_TEXT);

            Assert.AreEqual(options, $"{NoteOptions.WRAP_TEXT}{NoteOptions.SEPARATOR}");
        }

        [Test]
        public void NoteOptions_AddFlag_ExistingNotDoubled()
        {
            string options = $"{NoteOptions.WRAP_TEXT}{NoteOptions.SEPARATOR}";

            NoteOptions.Add(ref options, NoteOptions.WRAP_TEXT);

            Assert.AreEqual(options, $"{NoteOptions.WRAP_TEXT}{NoteOptions.SEPARATOR}");
        }

        [Test]
        public void NoteOptions_RemoveFlag()
        {
            string options = $"X{NoteOptions.SEPARATOR}{NoteOptions.WRAP_TEXT}{NoteOptions.SEPARATOR}Y{NoteOptions.SEPARATOR}";

            NoteOptions.Remove(ref options, NoteOptions.WRAP_TEXT);

            Assert.AreEqual(options, $"X{NoteOptions.SEPARATOR}Y{NoteOptions.SEPARATOR}");
        }

        [Test]
        public void NoteOptions_RemoveFlagWhenNull()
        {
            string options = null;

            NoteOptions.Remove(ref options, NoteOptions.WRAP_TEXT);

            Assert.AreEqual(options, null);
        }

        [Test]
        public void NoteOptions_RemoveFlag_NonExisting()
        {
            string options = $"X{NoteOptions.SEPARATOR}Y{NoteOptions.SEPARATOR}";

            NoteOptions.Remove(ref options, NoteOptions.WRAP_TEXT);

            Assert.AreEqual(options, $"X{NoteOptions.SEPARATOR}Y{NoteOptions.SEPARATOR}");
        }
    }
}